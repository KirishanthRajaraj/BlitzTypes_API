using BlitzTypes_API.Data;
using BlitzTypes_API.Models.Authentication;
using BlitzTypes_API.Repositories;
using BlitzTypes_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace BlitzTypes_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AuthenticationController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BlitzTypesContext _context;
        private readonly UserRepository _userRepository;

        public AuthenticationController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration,  IHttpContextAccessor httpContextAccessor, BlitzTypesContext
             context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _userService = new UserService(userManager, httpContextAccessor, context);
            _userRepository = new UserRepository(context, userManager);
        }

        // create createToken method

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Authentication", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            properties.AllowRefresh = true;
            properties.Items["ReturnUrl"] = returnUrl;

            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                return BadRequest(new { error = "Remote error occurred" });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return BadRequest(new { error = "No external login info found" });
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(info.Principal.FindFirstValue(ClaimTypes.Email));
                var token = GenerateJwtToken(user);
                Guid refreshToken = CreateRefreshToken();
                bool isRefreshTokenSet = await _userService.SetRefreshTokenAsync(refreshToken, null);
                if (!isRefreshTokenSet)
                {
                    return Redirect(Uri.UnescapeDataString(returnUrl) ?? "/");
                }
                _userService.SetCookie(token, refreshToken);
                return Redirect(Uri.UnescapeDataString(returnUrl) ?? "/");
            }
            else if (result.IsLockedOut)
            {
                return StatusCode(423, new { status = "User is locked out" });
            }
            else if (result.RequiresTwoFactor)
            {
                return StatusCode(401, new { status = "Two-factor authentication required" });
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName);

                var user = new User
                {
                    UserName = givenName,
                    Email = email,
                };

                var createResult = await _userManager.CreateAsync(user);
                if (createResult.Succeeded)
                {
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (addLoginResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        var token = GenerateJwtToken(user);
                        Guid refreshToken = CreateRefreshToken();
                        bool isRefreshTokenSet = await _userService.SetRefreshTokenAsync(refreshToken, null);
                        if (!isRefreshTokenSet)
                        {
                            return Redirect(Uri.UnescapeDataString(returnUrl) ?? "/");
                        }
                        _userService.SetCookie(token, refreshToken);
                        return Redirect(Uri.UnescapeDataString("http://" + returnUrl) ?? "/");
                    }
                    else
                    {
                        return BadRequest(new { error = "Failed to add external login" });
                    }
                }
                else
                {
                    return BadRequest(new { error = "Failed to create user" });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new User { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var token = GenerateJwtToken(user);
                return Ok(new { Token = token, Message = "User registered successfully!" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("errorDesc", error.Description);
            }

            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> GetToken()
        {
            string refreshTokenStr = await _userService.CreateNewAccessToken();

            if (string.IsNullOrEmpty(refreshTokenStr)) return BadRequest();
            Guid refreshToken = Guid.Parse(refreshTokenStr);
            var user = await _userRepository.GetUserByRefreshTokenHashAsync(refreshToken);
            if(user == null) return BadRequest(ModelState);
            var newToken = GenerateJwtToken(user);

            _userService.SetCookie(newToken, refreshToken);
            
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return BadRequest(ModelState);
            }
           
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(model.Username);
            }

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "No user found");
                return BadRequest(ModelState);
            }
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                var token = GenerateJwtToken(user);
                Guid refreshToken = CreateRefreshToken();
                bool isRefreshTokenSet = await _userService.SetRefreshTokenAsync(refreshToken, user);
                if (!isRefreshTokenSet)
                {
                    return BadRequest();
                }
                _userService.SetCookie(token, refreshToken);
                return Ok();
            }
            else
            {
                return Unauthorized(new { Message = "Invalid login attempt." });
            }
        }

        //logout todo

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [NonAction]
        public Guid CreateRefreshToken()
        {
            // encryption todo
            Guid refreshToken = Guid.NewGuid();
            return refreshToken;
        }

    }
}
