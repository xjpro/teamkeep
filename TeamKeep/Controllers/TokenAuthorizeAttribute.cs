using System.Web.Mvc;

namespace TeamKeep.Controllers
{
    public class TokenAuthorizeAttribute : AuthorizeAttribute
    {
        public bool RedirectToLogin { get; set; }

        public TokenAuthorizeAttribute()
        {
        }

        public TokenAuthorizeAttribute(bool redirectToLogin)
        {
            RedirectToLogin = redirectToLogin;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            /*if (!TokenAuthorization.IsAuthorized(filterContext.HttpContext.Request.Cookies))
            {
                filterContext.HttpContext.Response.StatusCode = 401;
                filterContext.Result = new HttpUnauthorizedResult();

                if (RedirectToLogin)
                {
                    filterContext.HttpContext.Response.Redirect("/?login=true&redirect=", true);
                }
            }*/
        }
    }
}