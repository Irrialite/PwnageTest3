using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServerTest2.WebSockets
{
    public abstract class IWebSocketServer
    {
        protected readonly ConcurrentBag<WebSocketImpl> m_WebSockets = new ConcurrentBag<WebSocketImpl>();
        protected readonly ILogger m_Logger;

        public string Path
        {
            get;
            private set;
        }

        public IWebSocketServer(string path, ILogger logger)
        {
            Path = path;
            m_Logger = logger;
        }

        public async Task RegisterWebSocket(WebSocketImpl ws)
        {
            m_WebSockets.Add(ws);
            await OnConnected(ws);

            try
            {
                var recvBuf = new byte[4096];
                var recvMsg = new List<byte>(recvBuf.Length * 2);

                while (ws.WebSocket.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    var res = await ws.WebSocket.ReceiveAsync(new ArraySegment<byte>(recvBuf), CancellationToken.None);
                    if (res.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                    {
                        await ws.WebSocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        return;
                    }

                    var recvFrame = new byte[res.Count];
                    Array.Copy(recvBuf, recvFrame, res.Count);
                    await OnFrameReceived(recvFrame, res.MessageType, ws);

                    recvMsg.AddRange(recvFrame);
                    if (res.EndOfMessage)
                    {
                        await OnMessageReceived(recvMsg.ToArray(), res.MessageType, ws);
                        recvMsg.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                await UnregisterWebSocket(ws);
            }
        }

        public void CleanupSockets()
        {
            foreach (var sock in m_WebSockets)
            {
                if (sock.WebSocket.State >= System.Net.WebSockets.WebSocketState.Closed)
                {
                    UnregisterWebSocket(sock).GetAwaiter().GetResult();
                }
            }
        }

        protected async Task UnregisterWebSocket(WebSocketImpl ws)
        {
            ws.Dispose();
            m_WebSockets.TryTake(out ws);
            await OnDisconnected(ws);
        }

        protected abstract Task OnConnected(WebSocketImpl ws);
        protected abstract Task OnDisconnected(WebSocketImpl ws);
        protected abstract Task OnFrameReceived(byte[] buffer, System.Net.WebSockets.WebSocketMessageType type, WebSocketImpl ws);
        protected abstract Task OnMessageReceived(byte[] buffer, System.Net.WebSockets.WebSocketMessageType type, WebSocketImpl ws);

        protected async Task Send(WebSocketImpl ws, string data)
        {
            try
            {
                if (data == null)
                {
                    data = string.Empty;
                }
                var buffer = System.Text.Encoding.UTF8.GetBytes(data);
                await ws.WebSocket.SendAsync(new ArraySegment<byte>(buffer), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception)
            {
                // TODO: log
            }
        }

        protected async Task Send(WebSocketImpl ws, byte[] data)
        {
            try
            {
                if (data == null)
                {
                    data = new byte[0];
                }

                await ws.WebSocket.SendAsync(new ArraySegment<byte>(data), System.Net.WebSockets.WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            catch (Exception)
            {
                // TODO: log
            }
        }

        protected async Task Broadcast(string data)
        {
            await Task.WhenAll(m_WebSockets.Select(x => Send(x, data)));
        }

        protected async Task Broadcast(byte[] data)
        {
            await Task.WhenAll(m_WebSockets.Select(x => Send(x, data)));
        }
    }
}

