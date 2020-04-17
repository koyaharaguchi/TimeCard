using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using System.IO;
public class ExcelFileCenter : IDayWorkLogStream
{
    enum Columns
    {
        日付 = 2,
        曜日,
        出勤,
        退勤,
        勤務時間,
        休憩時間,
        労働時間,
        詳細,
    }
    public void WritingLog(string filepath, IReadOnlyDayWorkLog Logs)
    {
        filepath += ".xlsx";

        if (!File.Exists(filepath))
        {
            CreateExcelFile(filepath);
        }

        using (var workbook = new XLWorkbook(filepath))
        {
            AddData(Logs, workbook);
            workbook.Save();
        }

    }
    public IDayWorkLog GetTodayDayWork(string filepath)
    {
        filepath += ".xlsx";
        if (!File.Exists(filepath))
        {
            return new DayWorkLog(new List<ILog>());
        }
        using (var workbook = new XLWorkbook(filepath))
        {
            var yesterday = DateTime.Today.AddDays(-1);
            var YesterdayWork = GetDayWorkLog(workbook, yesterday);
            if (YesterdayWork != null && YesterdayWork.IsUsing())
                return YesterdayWork;

            var TodayWork = GetDayWorkLog(workbook, DateTime.Today);
            if (TodayWork != null)
                return TodayWork;

            return new DayWorkLog(new List<ILog>());
        }

    }
    private IDayWorkLog GetDayWorkLog(XLWorkbook workbook, DateTime date)
    {
        if (!workbook.Worksheets.Any(w => w.Name == date.ToString(Format.Month)))
            return null;

        var MonthLastRowUsed = workbook.Worksheet(date.ToString(Format.Month)).LastRowUsed();
        var dayWorkLog = GetDayWorkLog(MonthLastRowUsed);

        if (dayWorkLog.GetStartTime()?.Date != date.Date)
            return null;

        return dayWorkLog;
    }

    private IDayWorkLog GetDayWorkLog(IXLRow row)
    {
        var logs = row.Cell((int)Columns.詳細).Value.ToString();
        return DayWorkLog.Parse(logs);
    }


    private void CreateExcelFile(string filepath)
    {
        using (var workbook = new XLWorkbook())
        {
            CreateWorkSheet(workbook);
            workbook.SaveAs(filepath);
        }
    }

    private IXLWorksheet CreateWorkSheet(IXLWorkbook workbook)
    {
        var worksheet = workbook.AddWorksheet($"{DateTime.Today.ToString(Format.Month)}");
        CreateTable(worksheet);
        return worksheet;
    }

