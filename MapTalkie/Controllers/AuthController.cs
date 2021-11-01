using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MapTalkie.Configuration;
using MapTalkie.Models;
using MapTalkie.Services.TokenService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MapTalkie.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        [HttpPost("signin")]
        public Task<ActionResult<LoginResponse>> SignIn(
            [FromBody] LoginRequest body,
            [FromServices] UserManager<User> manager,
            [FromServices] AuthenticationSettings authenticationSettings,
            [FromServices] ITokenService tokenService)
        {
            return SignInPrivate(body, manager, authenticationSettings, tokenService, false);
        }

        [HttpPost("hybrid-signin")]
        public Task<ActionResult<LoginResponse>> HybridSignIn(
            [FromBody] LoginRequest body,
            [FromServices] UserManager<User> manager,
            [FromServices] AuthenticationSettings authenticationSettings,
            [FromServices] ITokenService tokenService)
        {
            return SignInPrivate(body, manager, authenticationSettings, tokenService, true);
        }

        private async Task<ActionResult<LoginResponse>> SignInPrivate(
            LoginRequest body,
            UserManager<User> manager,
            AuthenticationSettings authenticationSettings,
            ITokenService tokenService,
            bool hybrid)
        {
            var user = await manager.FindByNameAsync(body.UserName);
            if (user == null)
                return Unauthorized();

            if (await manager.CheckPasswordAsync(user, body.Password))
            {
                var token = tokenService.CreateToken(user);
                if (hybrid)
                {
                    Response.Cookies.Append(
                        authenticationSettings.HybridCookieName,
                        token.Signature,
                        new CookieOptions
                        {
                            HttpOnly = true,
                            // Secure = true, // TODO remove this comment
                            Expires = DateTimeOffset.Now.AddDays(10)
                        });
                }

                return new LoginResponse
                {
                    Token = hybrid ? token.TokenBase : token.FullToken
                };
            }

            return Unauthorized();
        }

        [HttpPost("signup")]
        public async Task<ActionResult<SignUpResponse>> SignUp(
            [FromBody] SignUpRequest request,
            [FromServices] UserManager<User> manager)
        {
            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email
            };
            var result = await manager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            return new SignUpResponse { Detail = "User created successfully" };
        }

        public class LoginRequest
        {
            [Required] public string UserName { get; set; } = string.Empty;

            [Required] public string Password { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
            public string Token { get; set; }
        }

        public class SignUpRequest
        {
            [EmailAddress] public string? Email { get; set; }

            [Required]
            [MinLength(1)]
            [MaxLength(100)]
            public string UserName { get; set; }

            [MinLength(8)] public string Password { get; set; }
        }

        public class SignUpResponse
        {
            public string Detail { get; set; }
        }
    }
}