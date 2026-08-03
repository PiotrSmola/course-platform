using CoursePlatform.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace CoursePlatform.Infrastructure.Services;

internal sealed class GiftCodeProtector : IGiftCodeProtector
{
    private readonly IDataProtector _protector;

    public GiftCodeProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("CoursePlatform.GiftCodes.v1");
    }

    public string Protect(string code) => _protector.Protect(code);

    public string Unprotect(string protectedCode) => _protector.Unprotect(protectedCode);
}
