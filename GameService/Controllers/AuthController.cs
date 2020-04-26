using GameService.Context;
using GameService.Models;
using GameService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public AuthController(IIdentityService userService)
        {
            _identityService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody]AuthRequest loginRequest)
        {
            var user = await _identityService.Authenticate(loginRequest.Email, loginRequest.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user.Identity.SessionToken);
        }
        
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            var idClaim = HttpContext.User.Claims.Where(c => c.Type == "Id").FirstOrDefault();
            var playerId = new Guid(idClaim.Value);

            await _identityService.Logout(playerId);

            return Ok();
        }

    }
}
