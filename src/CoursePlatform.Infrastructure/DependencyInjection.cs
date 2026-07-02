using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Infrastructure.Persistence;
using CoursePlatform.Infrastructure.Services;
using CoursePlatform.Infrastructure.Identity;
using CoursePlatform.Infrastructure.Options;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;

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
        services.AddScoped<IFileStorageService, MinioFileStorageService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IHtmlSanitizer, HtmlSanitizerWrapper>();

        return services;
    }
}
