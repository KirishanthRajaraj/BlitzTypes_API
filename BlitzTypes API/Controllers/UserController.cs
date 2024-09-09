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
            _context = context;
            _userService = new UserService(userManager, httpContextAccessor, context);
            _userRepository = new UserRepository(context, userManager);
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
            ClaimsPrincipal currentUser = this.User;
            var currentUserName = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = await _userManager.FindByNameAsync(currentUserName);

            if(user == null)
            {
                return Unauthorized();
            }

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.highScoreAccuracy,
                user.highScoreWPM_15_sec,
                user.joinedDate,
                user.preferredLanguage,
                user.preferredTime,
                user.secondsWritten,
            });
        }

        [HttpGet]
        public async Task<IActionResult> getAllUsersForLeaderboard()
        {
            try
            {
                List<User> candidates = _userRepository.GetAllUsers();
                var result = candidates
                .Where(user => user.highScoreWPM_15_sec > 0)
                .Select(user => new
                {
                    user.UserName,
                    user.highScoreWPM_15_sec,
                    user.Id
                })
                .OrderByDescending(user => user.highScoreWPM_15_sec)
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