    private void CreateTable(IXLWorksheet worksheet)
    {
        CreateColumns(worksheet);
        worksheet.Range("B2:C2").Merge().Value = "合計勤務時間";
        worksheet.Range("B3:C3").Merge().Value = "合計労働時間";
        worksheet.Cell("D2").FormulaA1 = "=SUM(F:F)";
        worksheet.Cell("D3").FormulaA1 = "=SUM(H:H)";
        worksheet.Range("D2", "D3").Style.NumberFormat.Format = Format.ExcelTimeSpan;
        var cells = worksheet.Range("B2", "D3").Cells();
        cells.ToList().ForEach(c => c.WorksheetRow().Height = 19);
        cells.Style.Font.FontSize = 14;
        SetBorder(cells);
        cells.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    private void CreateColumns(IXLWorksheet worksheet)
    {
        var rowAdress = 5;
        foreach (Columns column in Enum.GetValues(typeof(Columns)))
        {
            worksheet.Cell(rowAdress, (int)column).Value = column.ToString();

            if (column == Columns.勤務時間 || column == Columns.労働時間 || column == Columns.休憩時間)
                worksheet.Column((int)column).Width = 12;

            if (column == Columns.詳細)
                worksheet.Column((int)column).Width = 6;
        }
        var columncells = worksheet.Row(rowAdress).CellsUsed();
        columncells.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        columncells.Style.Fill.BackgroundColor = XLColor.FromHtml("#f0ffff");
        SetBorder(columncells);
    }

    private void AddData(IReadOnlyDayWorkLog Logs, IXLWorkbook workbook)
    {
        var worksheet = GetWorkSheet(Logs, workbook);
        var row = GetRow(Logs, worksheet);
        WritingRow(Logs, row);
    }
    private IXLWorksheet GetWorkSheet(IReadOnlyDayWorkLog logs, IXLWorkbook workbook)
    {
        var ThisMonthworksheet = workbook.Worksheets.FirstOrDefault(w => w.Name == logs.GetStartTime()?.ToString(Format.Month));
        if (ThisMonthworksheet != null)
            return ThisMonthworksheet;
        return CreateWorkSheet(workbook);
    }
    private IXLRow GetRow(IReadOnlyDayWorkLog logs, IXLWorksheet worksheet)
    {
        if (logs.GetStartTime() == null)
            return worksheet.LastRowUsed().RowBelow();

        var LastRowUsed = worksheet.LastRowUsed();
        if (GetDayWorkLog(LastRowUsed).GetStartTime()?.Date == logs.GetStartTime()?.Date)
            return LastRowUsed;

        return LastRowUsed.RowBelow();
    }
    private void WritingRow(IReadOnlyDayWorkLog Logs, IXLRow row)
    {
        WritingDay(Logs, row);
        WrtingDayOfWeak(row);
        WritingStartTime(Logs, row);
        WritingEndTime(Logs, row);
        WritingSumTime(row);
        WritingBreakTime(Logs, row);
        WritingWorkingTime(row);
        WritingLog(Logs, row);
        row.CellsUsed().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        SetBorder(row.CellsUsed());
    }

    private void WritingLog(IReadOnlyDayWorkLog Logs, IXLRow row)
    {
        var cell = row.Cell((int)Columns.詳細);
        cell.Style.NumberFormat.Format = ";;;ログ";
        cell.Value = Logs.ToString();
    }

    private void WritingWorkingTime(IXLRow row)
    {
        var sumtime = GetCellAdress(Columns.勤務時間, row);
        var breaktime = GetCellAdress(Columns.休憩時間, row);

        var cell = row.Cell((int)Columns.労働時間);
        cell.FormulaA1 = $"=IFERROR(({sumtime}-{breaktime}),0)";
        cell.Style.NumberFormat.Format = Format.ExcelTimeSpan;
    }

    private void WritingBreakTime(IReadOnlyDayWorkLog Logs, IXLRow row)
    {
        var cell = row.Cell((int)Columns.休憩時間);
        cell.Style.NumberFormat.Format = Format.ExcelTimeSpan;
        cell.Value = Logs.GetBreakTime().ToString(Format.TimeSpan);
    }

    private void WritingSumTime(IXLRow row)
    {
        var StartTime = GetCellAdress(Columns.出勤, row);
        var EndTime = GetCellAdress(Columns.退勤, row);

        var cell = row.Cell((int)Columns.勤務時間);
        cell.FormulaA1 = $"=IFERROR(({EndTime}-{StartTime}),0)";
        cell.Style.NumberFormat.Format = Format.ExcelTimeSpan;
    }

    private void WritingEndTime(IReadOnlyDayWorkLog Logs, IXLRow row)
    {
        var cell = row.Cell((int)Columns.退勤);
        cell.Style.NumberFormat.Format = Format.Time;
        cell.Value = Logs.GetEndTime()?.ToString() ?? "-----";
    }

    private void WritingStartTime(IReadOnlyDayWorkLog Logs, IXLRow row)
    {
        var cell = row.Cell((int)Columns.出勤);
        cell.Style.NumberFormat.Format = Format.Time;
        cell.Value = Logs.GetStartTime()?.ToString() ?? "-----";
    }

    private void WrtingDayOfWeak(IXLRow row)
    {
        var Day = GetCellAdress(Columns.日付, row);

        var cell = row.Cell((int)Columns.曜日);
        cell.FormulaA1 = $@"=TEXT({Day},""aaa"")";
    }

    private void WritingDay(IReadOnlyDayWorkLog Logs, IXLRow row)
    {
        var cell = row.Cell((int)Columns.日付);
        cell.Style.NumberFormat.Format = Format.Day2;
        cell.Value = Logs.GetStartTime()?.ToString(Format.Day2) ?? "-----";
    }

    private void SetBorder(IXLCells cells)
    {
        cells.Style.
            Border.SetBottomBorder(XLBorderStyleValues.Thin).
            Border.SetTopBorder(XLBorderStyleValues.Thin).
            Border.SetLeftBorder(XLBorderStyleValues.Thin).
            Border.SetRightBorder(XLBorderStyleValues.Thin);
    }
    private string GetCellAdress(Columns columns, IXLRow row)
    {
        return row.Cell((int)columns).Address.ToString();

    }


}

