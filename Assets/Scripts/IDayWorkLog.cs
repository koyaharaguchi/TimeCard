using System;

public interface IDayWorkLog : IReadOnlyDayWorkLog
{
    void AddLog(Status status);
}
public interface IReadOnlyDayWorkLog
{
    bool CanAdd(Status next);
    TimeSpan GetBreakTime();
    DateTime? GetEndTime();
    DateTime? GetStartTime();
    TimeSpan GetSumTime();
    TimeSpan GetWorkingTime();
    bool IsUsing();
    string ToString();
}