using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace TakeChat.SocketsManager
{
    public class SocketMiddleware
    {
        private readonly RequestDelegate _next;
        private Handler _handler { get; set; }

        public SocketMiddleware(RequestDelegate next, Handler handler)
        {
            _next = next;
            _handler = handler;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }

            WebSocket currentSocket = await context.WebSockets.AcceptWebSocketAsync();
            await ReceiveMessage(currentSocket);
        }

        private async Task ReceiveMessage(WebSocket currentSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await currentSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                await _handler.HandleMessage(result, buffer, currentSocket);
                buffer = new byte[1024 * 4];
                result = await currentSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
        }

    }
}
