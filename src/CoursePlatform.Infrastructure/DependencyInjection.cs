using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Infrastructure.Persistence;
using CoursePlatform.Infrastructure.Services;
using CoursePlatform.Infrastructure.Identity;
using CoursePlatform.Infrastructure.Options;
using CoursePlatform.Infrastructure.Search;
using Amazon.S3;
using Amazon.Runtime;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

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
                options.UseInMemoryDatabase("CoursePlatformTests");
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
        services.AddSingleton<IFileStorageService, MinioFileStorageService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IHtmlSanitizer, HtmlSanitizerWrapper>();
        services.AddSingleton<IPaymentGateway, StripePaymentGateway>();

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
