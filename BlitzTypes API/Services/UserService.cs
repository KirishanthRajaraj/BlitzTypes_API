using BlitzTypes_API.Data;
using BlitzTypes_API.Models.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlitzTypes_API.Services
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserService _userService;
        public UserService(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
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
    }
}
