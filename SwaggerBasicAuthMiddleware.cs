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
        public bool IsAuthorized(string username, string password)
        {
            return true;
            var dt = _du.GetDataTable("SELECT TOP 1 1 FROM [dbo].[LoginNow] Where ([emailid]='" + username + "' OR [mobile]='" + username + "') AND DATEDIFF(minute,[expiredon],[createdon]) > 0 ");

            return dt.Rows.Count > 0;
        }
    }
}