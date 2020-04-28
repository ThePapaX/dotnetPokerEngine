using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GameService.Context;
using GameService.Services;
using GameService.Utilities;
using Grpc.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PokerClassLibrary;

namespace GameService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : Controller
    {
        readonly GameDbContext _dBContext;
        readonly ILogger<PlayersController> _logger;
        public PlayersController(GameDbContext context, ILogger<PlayersController> logger )
        {
            _dBContext = context;
            _logger = logger;
        }
        

        [HttpGet]
        public IEnumerable<Player> Get()
        {
            return _dBContext.Players.Take(100);
        }

        
        [HttpGet("{id}")]
        public async Task<Player> Get(Guid id)
        {
            return await _dBContext.Players.FindAsync(id);
        }

        
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Player player)
        {
            if (await _dBContext.Players.FirstOrDefaultAsync(p => p.Email == player.Email) != null)
            {
                var errors = new Dictionary<string, string[]>
                {
                    { "Email", new string[] { $"Duplicated email ${player.Email}" } }
                };

                var valResult = new ValidationProblemDetails(errors);

                var jsonRes = Json(valResult);
                jsonRes.StatusCode = 400;
                return jsonRes;
            }

            var hash = new Guid().ToString();

            player.Identity = new PlayerIdentity()
            {
                Hash = hash,
                Password = Encryption.EncryptPassword(player.Email, hash)
            };

            var result = await _dBContext.Players.AddAsync(player);
            player = result.Entity;

           
            await _dBContext.SaveChangesAsync();
            
            return Json(player);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody]Player player)
        {
            
            player.Id = new Guid(id);

            //TODO: replace this update with a Patch operation.
            _dBContext.Players.Update(player);
            await _dBContext.SaveChangesAsync();

            return Ok();
        }
    }
}
