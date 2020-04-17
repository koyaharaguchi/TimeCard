public interface IDateManager
{
    bool CanPassStatus(Status status);
    IReadOnlyDayWorkLog GetToDayWorkLog();
    void SendStatus(Status status);
    string Path { get; }
}