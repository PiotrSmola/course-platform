using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Admin.Search;
using CoursePlatform.Infrastructure.Search;
using CoursePlatform.Infrastructure.UnitTests.Common;

namespace CoursePlatform.Infrastructure.UnitTests.Search;

public class ReindexJobServiceTests
{
    [Fact]
    public async Task StartAsync_WhenCalled_SetsStateToRunning()
    {
        var sp = BuildServiceProvider();
        var service = sp.GetRequiredService<IReindexJobService>();

        var result = await service.StartAsync(CancellationToken.None);

        result.State.Status.Should().Be(ReindexStatus.Running);
        result.AlreadyRunning.Should().BeFalse();
        result.State.StartedAt.Should().NotBeNull();
        result.State.JobId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task StartAsync_WhenAlreadyRunning_ReturnsAlreadyInProgress()
    {
        var sp = BuildServiceProvider();
        var service = sp.GetRequiredService<IReindexJobService>();

        var first = await service.StartAsync(CancellationToken.None);
        var second = await service.StartAsync(CancellationToken.None);

        second.AlreadyRunning.Should().BeTrue();
        second.State.JobId.Should().Be(first.State.JobId);
    }

    [Fact]
    public void GetCurrent_BeforeStart_ReturnsNull()
    {
        var sp = BuildServiceProvider();
        var service = sp.GetRequiredService<IReindexJobService>();

        var current = service.GetCurrent();

        current.Should().BeNull();
    }

    [Fact]
    public async Task StartAsync_WithThrowingIndexing_MarksJobAsFailed()
    {
        var sp = BuildServiceProvider(useThrowing: true);
        var service = sp.GetRequiredService<IReindexJobService>();

        await service.StartAsync(CancellationToken.None);

        var state = await WaitForTerminalStateAsync(service);

        state.Status.Should().Be(ReindexStatus.Failed);
        state.FinishedAt.Should().NotBeNull();
        state.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task StartAsync_WithStubIndexing_CompletesSuccessfully()
    {
        var sp = BuildServiceProvider();
        var service = sp.GetRequiredService<IReindexJobService>();

        await service.StartAsync(CancellationToken.None);

        var state = await WaitForTerminalStateAsync(service);

        state.Status.Should().Be(ReindexStatus.Succeeded);
        state.Phase.Should().Be(ReindexPhase.Done);
        state.Logs.Should().Contain(l => l.Message.Contains("completed successfully"));
    }

    private static async Task<ReindexJobState> WaitForTerminalStateAsync(IReindexJobService service)
    {
        for (var i = 0; i < 100; i++)
        {
            var current = service.GetCurrent();
            if (current is { Status: ReindexStatus.Succeeded or ReindexStatus.Failed or ReindexStatus.Cancelled })
            {
                return current;
            }
            await Task.Delay(20);
        }
        throw new TimeoutException("Job did not reach terminal state in time.");
    }

    private static IServiceProvider BuildServiceProvider(bool useThrowing = false)
    {
        var services = new ServiceCollection();

        var dbOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        services.AddScoped<IApplicationDbContext>(_ => new TestDbContext(dbOptions));

        services.AddScoped<ICourseIndexingService>(_ =>
            useThrowing
                ? new ThrowingIndexingService()
                : new StubIndexingService());

        services.AddLogging();
        services.AddSingleton<IReindexJobService, ReindexJobService>();
        return services.BuildServiceProvider();
    }

    private sealed class StubIndexingService : ICourseIndexingService
    {
        public Task EnsureIndexAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task IndexCourseAsync(Guid courseId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task ReindexCoursesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<CourseIndexStats> GetStatsAsync(CancellationToken cancellationToken)
            => Task.FromResult(new CourseIndexStats(true, true, "ix", "ix", "ix-1", 0, 0, "green"));

        public Task ReindexCoursesChunkedAsync(
            IProgress<ReindexProgress>? progress,
            IProgress<ReindexLogEntry>? log,
            int batchSize,
            CancellationToken cancellationToken)
        {
            log?.Report(new ReindexLogEntry(DateTimeOffset.UtcNow, ReindexLogLevel.Info, "stub ok"));
            progress?.Report(new ReindexProgress(0, 0, 0, batchSize, 0, 0, 100));
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingIndexingService : ICourseIndexingService
    {
        public Task EnsureIndexAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task IndexCourseAsync(Guid courseId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task ReindexCoursesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<CourseIndexStats> GetStatsAsync(CancellationToken cancellationToken)
            => throw new InvalidOperationException("ES down");
        public Task ReindexCoursesChunkedAsync(
            IProgress<ReindexProgress>? progress,
            IProgress<ReindexLogEntry>? log,
            int batchSize,
            CancellationToken cancellationToken)
            => throw new InvalidOperationException("ES down");
    }
}