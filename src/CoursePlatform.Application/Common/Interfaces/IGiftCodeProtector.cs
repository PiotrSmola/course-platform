namespace CoursePlatform.Application.Common.Interfaces;

public interface IGiftCodeProtector
{
    string Protect(string code);
    string Unprotect(string protectedCode);
}
