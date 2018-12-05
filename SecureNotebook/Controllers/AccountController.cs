using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SecureNotebook.Db;
using SecureNotebook.Encryption;
using SecureNotebook.Models;
using SecureNotebook.Services;

namespace SecureNotebook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private NotebookContext db;
        private UserService userService;

        public AccountController(NotebookContext db)
        {
            this.db = db;
        }

        [HttpPost("login")]
        public IActionResult Login([FromForm] LoginModel user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userInfo = userService.LoginUser(user.Username, user.Password);
                    var tokenString = BuildToken(user.Username);

                    return Ok(
                        new
                        {
                            userInfo.Id,
                            userInfo.Username,
                            Token = tokenString,
                        });
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /*
        [HttpPost("register")]
        public IActionResult Register([FromBody]UserRegisterModel user)
        {
            var userDto = Mapper.Map<UserRegisterDTO>(user);

            if (ModelState.IsValid)
            {
                try
                {
                    var newUser = userService.RegisterUser(userDto);
                    var tokenString = BuildToken(newUser);

                    return Ok(
                        new
                        {
                            newUser.Id,
                            newUser.Email,
                            newUser.Name,
                            newUser.Role,
                            newUser.ClinicId,
                            Token = tokenString,
                        });
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
        */

        private string BuildToken(string username)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, username)
            };

            var key = AuthOptions.GetSymmetricSecurityKey();
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                AuthOptions.ISSUER,
                AuthOptions.AUDIENCE,
                claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}