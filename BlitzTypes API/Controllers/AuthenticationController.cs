using BlitzTypes_API.Data;
using BlitzTypes_API.Models;
using BlitzTypes_API.Models.Authentication;
using BlitzTypes_API.Repositories;
using BlitzTypes_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Serilog;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlitzTypes_API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AuthenticationController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;
        private readonly AuthenticationService _authenticationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BlitzTypesContext _context;
        private readonly UserRepository _userRepository;

        public AuthenticationController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, BlitzTypesContext
             context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _userService = new UserService(userManager, httpContextAccessor, context);
            _authenticationService = new AuthenticationService(userManager, httpContextAccessor, context, configuration);
            _userRepository = new UserRepository(context, userManager);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult isAuthenticated()
        {
            if(User.Identity.IsAuthenticated)
            {
                return Ok(new { message = "User is logged in."});
            }
            return Unauthorized();
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
                Log.Error("Remote error occured: {remoteError}", remoteError);
                return BadRequest(new { error = "Remote error occurred" });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                Log.Error("Error finding external login info");
                return BadRequest(new { error = "No external login info found" });
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(info.Principal.FindFirstValue(ClaimTypes.Email));
                var token = _authenticationService.GenerateJwtToken(user);
                string newRefreshToken = await _authenticationService.CreateNewRefreshTokenProcedure(user);
                var isRefreshTokenSet = !string.IsNullOrEmpty(newRefreshToken);
                if (!isRefreshTokenSet)
                {
                    Log.Error("Failed to set refresh token for User with ID {UserId}", user.Id);
                    return Redirect(Uri.UnescapeDataString(returnUrl) ?? "/");
                }
                Log.Information("User with ID {UserId} successfully logged in", user.Id);
                _authenticationService.SetCookie(token, newRefreshToken);
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
                    joinedDate = DateTime.Now
                };

                var createResult = await _userManager.CreateAsync(user);
                if (createResult.Succeeded)
                {
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (addLoginResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        var token = _authenticationService.GenerateJwtToken(user);
                        string newRefreshToken = await _authenticationService.CreateNewRefreshTokenProcedure();
                        var isRefreshTokenSet = !string.IsNullOrEmpty(newRefreshToken);
                        if (!isRefreshTokenSet)
                        {
                            return BadRequest();
                        }
                        if (!isRefreshTokenSet)
                        {
                            Log.Warning("Refresh Token couldn't be set for user with ID {UserId}", user.Id);
                            return Redirect(Uri.UnescapeDataString(returnUrl) ?? "/");
                        }
                        _authenticationService.SetCookie(token, newRefreshToken);
                        Log.Information("User with ID {UserId} successfully logged in", user.Id);
                        return Redirect(Uri.UnescapeDataString("http://" + returnUrl) ?? "/");
                    }
                    else
                    {
                        Log.Warning("Failed to add external login");
                        return BadRequest(new { error = "Failed to add external login" });
                    }
                }
                else
                {
                    Log.Error("Failed to create user from external login method");
                    return BadRequest(new { error = "Failed to create user" });
                }
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new User { UserName = model.Username, Email = model.Email, joinedDate = DateTime.Now };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var token = _authenticationService.GenerateJwtToken(user);
                var refreshToken = await _authenticationService.CreateNewRefreshTokenProcedure(user);
                _authenticationService.SetCookie(token, refreshToken);

                Log.Information("New user with ID {UserId} was successfully created and registered", user.Id);
                return Ok(new { Message = "User registered successfully!" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("error", error.Description);
            }

            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> GetToken()
        {
            string newRefreshToken = await _authenticationService.CreateNewRefreshTokenProcedure();

            if (string.IsNullOrEmpty(newRefreshToken)) return BadRequest();
            var user = await _userRepository.GetUserByRefreshTokenHashAsync(newRefreshToken);
            if (user == null) return BadRequest(ModelState);
            var newAccessToken = _authenticationService.GenerateJwtToken(user);

            _authenticationService.SetCookie(newAccessToken, newRefreshToken);

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                Log.Error("Invalid login attempt {modelState}", ModelState);
                ModelState.AddModelError("error", "Bad Request. Check inputs again.");
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(model.Username);
            }

            if (user == null)
            {
                Log.Error("Invalid loginattempt", model.Username);
                ModelState.AddModelError("error", "Invalid username or password");
                return BadRequest(ModelState);
            }
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                var token = _authenticationService.GenerateJwtToken(user);
                string newRefreshToken = await _authenticationService.CreateNewRefreshTokenProcedure(user);
                var isRefreshTokenSet = !string.IsNullOrEmpty(newRefreshToken);
                if (!isRefreshTokenSet)
                {
                    Log.Error("Failed to set refresh token for User with ID {UserId}", user.Id);
                    return BadRequest();
                }
                _authenticationService.SetCookie(token, newRefreshToken, model.RememberMe);
                Log.Information("User with ID {UserId} successfully logged in", user.Id);
                return Ok();
            }
            else
            {
                Log.Warning("Invalid login attempt {UserId}", user.Id);
                return Unauthorized(new { Error = "Invalid username or password" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var user = await _userService.GetCurrentUser();
            if (user == null)
            {
                var currentRefreshToken = _httpContextAccessor.HttpContext.Request.Cookies["RefreshToken"];
                user = await _userRepository.GetUserByRefreshTokenHashAsync(currentRefreshToken);
            }
            var isUserLoggedOut = await _authenticationService.logoutUser(user);
            if (!isUserLoggedOut) { return StatusCode(500, "An unexpected error occured working with the current HttpContext."); }
            return Ok( new { Message = "Successfully logged out User" });
        }
    }
}
