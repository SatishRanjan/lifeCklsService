using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace LifeCicklsWebApi.Security
{
    public sealed class LifeCklsServiceAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string authorizationHeader = context.HttpContext.Request.Headers["Authorization"];
            if (authorizationHeader != null && authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                // Extract credentials and decode from Base64
                string encodedCredentials = authorizationHeader.Substring("Basic ".Length).Trim();
                byte[] credentialsBytes = Convert.FromBase64String(encodedCredentials);
                string credentials = Encoding.UTF8.GetString(credentialsBytes);

                // Split username and password
                string[] parts = credentials.Split(':', 2);
                string username = parts[0];
                string password = parts.Length > 1 ? parts[1] : "";

                if (string.IsNullOrEmpty(username)
                    || string.IsNullOrEmpty(password)
                    || username != "lifeCklsApi"
                    || password != "lifeCklsApi@Pwd10")
                {
                    context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
            }
            else
            {
                // Authorization header is missing or not in the expected format
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }
        }
    }
}
