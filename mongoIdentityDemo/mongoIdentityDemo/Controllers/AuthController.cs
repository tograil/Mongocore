using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Identity4.Mongo.Core;
using mongoIdentityDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace mongoIdentityDemo.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly UserManager<MongoIdentityUser> _userManager;
        private readonly SignInManager<MongoIdentityUser> _signInManager;
        private readonly IOptions<AppConfiguration> _appConfiguration;

        public AuthController(
            UserManager<MongoIdentityUser> userManager, 
            SignInManager<MongoIdentityUser> signInManager,
            IOptions<AppConfiguration> appConfiguration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appConfiguration = appConfiguration;
        }

        [HttpGet("register")]
        public IEnumerable<string> Gett()
        {
            _signInManager.SignOutAsync().Wait();

            return new string[] { "value1", "value2" };
        }

        [HttpPost("reg")]
        public async Task<IEnumerable<string>> Gett1([FromBody]string test)
        {
            var user = new MongoIdentityUser { UserName ="Test", Email = "test@test.xx" };
            var result = await _userManager.CreateAsync(user, "Test1234_");

            

            await _signInManager.SignInAsync(user, false);

            return new string[] { "value1", "value2" };
        }

        [HttpPost("register")]
        public async Task<IActionResult> Create([FromBody] UserAuth model)
        {
            var user = new MongoIdentityUser { UserName = model.name, Email = "test@test.xx" };
            var result = await _userManager.CreateAsync(user, model.password);

            //await _userManager.AddClaimAsync(user, new Claim());

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(x => x.Description).ToList());
            }

            await _signInManager.SignInAsync(user, false);

            return Ok();
        }

        [HttpPost("token")]
        public async Task<IActionResult> Token([FromBody] UserAuth model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _signInManager.PasswordSignInAsync(model.name, model.password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.name);

                try
                {
                    var token = GetJwtSecurityToken(user);

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });
                }
                catch (Exception ex)
                {
                    return NotFound();
                }
            }

            return NotFound();
        }

        private JwtSecurityToken GetJwtSecurityToken(MongoIdentityUser user)
        {
            return new JwtSecurityToken(
                issuer: "http://localhost:3000",
                audience: "http://localhost:3000",
                claims: GetTokenClaims(user),
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("I-Am-A-Key5244512e79775268374231315e41507e3f6e72734c40397a7b2851674b")), SecurityAlgorithms.HmacSha256)
            );
        }

        private static IEnumerable<Claim> GetTokenClaims(MongoIdentityUser user)
        {
            return new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName)
            };
        }
    }
}