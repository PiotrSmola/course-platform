using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Authorization;
using CoursePlatform.Application.Common.Interfaces;

namespace CoursePlatform.Infrastructure.Authorization;

public sealed class ManageCourseAuthorizationHandler : AuthorizationHandler<ManageCourseRequirement>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ManageCourseAuthorizationHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageCourseRequirement requirement)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var userIdValue = context.User.FindFirstValue("sub") ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        var routeValues = httpContext.Request.RouteValues;
        Guid courseId = default;
        var resolved =
            TryGetGuid(routeValues, "id", out courseId) ||
            TryGetGuid(routeValues, "courseId", out courseId);

        if (!resolved)
        {
            return;
        }

        var isOwner = await _context.Courses
            .AsNoTracking()
            .AnyAsync(c => c.Id == courseId && c.InstructorId == userId);

        if (isOwner)
        {
            context.Succeed(requirement);
        }
    }

    private static bool TryGetGuid(RouteValueDictionary routeValues, string key, out Guid value)
    {
        value = default;
        if (!routeValues.TryGetValue(key, out var raw) || raw == null)
        {
            return false;
        }

        return Guid.TryParse(raw.ToString(), out value);
    }
}
