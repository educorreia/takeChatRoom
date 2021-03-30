using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace TakeChat.SocketsManager
{
    public interface IHandler
    {
        Task SendMessage(WebSocket socket, CustomWebSocketMessage message);

        Task SendMessage(string id, CustomWebSocketMessage message);
        Task SendMessageToAll(CustomWebSocketMessage message);

        Task SendMessageToChannel(string channel, CustomWebSocketMessage message);

        Task HandleMessage(WebSocketReceiveResult result, byte[] buffer, WebSocket socket);
    }
}
