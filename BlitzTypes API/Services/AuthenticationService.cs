using BlitzTypes_API.Data;
using BlitzTypes_API.Models;
using BlitzTypes_API.Models.Authentication;
using BlitzTypes_API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BlitzTypes_API.Services
{
    public class AuthenticationService
    {
        private readonly UserManager<User> _userManager;
        private readonly UserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BlitzTypesContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticationService(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, BlitzTypesContext context, IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = new UserRepository(context, userManager);
            _configuration = configuration;
        }


        public async Task<bool> SetRefreshTokenAsync(User? _user, string newRefreshTokenHash, string? oldRefreshTokenHash = null)
        {
            var user = _user;
            if (user == null)
            {
                if(oldRefreshTokenHash != null)
                {
                    user = await _userRepository.GetUserByRefreshTokenHashAsync(oldRefreshTokenHash);
                } else
                {
                    return false;
                }
            }

            if (user == null)
            {
                return false;
            }

            // todo later add user manipulation to separate repository

            user.refreshTokenHash = newRefreshTokenHash;

            user.refreshTokenExpiry = DateTime.Now.AddDays(30);
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                SetCookie("", newRefreshTokenHash);
                return true;
            }

            return false;
        }


        public HashResult CreateHash(string input)
        {
            var salt = GenerateSalt();
            using (var pbkdf2 = new Rfc2898DeriveBytes(input, salt, 1000, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);

                return new HashResult
                {
                    Hash = Convert.ToBase64String(hash),
                    Salt = Convert.ToBase64String(salt)
                };
            }
        }

        public static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// Gets current refreshToken from Cookie
        /// Validates refreshToken expiry
        /// creates and 
        /// stores new refreshToken to user in DB
        /// </summary>
        /// <returns>
        /// A string of the newly successfully generated refresh token.
        /// </returns>
        public async Task<string> CreateNewRefreshTokenProcedure(User? user = null)
        {
            var context = _httpContextAccessor.HttpContext;
            var refreshTokenCookie = context?.Request.Cookies["RefreshToken"];

            if (user == null && !String.IsNullOrEmpty(refreshTokenCookie))
            {
                user = await _userRepository.GetUserByRefreshTokenHashAsync(refreshTokenCookie);
                var validateResult = await ValidateRefreshTokenAsync(refreshTokenCookie);
                if (!validateResult) return "";
            }

            string newRefreshToken = CreateRefreshToken();
            HashResult hashResult = CreateHash(newRefreshToken);
            var refreshTokenIsSet = false;
            if (user != null)
            {
                refreshTokenIsSet = await SetRefreshTokenAsync(user, hashResult.Hash);
            } else { return ""; }
            if (refreshTokenIsSet)
            {
                return hashResult.Hash;
            }
            return "";
        }

        public string CreateRefreshToken()
        {
            Guid refreshToken = Guid.NewGuid();
            return refreshToken.ToString();
        }
        public string GenerateJwtToken(User user)
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

        public bool SetCookie(string token, string? refreshToken, bool rememberMe = true, bool logout = false)
        {
            try
            {
                // AccessToken Cookie
                var cookieOptionsJwtAccessToken = new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Secure = true
                };

                if (!string.IsNullOrEmpty(token))
                {
                    if (rememberMe)
                    {
                        cookieOptionsJwtAccessToken.Expires = DateTime.Now.AddMinutes(15);
                        _httpContextAccessor.HttpContext.Response.Cookies.Append("JwtToken", token, cookieOptionsJwtAccessToken);
                    }
                    else
                    {
                        // if user unselected remember me option, do session storage of cookie
                        _httpContextAccessor.HttpContext.Response.Cookies.Append("JwtToken", token, cookieOptionsJwtAccessToken);
                    }
                }
                else if (logout)
                {
                    _httpContextAccessor.HttpContext.Response.Cookies.Delete("JwtToken");
                }

                var cookieOptionsRefreshToken = new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Secure = true
                };

                if (!string.IsNullOrEmpty(refreshToken))
                {

                    if (rememberMe)
                    {
                        cookieOptionsRefreshToken.Expires = DateTime.Now.AddMinutes(15);
                        _httpContextAccessor.HttpContext.Response.Cookies.Append("RefreshToken", refreshToken, cookieOptionsRefreshToken);
                    }
                    else
                    {
                        // if user unselected remember me option, do   storage of cookie
                        _httpContextAccessor.HttpContext.Response.Cookies.Append("RefreshToken", refreshToken, cookieOptionsRefreshToken);
                    }

                    _httpContextAccessor.HttpContext.Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        SameSite = SameSiteMode.None,
                        Secure = true,
                        Expires = DateTime.UtcNow.AddDays(30)
                    });
                }
                else if (logout)
                {
                    _httpContextAccessor.HttpContext.Response.Cookies.Delete("RefreshToken");
                }
            }
            catch (Exception ex)
            {
                return false;
            }


            return true;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string providedRefreshToken)
        {
            var user = await _userRepository.GetUserByRefreshTokenHashAsync(providedRefreshToken);

            if (user != null)
            {
                if (user.refreshTokenHash == providedRefreshToken && user.refreshTokenExpiry > DateTime.Now)
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        public async Task<bool> logoutUser(User? user)
        {
            bool isCookieSet = SetCookie("", null, false, logout: true);
            if (!isCookieSet) { return false; }
            bool refreshTokenIsRemoved = await _userRepository.removeRefreshTokenFromUser(user);
            if (!refreshTokenIsRemoved) { return false; }
            return true;
        }
    }
}
