using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerTest2
{
    public static class Extensions
    {
        public static string GetRequestPath(this HttpContext http)
        {
            return http.Request.Path.ToString().ToLowerInvariant();
        }
    }
}
