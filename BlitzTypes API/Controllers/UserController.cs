using BlitzTypes_API.Data;
using BlitzTypes_API.Models;
using BlitzTypes_API.Models.Authentication;
using BlitzTypes_API.Repositories;
using BlitzTypes_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.NetworkInformation;
using System.Security.Claims;

namespace BlitzTypes_API.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserService _userService;
        private readonly UserRepository _userRepository;
        private readonly BlitzTypesContext _context;


        public UserController(UserManager<User> userManager, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, BlitzTypesContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _userService = new UserService(userManager, httpContextAccessor);
            _context = context;
            _userRepository = new UserRepository(context);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost]
        public async Task<IActionResult> SetWPMHighscore([FromBody] ResultModel resultModel)
        {
            try
            {
                var status = await _userService.SetWPMHighscore(resultModel.score);

                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet]
        public async Task<IActionResult> GetCurrentUser()
        {
            var claims = _httpContextAccessor.HttpContext?.User?.Claims;
            if (claims == null)
            {
                return NotFound();
            }

            var nameIdentifierClaims = _httpContextAccessor.HttpContext?.User?.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier);

            var userId = nameIdentifierClaims?.LastOrDefault()?.Value;

            if (userId == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email
            });
        }

        [HttpGet]
        public async Task<IActionResult> getAllUsersForLeaderboard()
        {
            try
            {
                List<User> candidates = _userRepository.GetAllUsers();
                var result = candidates
                .Where(user => user.highScoreWPM > 0)
                .Select(user => new
                {
                    user.UserName,
                    user.highScoreWPM
                })
                .OrderByDescending(user => user.highScoreWPM)
                .ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }


        }

    }
}
