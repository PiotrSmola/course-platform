using Microsoft.EntityFrameworkCore;

namespace CoursePlatform.Infrastructure.UnitTests.Common;

public sealed class PostgreSqlTestFixture : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=db;Port=5432;Database=courseplatform_tests;Username=postgres;Password=postgres";

    public string GetConnectionString() => ConnectionString;

    public TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new TestDbContext(options);
    }

    public async Task ResetAsync()
    {
        await using var context = CreateContext();
        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE \"Enrollments\", \"LessonProgresses\", \"Reviews\", \"Modules\", \"Lessons\", \"Courses\", \"Payments\", \"ProcessedStripeEvents\", \"Users\" RESTART IDENTITY CASCADE;");
    }

    public async Task InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }
}