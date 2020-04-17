using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
public class TextFileCenter : IDayWorkLogStream
{
    public IDayWorkLog GetTodayDayWork(string folderpath)
    {
        var filepath = GetFilePath(folderpath);
        var enc = Encoding.GetEncoding("Shift_JIS");
        if (File.Exists(filepath))
        {
            using (var reader = new StreamReader(filepath, enc))
            {
                return DayWorkLog.Parse((reader.ReadToEnd()));
            }
        }
        return new DayWorkLog(new List<ILog>());
    }

    public void WritingLog(string folderpath, IReadOnlyDayWorkLog logs)
    {
        if (!Directory.Exists(folderpath))
        {
            Directory.CreateDirectory(folderpath);
        }

        var filepath = GetFilePath(folderpath);
        var enc = Encoding.GetEncoding("Shift_JIS");
        using (var writer = new StreamWriter(filepath, false, enc))
        {
            writer.Write(logs);
        }

    }

    private string GetFilePath(string folderpath)
    {
        var Yesterdayfilepath = folderpath + @"\" + $"{DateTime.Today.AddDays(-1).ToString(Format.Day)}.txt";
        if (IsUsing(Yesterdayfilepath))
        {
            return Yesterdayfilepath;
        }
        return folderpath + @"\" + $"{DateTime.Today.ToString(Format.Day)}.txt";
    }


    private bool IsUsing(string filepath)
    {
        var enc = Encoding.GetEncoding("Shift_JIS");
        if (!File.Exists(filepath))
            return false;

        using (var reader = new StreamReader(filepath, enc))
        {
            var dayWorkLog = DayWorkLog.Parse(reader.ReadToEnd());
            return dayWorkLog.IsUsing();
        }
    }
}
