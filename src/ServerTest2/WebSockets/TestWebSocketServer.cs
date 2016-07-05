using Microsoft.Extensions.Logging;
using ServerTest2.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using TCPServerBase.WebSockets;

namespace ServerTest2.WebSockets
{
    public class TestWebSocketServer : IWebSocketServer
    {
        public TestWebSocketServer(string path, ILogger logger) : base(path, logger)
        {
        }

        protected override async Task OnConnected(WebSocketImpl ws)
        {
            Console.WriteLine("SOCKET CONNECTED");
            await Task.FromResult<object>(null);
        }

        protected override async Task OnDisconnected(WebSocketImpl ws)
        {
            Console.WriteLine("SOCKET DISCONNECTED");
            await Task.FromResult<object>(null);
        }

        protected override async Task OnFrameReceived(byte[] buffer, WebSocketMessageType type, WebSocketImpl ws)
        {
            Console.WriteLine("FRAME RECV " + Encoding.UTF8.GetString(buffer));
            await Task.FromResult<object>(null);
        }

        protected override async Task OnMessageReceived(byte[] buffer, WebSocketMessageType type, WebSocketImpl ws)
        {
            Console.WriteLine("MSG RECV " + Encoding.UTF8.GetString(buffer));
            await Task.Delay(500);
            await Broadcast(Encoding.UTF8.GetString(buffer));
            //Send(ws, Encoding.UTF8.GetString(buffer));
        }
    }
}
