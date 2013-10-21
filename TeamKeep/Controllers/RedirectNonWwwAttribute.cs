using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Teamkeep.Controllers
{
    public class RedirectNonWwwAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var test = new Regex(@"^www\.");

            if (request.Url != null && test.Match(request.Url.Host).Success && !request.IsLocal)
            {
                filterContext.Result = new RedirectResult(request.Url.Scheme + "://" + test.Replace(request.Url.Host, string.Empty) + request.RawUrl);
            }
        }
    }
}