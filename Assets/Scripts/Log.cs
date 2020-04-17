using System;
using System.Globalization;

public class Log : ILog
{
    public Status status { private set; get; }
    public DateTime dateTime { private set; get; }

    public override string ToString()
    {
        return $"{status.ToString()} {dateTime.ToString(Format.DayTime)}";
    }
    public Log(Status status, DateTime dateTime)
    {
        this.status = status;
        this.dateTime = dateTime;
    }
    public static Log Parse(string text)
    {
        var status = StatusExtract(text);
        if (status == null)
        {
            return null;
        }
        var dateTime = DateTimeExtract(text, status);
        if (dateTime == null)
        {
            return null;
        }
        return new Log((Status)status, dateTime);
    }

    private static Status? StatusExtract(string text)
    {
        foreach (Status value in Enum.GetValues(typeof(Status)))
        {
            var name = value.ToString();
            if (text.Contains(name))
            {
                return value;
            }
        }
        return null;
    }
    private static DateTime DateTimeExtract(string text, Status? status)
    {
        text = text.Replace(status.ToString(), "");
        text = text.Trim();
        CultureInfo ci = CultureInfo.CurrentCulture;
        DateTimeStyles dts = DateTimeStyles.None;
        DateTime dateTime;
        DateTime.TryParseExact(text, Format.DayTime, ci, dts, out dateTime);
        return dateTime;
    }

    public void setStatus(Status status)
    {
        this.status = status;
    }

}
