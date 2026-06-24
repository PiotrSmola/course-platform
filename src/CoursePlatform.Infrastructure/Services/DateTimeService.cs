using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime NowUtc => DateTime.UtcNow;
}

public interface IDateTimeService
{
    DateTime NowUtc { get; }
}
