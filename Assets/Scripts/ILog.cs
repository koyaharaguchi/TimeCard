using System;

public interface IReadOnlyLog
{
    DateTime dateTime { get; }
    Status status { get; }
    string ToString();
}
public interface ILog :IReadOnlyLog
{
    void setStatus(Status status);
}