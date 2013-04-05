using System.Web.Mvc;

namespace TeamKeep.Controllers
{
    public class RedirectHttpsAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            if (request.Url != null && !request.IsSecureConnection && !request.IsLocal)
            {
                filterContext.Result = new RedirectResult("https://" + request.Url.Host + request.RawUrl);
            }
        }
    }
}