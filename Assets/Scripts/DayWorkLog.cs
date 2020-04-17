using System;
using System.Collections.Generic;
using System.Linq;
public class DayWorkLog : IDayWorkLog
{
    private List<ILog> _Logs;

    public DayWorkLog(IReadOnlyList<ILog> logs)
    {
        _Logs = new List<ILog>(logs);
    }

    public static DayWorkLog Parse(string allLine)
    {
        var Lines = allLine.Split('\n');
        var logs = new List<ILog>();
        foreach (var line in Lines)
        {
            var log = Log.Parse(line);
            if (log != null)
            {
                logs.Add(log);
            }
        }
        logs = logs.OrderBy(value => value.dateTime).ToList();
        return new DayWorkLog(logs);
    }
    public DateTime? GetStartTime()
    {
        if (_Logs.Count == 0)
            return null;

        return _Logs[0].dateTime;
    }
    public DateTime? GetEndTime()
    {
        if (_Logs[_Logs.Count - 1].status == Status.退勤)
        {
            return _Logs[_Logs.Count - 1].dateTime;
        }

        return null;
    }
    public TimeSpan GetWorkingTime()
    {
        var Logs = new List<IReadOnlyLog>(_Logs);
        if (Logs.Count % 2 != 0)
        {
            Logs.Add(new Log(Status.退勤, DateTime.Now));
        }
        TimeSpan WorkingTime = new TimeSpan();
        for (int i = 0; i < Logs.Count - 1; i++)
        {
            var timeSpan = Logs[i + 1].dateTime - Logs[i].dateTime;
            if (i % 2 == 0)
            {
                WorkingTime += timeSpan;
            }
        }
        return WorkingTime;
    }
    public TimeSpan GetBreakTime()
    {
        var Logs = new List<IReadOnlyLog>(_Logs);
        if (Logs.Count % 2 != 0)
        {
            Logs.Add(new Log(Status.退勤, DateTime.Now));
        }
        TimeSpan BreakTime = new TimeSpan();
        for (int i = 0; i < Logs.Count - 1; i++)
        {
            var timeSpan = Logs[i + 1].dateTime - Logs[i].dateTime;
            if (i % 2 != 0)
            {
                BreakTime += timeSpan;
            }
        }
        return BreakTime;
    }
    public TimeSpan GetSumTime()
    {
        return GetWorkingTime() + GetBreakTime();
    }
    public void AddLog(Status status)
    {
        if (!CanAdd(status))
            return;

        var Log = new Log(status, DateTime.Now);
        if (_Logs.Count > 0 && getNowStatus() == Status.退勤)
        {
            _Logs[_Logs.Count - 1].setStatus(Status.休入);
            Log.setStatus(Status.休終);
        }
        _Logs.Add(Log);
        _Logs = _Logs.OrderBy(value => value.dateTime).ToList();

    }
    public bool CanAdd(Status next)
    {
        if (_Logs.Count == 0)
        {
            if (next == Status.出勤)
                return true;
            return false;
        }

        switch (getNowStatus())
        {
            case Status.休入:
                if (next == Status.休終)
                    return true;
                break;
            case Status.休終:
                if (next == Status.休入 || next == Status.退勤)
                    return true;
                break;
            case Status.出勤:
                if (next == Status.休入 || next == Status.退勤)
                    return true;
                break;
            case Status.退勤:
                if (next == Status.出勤)
                    return true;
                break;
        }
        return false;
    }

    private Status getNowStatus()
    {
        return _Logs[_Logs.Count - 1].status;
    }
    public bool IsUsing()
    {
        if (_Logs.Count == 0)
            return false;

        var logs = new List<ILog>(_Logs);
        if (logs[logs.Count - 1].status == Status.退勤)
        {
            return false;
        }
        foreach (var item in _Logs.Select((log, index) => new { status = log.status, i = index }))
        {
            if (item.i == 0)
            {
                if (item.status != Status.出勤)
                    return false;
            }
            else
            {
                if (item.i % 2 == 1)
                {
                    if (item.status != Status.休入)
                        return false;
                }
                else
                {
                    if (item.status != Status.休終)
                        return false;
                }
            }
        }
        return true;
    }
    public override string ToString()
    {
        string text = "";
        foreach (var log in _Logs)
        {
            text += $"{log.ToString()}\n";
        }
        text += $"勤務時間 {GetSumTime().ToString(Format.TimeSpan)}\n";
        text += $"労働時間 {GetWorkingTime().ToString(Format.TimeSpan)}\n";
        text += $"休憩時間 {GetBreakTime().ToString(Format.TimeSpan)}\n";
        return text;
    }
}

