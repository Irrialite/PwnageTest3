using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerTest2.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetworkBase.Events.Args;
using TCPServerBase.TCP;

namespace ServerTest2.Controllers
{
    [Route("api/[controller]")]
    public sealed class GameInstanceController : ControllerBase
    {
        private readonly TCPServerManager m_TCPServerManager;
        private readonly ILogger<GameInstanceController> m_Logger;

        public GameInstanceController(TCPServerManager serverManager, ILogger<GameInstanceController> logger)
        {
            m_Logger = logger;
            m_TCPServerManager = serverManager;
        }

        public IActionResult Index()
        {
            return NotFound();
        }

        [HttpGet("RequestGame/{game}")]
        public IActionResult RequestGame(string game)
        {
            int gameId;
            int id;
            if (string.IsNullOrEmpty("game") || !int.TryParse(game, out gameId) || (id = m_TCPServerManager.GetNewInstanceForGame(gameId)) == -1)
            {
                return new JsonResult(new
                {
                    error = "Invalid game id specified!",
                });
            }

            m_TCPServerManager.RegisterTCPServer(new TestTCPServer(gameId, id, m_Logger));
            return new JsonResult(new GameInstanceCreated()
            {
                game = gameId,
                id = id,
                slots = new Random().Next(1, 8),
                secret = new Random().NextDouble().GetHashCode()
            });
        }

        [HttpGet("GetServerList")]
        public IActionResult GetServerList()
        {
            var servers = m_TCPServerManager.GetServerList();

            return new JsonResult(new GameServerList()
            {
                servers = servers.Select(x => new int[2] { x.GameID, x.InstanceID }).ToArray(),
            });
        }
    }
}
