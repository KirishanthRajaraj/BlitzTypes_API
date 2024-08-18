using Azure;
using BlitzTypes_API.Controllers;
using BlitzTypes_API.Data;
using BlitzTypes_API.Models.Authentication;
using BlitzTypes_API.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Net;
using System.Security.Claims;

namespace BlitzTypes_API.Services
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserRepository _userRepository;
        private readonly BlitzTypesContext _context;

        public UserService(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, BlitzTypesContext context)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _userRepository = new UserRepository(context, userManager);
        }

        public async Task<bool> SetWPMHighscore(int? highscore)
        {
            if (highscore < 0)
            {
                return false;
            }
            var claims = _httpContextAccessor.HttpContext?.User?.Claims;
            if (claims == null)
            {
                return false;
            }

            var nameIdentifierClaims = _httpContextAccessor.HttpContext?.User?.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier);

            var userId = nameIdentifierClaims?.LastOrDefault()?.Value;

            if (userId == null)
            {
                return false;
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            if (highscore > user.highScoreWPM || user.highScoreWPM == null)
            {
                user.highScoreWPM = highscore;
                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            return false;
        }

        public async Task<bool> ValidateRefreshTokenAsync(Guid providedRefreshToken)
        {
            var user = await _userRepository.GetUserByRefreshTokenHashAsync(providedRefreshToken);

            if (user != null)
            {
                if (user.refreshToken == providedRefreshToken && user.refreshTokenExpiry > DateTime.Now)
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        public async Task<bool> SetRefreshTokenAsync(Guid providedRefreshToken, User? _user)
        {
            var user = _user;
            if (_user == null)
            {
                user = await _userRepository.GetUserByRefreshTokenHashAsync(providedRefreshToken);
            }

            if (user == null)
            {
                return false;
            }

            user.refreshToken = providedRefreshToken;
            user.refreshTokenExpiry = DateTime.Now.AddDays(30);
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                SetCookie("", providedRefreshToken);
                return true;
            }

            return false;
        }

        public async Task<string> CreateNewAccessToken()
        {
            var context = _httpContextAccessor.HttpContext;
            var refreshTokenCookie = context?.Request.Cookies["RefreshToken"];
            if (refreshTokenCookie == null)
            {
                return "";
            }
            var validateResult = await ValidateRefreshTokenAsync(Guid.Parse(refreshTokenCookie));
            if (!validateResult) return "";
            Guid refreshToken = CreateRefreshToken();
            var user = await _userRepository.GetUserByRefreshTokenHashAsync(Guid.Parse(refreshTokenCookie));

            var refreshTokenIsSet = await SetRefreshTokenAsync(refreshToken, user);
            if (refreshTokenIsSet)
            {
                return refreshToken.ToString();
            }
            return "";
        }

        public Guid CreateRefreshToken()
        {
            // encryption todo
            Guid refreshToken = Guid.NewGuid();
            return refreshToken;
        }

        public bool SetCookie(string token, Guid? refreshToken)
        {
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    _httpContextAccessor.HttpContext.Response.Cookies.Append("JwtToken", token, new CookieOptions
                    {
                        HttpOnly = true,
                        // change in production
                        SameSite = SameSiteMode.None,
                        Secure = true,
                        Expires = DateTime.Now.AddSeconds(15),
                    });
                }

                if (refreshToken.HasValue)
                {
                    _httpContextAccessor.HttpContext.Response.Cookies.Append("RefreshToken", refreshToken.ToString(), new CookieOptions
                    {
                        HttpOnly = true,
                        SameSite = SameSiteMode.None,
                        Secure = true,
                        Expires = DateTime.UtcNow.AddDays(30)
                    });
                }
            }
            catch (Exception ex)
            {
                return false;
            }


            return true;
        }

    }
}
