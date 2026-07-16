using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR;
using CoursePlatform.Application.Common.Behaviours;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.Features.Certificates.Services;

namespace CoursePlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TracingBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TrimmingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });

        services.AddScoped<ICertificateIssuer, CertificateIssuer>();

        return services;
    }
}
