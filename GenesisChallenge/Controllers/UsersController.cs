using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GenesisChallenge.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using GenesisChallenge.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GenesisChallenge.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [AllowAnonymous]
        [HttpPost("signup")]
        public ActionResult SignUp([FromBody] User user)
        {
            if (_userService.EmailExists(user.Email))
            {
                return Conflict(new { message = "E-mail already exists" });
            }

            var newUser = _userService.CreateUser(user);

            return Ok(newUser);
        }

        [AllowAnonymous]
        [HttpPost("signin")]
        public ActionResult SignIn([FromBody] SignIn signIn)
        {
            var user = _userService.Authenticate(signIn.Email, signIn.Password);

            if (user == null)
                return StatusCode(401,new { message = "Invalid user and / or password" });

            return Ok(user);
        }


        [HttpGet("search/{id}")]
        public ActionResult Search([FromRoute] Guid id)
        {
            var currentUser = HttpContext.User;
            if (currentUser.HasClaim(c => c.Type == ClaimTypes.Name))
            {
                Guid currentId = Guid.Parse(currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value);
                if (currentId == id)
                {
                    var user = _userService.GetUser(id);
                    if (user != null)
                        return Ok(user);
                }
            }
            return StatusCode(401, new { message = "Unauthorized" });

        }

    }
}