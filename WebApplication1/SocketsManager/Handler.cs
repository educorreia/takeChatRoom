using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TakeChat.SocketsManager
{
    public class Handler : IHandler
    {
        public ConnectionManager Connections { get; set; }

        public Handler(ConnectionManager connections)
        {
            Connections = connections;
        }

        public virtual async Task OnDisconnected(WebSocket socket)
        {
            await Task.Run(() => Connections.RemoveSocketAsync(Connections.GetId(socket)));
        }

        public async Task SendMessage(WebSocket socket, CustomWebSocketMessage message)
        {
            if (socket.State != WebSocketState.Open)
                return;
            string serialisedMessage = JsonConvert.SerializeObject(message);
            byte[] bytes = Encoding.ASCII.GetBytes(serialisedMessage);
            await socket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SendMessage(string id, CustomWebSocketMessage message)
        {
            await SendMessage(Connections.GetSocketById(id), message);
        }

        public async Task SendMessageToAll(CustomWebSocketMessage message)
        {
            foreach (var con in Connections.GetAllConnections())
                await SendMessage(con.Value, message);
        }

        public async Task SendMessageToChannel(string channel, CustomWebSocketMessage message)
        {
            foreach (var item in Connections.getChannelById(channel))
                await SendMessage(item, message);
        }

        public async Task SendMessageOthersToChannel(string user, CustomWebSocketMessage message)
        {
            var currentChannel = Connections.GetChannelByUser(user);
            foreach (var item in Connections.getChannelById(currentChannel))
            {
                if(item != user)
                    await SendMessage(item, message);
            }
        }

        public async Task HandleMessage(WebSocketReceiveResult result, byte[] buffer, WebSocket socket)
        {
            string msg = Encoding.ASCII.GetString(buffer);
            try
            {
                var request = JsonConvert.DeserializeObject<CustomWebSocketMessage>(msg);
                var retorno = new CustomWebSocketMessage();
                if (result.MessageType == WebSocketMessageType.Text)
                {

                    //verificar a msg
                    string[] subs = request.Text.Split(' ');
                    string currentChannel;
                    switch (subs[0])
                    {
                        case "/register":
                            var newUser = subs[1].Trim();
                            if(Connections.AddSocket(socket, newUser))
                            {
                                currentChannel = "#general";
                                Connections.AddUserInChannel(currentChannel, newUser);
                                retorno.Text = String.Format("{0} entrou na sala: {1}", request.Username, currentChannel);
                                retorno.Username = newUser;
                                await SendMessageToChannel(currentChannel, retorno);
                            }
                            else
                            {
                                retorno.Text = String.Format("usuário já registrado");
                                await SendMessage(socket, retorno);
                            }
                            break;
                        case "/channel":
                            var newChannel = subs[1].Trim();
                            if (!Connections.UserExist(request.Username))
                                await SendMessage(socket, new CustomWebSocketMessage() { Text= "usuario nao registrado"});
                            
                            if (Connections.ChannelExist(newChannel))
                            {
                                currentChannel = Connections.GetChannelByUser(request.Username);
                                retorno.Text = String.Format("{0} saiu na sala: {1}", request.Username, currentChannel);
                                await SendMessageToChannel(currentChannel, retorno);
                                Connections.ChangeChannel(currentChannel, newChannel, request.Username);
                                retorno.Text = String.Format("{0} enrou na sala: {1}", request.Username, newChannel);
                                currentChannel = Connections.GetChannelByUser(request.Username);
                                await SendMessageToChannel(newChannel, retorno);
                            }
                            else
                            {
                                retorno.Text = String.Format("sala não existe");
                                await SendMessage(socket, retorno);
                            }
                            break;
                        case "/exit":
                            currentChannel = Connections.GetChannelByUser(request.Username);
                            Connections.removeUserInChannel(currentChannel, request.Username);
                            retorno.Text = String.Format("{0} saiu na sala: {1}", request.Username, currentChannel);
                            await SendMessageToChannel(currentChannel, retorno);
                            retorno.Text = String.Format("você foi desconectado");
                            await SendMessage(socket, retorno);
                            await Connections.RemoveSocketAsync(Connections.GetId(socket));
                            break;
                        default:
                            if (subs[0].StartsWith("@"))
                            {
                                //verificar se usuario existe
                                var userDest = subs[0].Replace("@", "");
                                if (!Connections.UserExist(userDest))
                                {
                                    retorno.Text = String.Format("usuário {0} não registrado", userDest);
                                    await SendMessage(socket, retorno);
                                }
                                if (subs[1] == "private")
                                {
                                    var socketDest = Connections.GetSocketById(userDest);
                                    retorno.Text = String.Format("{0} disse reservadamente para {1}: {2}", request.Username, userDest, request.Text.Replace("@private", "").Trim());
                                    await SendMessage(socket, retorno);
                                }
                                else
                                {
                                    retorno.Text = String.Format("{0} disse para {1}: {2}", request.Username, userDest, request.Text);
                                    currentChannel = Connections.GetChannelByUser(request.Username);
                                    await SendMessageToChannel(currentChannel, retorno);
                                }
                            }
                            else
                            {
                                currentChannel = Connections.GetChannelByUser(request.Username);
                                retorno.Text = request.Username + " disse: " + request.Text;
                                await SendMessageToChannel(currentChannel, retorno);
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                await socket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
            }
        }

    }

    public class CustomWebSocketMessage
    {
        public string Text { get; set; }
        public string Username { get; set; }
        public string Recipient { get; set; }
        public string Sender { get; set; }
        public Boolean Private { get; set; }
        public string Date { get; set; }
        public string Type { get; set; }
    }
}
