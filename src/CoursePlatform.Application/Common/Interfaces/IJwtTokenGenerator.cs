using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user, IEnumerable<string> roles);
}
