using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace TakeChat.SocketsManager
{
    public class ConnectionManager : IConnectionManager
    {
        private ConcurrentDictionary<string, WebSocket> _connections;
        private ConcurrentDictionary<string, List<string>> channnels = new ConcurrentDictionary<string, List<string>>();

        public ConnectionManager()
        {
            _connections = new ConcurrentDictionary<string, WebSocket>();
            channnels.TryAdd("#general", new List<string>());
            channnels.TryAdd("#recife", new List<string>());
        }

        public WebSocket GetSocketById(string id)
        {
            return _connections.FirstOrDefault(n => n.Key == id).Value;
        }

        public ConcurrentDictionary<string, WebSocket> GetAllConnections()
        {
            return _connections;
        }

        public string GetId(WebSocket socket)
        {
            return _connections.FirstOrDefault(n => n.Value == socket).Key;
        }

        public async Task RemoveSocketAsync(string id)
        {
            _connections.TryRemove(id, out var socket);
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "socket connection closed", CancellationToken.None);
        }

        public bool AddSocket(WebSocket socket, string nick)
        {
            return _connections.TryAdd(nick, socket);
        }

        public void AddUserInChannel(string channel, string user)
        {
            //var list = channnels[channel]; // = channnels[channel].Add(user);
            channnels[channel].Add(user);
        }

        public List<string> getChannelById(string channel)
        {
            return channnels[channel];
        }

        public bool UserExist(string user)
        {
            return _connections.ContainsKey(user);
        }

        public string GetChannelByUser(string user)
        {
            foreach (var item in channnels)
            {
                if(item.Value.Any(n => ((string) n) == user))
                    return item.Key;
            }
            return "";
        }

        public bool ChannelExist(string channel)
        {
            return channnels.ContainsKey(channel);
        }

        public void ChangeChannel(string channel, string newChannel, string user)
        {
            removeUserInChannel(channel, user);
            AddUserInChannel(newChannel, user);
        }

        public void removeUserInChannel(string channel, string user)
        {
            List<string> list = new List<string>();
            foreach (var item in channnels[channel])
            {
                if (item != user)
                    list.Add(item);
            }
            channnels[channel] = list;
        }
    }
}
