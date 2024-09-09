using Azure;
using BlitzTypes_API.Controllers;
using BlitzTypes_API.Data;
using BlitzTypes_API.Models.Authentication;
using BlitzTypes_API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
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

            if (highscore > user.highScoreWPM_15_sec || user.highScoreWPM_15_sec == null)
            {
                user.highScoreWPM_15_sec = highscore;
                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            return false;
        }

        public async Task<User?> GetCurrentUser()
        {
            var claims = _httpContextAccessor.HttpContext?.User?.Claims;
            if (claims == null)
            {
                return null;
            }

            var nameIdentifierClaims = _httpContextAccessor.HttpContext?.User?.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier);

            var userId = nameIdentifierClaims?.LastOrDefault()?.Value;

            if (userId == null)
            {
                return null;
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            return user;
        }



    }
}
