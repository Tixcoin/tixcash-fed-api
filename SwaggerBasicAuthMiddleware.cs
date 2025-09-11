using System.Net;
using System.Text;

namespace dataAPI
{
    public class SwaggerBasicAuthMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IConfiguration config;
        private readonly DataUtility _du;
        public SwaggerBasicAuthMiddleware(RequestDelegate next, IConfiguration config)
        {
            this.next = next;
            this.config = config;
            _du = new DataUtility((string)this.config["ConnectionString"]);
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/WeatherForecast") || context.Request.Path.StartsWithSegments("/swagger"))
            {
                if (context.Request.Path.Value.Contains("Auth"))
                {
                    if (!IsAuthorized("", ""))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return;
                    }
                }
                await next.Invoke(context);
                return;
            }
            else
            {
                await next.Invoke(context);
            }
        }
        public async Task InvokeAsync1(HttpContext context)
        {
            return ;
            if (context.Request.Path.StartsWithSegments("/WeatherForecast") || context.Request.Path.StartsWithSegments("/swagger"))
            {
                string authHeader = context.Request.Headers["Authorization"];
                if (authHeader != null && authHeader.StartsWith("Basic "))
                {
                    var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();
                    var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));

                    var username = decodedUsernamePassword.Split(':', 2)[0];
                    var password = decodedUsernamePassword.Split(':', 2)[1];

                    if (!context.Request.Path.Value.Contains("Auth") || IsAuthorized(username, password))
                    {
                        await next.Invoke(context);
                        return;
                    }
                }

                //if (context.Request.Path.Value.Contains("Auth"))
                //{
                //    if (IsAuthorized("", ""))
                //    {
                //        context.Response.Headers["WWW-Authenticate"] = "Basic";
                //        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                //    }
                //}
                //else
                //{
                //   // await next.Invoke(context);
                   
                //}


            }
            else
            {
                await next.Invoke(context);
            }
        }

        public bool IsAuthorized(string username, string password)
        {
            return true;
            var dt = _du.GetDataTable("SELECT TOP 1 1 FROM [dbo].[LoginNow] Where ([emailid]='" + username + "' OR [mobile]='" + username + "') AND DATEDIFF(minute,[expiredon],[createdon]) > 0 ");

            return dt.Rows.Count > 0;
        }
    }
}