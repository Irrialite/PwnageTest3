using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServerTest2.WebSockets
{
    public class WebSocketImpl
    {
        public CommonWebSocket WebSocket
        {
            get;
            private set;
        }

        public HttpContext Context
        {
            get;
            private set;
        }

        public WebSocketImpl(CommonWebSocket sock, HttpContext context)
        {
            WebSocket = sock;
            Context = context;
        }

        public void Dispose()
        {
            Context = null;
            WebSocket.Dispose();
            WebSocket = null;
        }
    }
}
