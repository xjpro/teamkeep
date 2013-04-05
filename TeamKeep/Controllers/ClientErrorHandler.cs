using System.Net.Mime;
using System.Web.Mvc;

namespace TeamKeep.Controllers
{
    public class ClientErrorHandler : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            var response = filterContext.RequestContext.HttpContext.Response;
            response.Write(filterContext.Exception.Message);
            response.StatusCode = 400;
            response.ContentType = MediaTypeNames.Text.Plain;
            filterContext.ExceptionHandled = true;
        }
    }
}