using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Infrastructure.Persistence;
using CoursePlatform.Infrastructure.Services;
using CoursePlatform.Infrastructure.Options;
using CoursePlatform.Infrastructure.Resilience;
using CoursePlatform.Infrastructure.Search;
using CoursePlatform.Infrastructure.Hubs;
using CoursePlatform.Infrastructure.Storage;
using Amazon.S3;
using Amazon.Runtime;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Polly;
using Polly.Retry;
using StripeException = Stripe.StripeException;

namespace CoursePlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (environment.IsEnvironment("Testing"))
            {
                var databaseName = configuration["Testing:DatabaseName"] ?? "CoursePlatformTests";
                options.UseInMemoryDatabase(databaseName);
            }
            else
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            }
        });

        services.Configure<MinioOptions>(configuration.GetSection("Minio"));
        services.Configure<ElasticOptions>(configuration.GetSection("Elastic"));
        services.Configure<StripeOptions>(configuration.GetSection("Stripe"));
        services.Configure<CertificateOptions>(configuration.GetSection("Certificates"));
        services.Configure<EmailOptions>(configuration.GetSection("Email"));
        services.Configure<CoursePlatform.Application.Common.Options.FrontendOptions>(
            configuration.GetSection(CoursePlatform.Application.Common.Options.FrontendOptions.SectionName));
        services.Configure<CoursePlatform.Application.Common.Options.SubscriptionOptions>(
            configuration.GetSection(CoursePlatform.Application.Common.Options.SubscriptionOptions.SectionName));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MinioOptions>>().Value;

            var credentials = new BasicAWSCredentials(options.AccessKey, options.SecretKey);
            var internalEndpoint = new Uri(options.InternalEndpoint);
            var config = new AmazonS3Config
            {
                ServiceURL = options.InternalEndpoint,
                UseHttp = internalEndpoint.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase),
                ForcePathStyle = true,
                AuthenticationRegion = options.Region
            };

            return new AmazonS3Client(credentials, config);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IIdentityService, Identity.IdentityService>();
        services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, Authorization.ManageCourseAuthorizationHandler>();
        services.AddHostedService<Identity.RefreshTokenCleanupService>();
        if (!environment.IsEnvironment("Testing"))
        {
            services.AddHostedService<StaleMultipartUploadCleanupService>();
        }

        services.AddResiliencePipeline(ResiliencePipelineNames.Outbound, builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<IOException>()
                    .Handle<TimeoutException>()
                    .Handle<AmazonS3Exception>(ex =>
                        (int)ex.StatusCode >= 500
                        || ex.StatusCode == System.Net.HttpStatusCode.RequestTimeout
                        || ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable
                        || ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    .Handle<AmazonClientException>()
                    .Handle<StripeException>(ex =>
                        ex.HttpStatusCode is >= System.Net.HttpStatusCode.InternalServerError
                            or System.Net.HttpStatusCode.RequestTimeout
                            or System.Net.HttpStatusCode.TooManyRequests)
            });
            builder.AddTimeout(TimeSpan.FromSeconds(15));
        });

        services.AddSingleton<IFileStorageService, MinioFileStorageService>();
        services.AddSingleton<INotificationService, SignalRNotificationService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IJwtTokenGenerator, Identity.JwtTokenGenerator>();
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IHtmlSanitizer, HtmlSanitizerWrapper>();
        services.AddSingleton<IPaymentGateway, StripePaymentGateway>();

        services.AddSingleton<ICertificatePdfGenerator, QuestPdfCertificateGenerator>();

        services.AddSingleton<Email.ChannelEmailQueue>();
        services.AddSingleton<IEmailQueue>(sp => sp.GetRequiredService<Email.ChannelEmailQueue>());
        services.AddSingleton<IEmailSender, Email.SmtpEmailSender>();
        services.AddHostedService<Email.EmailDispatcher>();

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection) && !environment.IsEnvironment("Testing"))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);
        }

        services.AddHybridCache();
        services.AddSingleton<IAppCache, Caching.HybridAppCache>();

        services.AddScoped<EfCourseSearchService>();

        var elasticOptions = configuration.GetSection("Elastic").Get<ElasticOptions>() ?? new ElasticOptions();
        if (elasticOptions.Enabled && !environment.IsEnvironment("Testing"))
        {
            services.AddSingleton(sp =>
            {
                var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ElasticOptions>>().Value;
                var settings = new ElasticsearchClientSettings(new Uri(opts.Uri));

                if (!string.IsNullOrWhiteSpace(opts.ApiKey))
                {
                    settings.Authentication(new ApiKey(opts.ApiKey));
                }
                else if (!string.IsNullOrWhiteSpace(opts.Username) && !string.IsNullOrWhiteSpace(opts.Password))
                {
                    settings.Authentication(new BasicAuthentication(opts.Username, opts.Password));
                }

                return new ElasticsearchClient(settings);
            });
            services.AddScoped<ICourseSearchService, ElasticCourseSearchService>();
            services.AddScoped<ICourseIndexingService, ElasticCourseIndexingService>();
            services.AddSingleton<IReindexJobService, ReindexJobService>();
        }
        else
        {
            services.AddScoped<ICourseSearchService>(sp => sp.GetRequiredService<EfCourseSearchService>());
            services.AddScoped<ICourseIndexingService, NoOpCourseIndexingService>();
            services.AddSingleton<IReindexJobService, NoOpReindexJobService>();
        }

        return services;
    }
}
