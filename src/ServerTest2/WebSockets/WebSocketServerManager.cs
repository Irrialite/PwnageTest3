using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServerTest2.WebSockets
{
    public class WebSocketServerManager
    {
        private readonly ConcurrentBag<IWebSocketServer> m_Servers = new ConcurrentBag<IWebSocketServer>();
        private readonly ConcurrentDictionary<string, IWebSocketServer> m_Handlers = new ConcurrentDictionary<string, IWebSocketServer>();

        public WebSocketServerManager()
        {
            var t = new Thread(new ThreadStart(CleanupSockets));
            t.IsBackground = true;
            t.Start();
        }

        public void RegisterWebSocketServer(IWebSocketServer server)
        {
            m_Servers.Add(server);

            Debug.Assert(!m_Handlers.ContainsKey(server.Path), "Server map already contains " + server.Path);
            m_Handlers[server.Path] = server;
        }

        public bool GetWebSocketServerForPath(string path, out IWebSocketServer server)
        {
            return m_Handlers.TryGetValue(path, out server);
        }

        private void CleanupSockets()
        {
            while (true)
            {
                foreach (var server in m_Servers)
                {
                    server.CleanupSockets();
                }
                Thread.Sleep(20 * 1000);
            }
        }
    }
}
