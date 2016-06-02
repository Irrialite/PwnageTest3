using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkBase;
using NetworkBase.Events;
using NetworkBase.Collections;
using NetworkBase.Events.Args;

namespace ServerTest2.TCP
{
    public sealed class TCPServerManager
    {
        private readonly ConcurrentBag<ITCPServer> m_Servers = new ConcurrentBag<ITCPServer>();
        private readonly ConcurrentDictionary<string, ITCPServer> m_Handlers = new ConcurrentDictionary<string, ITCPServer>();
        private readonly TcpListener m_TCPListener;
        private readonly ILogger<TCPServerManager> m_Logger;

        public TCPServerManager(ILogger<TCPServerManager> logger)
        {
            m_Logger = logger;
            m_TCPListener = new TcpListener(System.Net.IPAddress.Any, 6789);

            var t = new Thread(new ThreadStart(Listen));
            t.IsBackground = true;
            t.Start();
        }

        public void RegisterTCPServer(ITCPServer server)
        {
            m_Servers.Add(server);

            Debug.Assert(!m_Handlers.ContainsKey(server.Path), "Server map already contains " + server.Path);
            m_Handlers[server.Path] = server;
        }

        public bool GetTCPServerForPath(string path, out ITCPServer server)
        {
            return m_Handlers.TryGetValue(path, out server);
        }

        public bool GetTCPServerForGame(int gameID, int instanceID, out ITCPServer server)
        {
            return m_Handlers.TryGetValue($"/{gameID}/{instanceID}", out server);
        }

        private async Task ReceiveHandshakeTask(Socket client)
        {
            var recvBuf = new byte[4096];
            var recvBufMsg = new FastList<byte>(recvBuf.Length * 2);
            bool recv = true;
            while (recv)
            {
                try
                {
                    var msgSize = await client.ReceiveAsync(new ArraySegment<byte>(recvBuf), SocketFlags.None);
                    recvBufMsg.AddRange(recvBuf, msgSize);
                    GameEvent[] ges;
                    var bytesProcessed = recvBufMsg.Buffer.ParseGameEvents(recvBufMsg.Count, out ges);
                    recvBufMsg.RemoveRange(0, bytesProcessed);

                    if (ges.Length != 1)
                    {
                        client.Shutdown(SocketShutdown.Both);
                        client.Dispose();
                        return;
                    }

                    var hs = ges[0].GetData<ClientHandshake>();
                    ITCPServer server;
                    if (GetTCPServerForGame(hs.game, hs.instance, out server))
                    {
                        await server.RegisterTCPSocket(client, recvBuf, recvBufMsg);
                        recv = false;
                    }
                }
                catch (Exception e)
                {
                    m_Logger.LogError(new EventId(), e, $"Failed receiving from {client.RemoteEndPoint}!");
                    recv = false;
                }
            }

            client.Shutdown(SocketShutdown.Both);
            client.Dispose();
        }

        private void Listen()
        {
            m_TCPListener.Start();

            while (true)
            {
                var client = m_TCPListener.AcceptSocketAsync().GetAwaiter().GetResult();
                m_Logger.LogInformation($"Accepted a new client from {client.RemoteEndPoint}!");
                var task = ReceiveHandshakeTask(client);
            }
        }
    }
}
