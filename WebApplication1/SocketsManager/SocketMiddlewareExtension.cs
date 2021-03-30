using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TakeChat.SocketsManager
{
    public static class SocketMiddlewareExtension
    {
        public static IApplicationBuilder UseSocketMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SocketMiddleware>();
        }
    }
}
