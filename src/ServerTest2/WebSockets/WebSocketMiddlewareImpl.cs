using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebSockets.Protocol;
using Microsoft.AspNetCore.WebSockets.Server;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerTest2.WebSockets
{
    public sealed class WebSocketMiddlewareImpl
    {
        private readonly RequestDelegate m_Next;
        private readonly WebSocketServerManager m_WebSocketServerManager;
        private readonly ILogger<WebSocketMiddlewareImpl> m_Logger;

        public WebSocketMiddlewareImpl(RequestDelegate next, WebSocketServerManager websocketManager, ILogger<WebSocketMiddlewareImpl> logger)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (websocketManager == null)
            {
                throw new ArgumentNullException(nameof(websocketManager));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            m_Next = next;
            m_WebSocketServerManager = websocketManager;
            m_Logger = logger;
            m_WebSocketServerManager.RegisterWebSocketServer(new TestWebSocketServer("/hai", m_Logger));
        }

        public async Task Invoke(HttpContext http)
        {
            var f = http.Features.Get<IHttpWebSocketFeature>();
            if (f == null || !f.IsWebSocketRequest)
            {
                await m_Next.Invoke(http);
                return;
            }

            try
            {
                IWebSocketServer server;
                if (m_WebSocketServerManager.GetWebSocketServerForPath(http.GetRequestPath(), out server))
                {
                    var socket = await f.AcceptAsync(null);
                    if (socket != null && socket.State == WebSocketState.Open)
                    {
                        m_Logger.LogInformation($"Accepted a new socket from {http.Connection.RemoteIpAddress}!");
                        await server.RegisterWebSocket(new WebSocketImpl(socket as CommonWebSocket, http));
                    }
                    else
                    {
                        m_Logger.LogInformation($"Accepted a new socket from {http.Connection.RemoteIpAddress} but it was not open/valid!");
                        await m_Next.Invoke(http);
                    }
                }
                else
                {
                    m_Logger.LogInformation($"Processed a websocket request on an unregistered URI from {http.Connection.RemoteIpAddress}!");
                }
            }
            catch (InvalidOperationException)
            {
                m_Logger.LogInformation($"Processed a normal HTTP request on a websocket URI from {http.Connection.RemoteIpAddress}! Passing on...");
                await m_Next.Invoke(http);
            }
        }
    }
}
