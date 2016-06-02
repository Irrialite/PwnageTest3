using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerTest2.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetworkBase.Events.Args;

namespace ServerTest2.Controllers
{
    [Route("api/[controller]")]
    public sealed class GameInstanceController : ControllerBase
    {
        private readonly TCP.TCPServerManager m_TCPServerManager;
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
            if (string.IsNullOrEmpty("game") || !int.TryParse(game, out gameId))
            {
                return new JsonResult(new
                {
                    error = "Invalid game id specified!",
                });
            }

            m_TCPServerManager.RegisterTCPServer(new TestTCPServer(gameId, 1, m_Logger));
            return new JsonResult(new GameInstanceCreated()
            {
                game = gameId,
                id = 1,
            });
        }
    }
}
