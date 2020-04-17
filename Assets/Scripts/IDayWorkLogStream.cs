public interface IDayWorkLogStream
{
    void WritingLog(string path, IReadOnlyDayWorkLog logs);
    IDayWorkLog GetTodayDayWork(string path);
}
