using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HTSV.FE.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public AuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                // Không có người dùng đăng nhập, chuyển hướng đến trang đăng nhập
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            // Kiểm tra quyền nếu có yêu cầu
            if (_roles.Any() && !_roles.Any(role => user.IsInRole(role)))
            {
                // Người dùng không có quyền truy cập
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
} 