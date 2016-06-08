using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetworkBase;
using NetworkBase.Events;

namespace ServerTest2.TCP
{
    public sealed class TestTCPServer : ITCPServer
    {
        public TestTCPServer(int gameID, int instanceID, ILogger logger) : base(gameID, instanceID, logger)
        {
        }

        protected override async Task OnConnected(Socket s)
        {
            m_Logger.LogInformation($"Client {s.RemoteEndPoint} connected to {Path}.");
            await s.SendEventAsync(new GameEvent(EGameEventID.Handshake, null, null));
        }

        protected override async Task OnDisconnected(Socket s)
        {
            await Task.FromResult<object>(null);
        }

        protected override async Task OnMessageReceived(GameEvent ev, Socket s)
        {
            m_Logger.LogInformation($"Received {ev.ID} with data {ev.GetData<object>()}");
            await Task.Delay(1000);
            await s.SendEventAsync(new GameEvent(EGameEventID.BetSet, null, null));
        }
    }
}
