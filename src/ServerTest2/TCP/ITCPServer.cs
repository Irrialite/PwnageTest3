using Microsoft.Extensions.Logging;
using NetworkBase;
using NetworkBase.Collections;
using NetworkBase.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ServerTest2.TCP
{
    public abstract class ITCPServer
    {
        protected readonly ConcurrentBag<Socket> m_Clients = new ConcurrentBag<Socket>();
        protected readonly ILogger m_Logger;

        public string Path
        {
            get;
            private set;
        }

        public int GameID
        {
            get;
            private set;
        }

        public int InstanceID
        {
            get;
            private set;
        }

        public ITCPServer(int gameID, int instanceID, ILogger logger)
        {
            GameID = gameID;
            InstanceID = instanceID;
            Path = $"/{gameID}/{instanceID}";
            m_Logger = logger;
        }

        public async Task RegisterTCPSocket(Socket s, byte[] recvBuf, FastList<byte> recvBufList)
        {
            m_Clients.Add(s);
            await OnConnected(s);

            bool recv = true;
            while (recv)
            {
                try
                {
                    var msgSize = await s.ReceiveAsync(new ArraySegment<byte>(recvBuf), SocketFlags.None);
                    recvBufList.AddRange(recvBuf, msgSize);
                    GameEvent[] ges;
                    var bytesProcessed = recvBufList.Buffer.ParseGameEvents(recvBufList.Count, out ges);
                    recvBufList.RemoveRange(0, bytesProcessed);

                    foreach (var ge in ges)
                    {
                        await OnMessageReceived(ge, s);
                    }
                }
                catch (Exception e)
                {
                    m_Logger.LogError(new EventId(), e, $"Failed receiving from {s.RemoteEndPoint}!");
                    recv = false;
                }
            }

            await UnregisterTCPSocket(s);
        }

        protected async Task UnregisterTCPSocket(Socket s)
        {
            m_Clients.TryTake(out s);
            await OnDisconnected(s);
        }

        protected abstract Task OnConnected(Socket s);
        protected abstract Task OnDisconnected(Socket s);
        protected abstract Task OnMessageReceived(GameEvent ev, Socket s);
    }
}
