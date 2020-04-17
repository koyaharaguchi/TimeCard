using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class DateManager : IDateManager
{
    private IDayWorkLogStream ExcelCenter = new ExcelFileCenter();
    public string Path { get; private set; }
    private IDayWorkLog TodayWorkLog;
    public DateManager(string path)
    {
        Path = path;
        TodayWorkLog = ExcelCenter.GetTodayDayWork(GetPath());
    }

    public async void SendStatus(Status status)
    {
        if (!TodayWorkLog.CanAdd(status))
            return;

        StartDateCheck(status);
        TodayWorkLog.AddLog(status);
        await Task.Run(() => ExcelCenter.WritingLog(GetPath(), TodayWorkLog));
    }

    private void StartDateCheck(Status status)
    {
        if (status == Status.出勤 && TodayWorkLog.GetStartTime() != null && TodayWorkLog.GetStartTime()?.Date != DateTime.Today.Date)
        {
            TodayWorkLog = new DayWorkLog(new List<ILog>());
        }
    }

    public bool CanPassStatus(Status status)
    {
        return TodayWorkLog.CanAdd(status);
    }
    public IReadOnlyDayWorkLog GetToDayWorkLog()
    {
        return TodayWorkLog;
    }
    private string GetPath()
    {
        return Path;
    }
}


