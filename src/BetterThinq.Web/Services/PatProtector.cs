using Microsoft.AspNetCore.DataProtection;

namespace BetterThinq.Web.Services;

public sealed class PatProtector(IDataProtectionProvider provider)
{
    private readonly IDataProtector _protector = provider.CreateProtector("BetterThinq.Pat.v1");

    public string Encrypt(string plain) => _protector.Protect(plain);
    public string Decrypt(string cipher) => _protector.Unprotect(cipher);
}
