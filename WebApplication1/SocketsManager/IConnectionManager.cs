using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace TakeChat.SocketsManager
{
    public interface IConnectionManager
    {
        WebSocket GetSocketById(string id);

        ConcurrentDictionary<string, WebSocket> GetAllConnections();

        string GetId(WebSocket socket);

        Task RemoveSocketAsync(string id);

        bool AddSocket(WebSocket socket, string user);
        string GetChannelByUser(string user);
        bool ChannelExist(string channel);
        void ChangeChannel(string channel, string newChannel, string user);
        bool UserExist(string user);
    }
}
