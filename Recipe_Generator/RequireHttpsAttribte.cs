using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Text;

namespace Recipe_Generator
{
    public class RequireHttpsAttribte :AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if(actionContext.Request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                actionContext.Response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.Found);
                actionContext.Response.Content = new StringContent("<p>Use HTTPS instead of HTTP</p>",Encoding.UTF8 ,"text/html");

                UriBuilder uriBuilder = new UriBuilder(actionContext.Request.RequestUri);
                uriBuilder.Scheme = Uri.UriSchemeHttps;
                //uriBuilder.Port = 5115
            }
            else
            {
                base.OnAuthorization(actionContext);
            }
        } 
    }
}
