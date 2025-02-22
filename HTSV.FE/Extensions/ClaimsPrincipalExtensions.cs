using System.Security.Claims;

namespace HTSV.FE.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        public static string GetMaSinhVien(this ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst("MaSinhVien");
            return claim?.Value ?? string.Empty;
        }
    }
} 