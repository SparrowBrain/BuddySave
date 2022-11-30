namespace BuddySave.System;

internal class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now()
    {
        return DateTime.Now;
    }
}