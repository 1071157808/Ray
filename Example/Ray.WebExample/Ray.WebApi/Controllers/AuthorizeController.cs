using Ray.WebApi.Models;
using Ray.WebApi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ray.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class AuthorizeController : Controller
    {
        private JwtSettings jwtSettings;

        public AuthorizeController(IOptions<JwtSettings> jwtSettings)
        {
            this.jwtSettings = jwtSettings.Value;
        }
        /// <summary>
        /// 获取token
        /// </summary>
        /// <remarks>
        /// Sample Request
        /// 
        ///     POST /api/authorize
        ///     {
        ///         "user":"Ray",
        ///         "password":"123456"        
        ///     }
        /// </remarks>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Token([FromBody]LoginViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.User == DataBaseUser.Name && viewModel.Password == DataBaseUser.Pwd)
                {
                    var claims = new Claim[]{
                        new Claim(ClaimTypes.Name, DataBaseUser.Name),
                        new Claim(ClaimTypes.Role,"user"),
                        new Claim(ClaimTypes.NameIdentifier,DataBaseUser.Id),
                        new Claim(Enum.GetName(typeof(AuthorizationType),AuthorizationType.SuperAdminOnly),"true")
                    };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                        jwtSettings.Issuer,
                        jwtSettings.Audience,
                        claims,
                        DateTime.Now, DateTime.Now.AddMinutes(30),
                        creds);
                    return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
                }
            }
            return BadRequest();
        }
        public class DataBaseUser
        {
            public static string Id => "1";
            public static string Name => "Ray";
            public static string Pwd => "123456";
        }
    }
}
