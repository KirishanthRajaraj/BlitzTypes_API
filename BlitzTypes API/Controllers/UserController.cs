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
using System.Reflection.Metadata.Ecma335;
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
        public async Task<IActionResult> submitTypingResult([FromBody] ResultModel resultModel)
        {
            try
            {
                var status = await _userService.SetWPMHighscore(resultModel.score, resultModel.typingTime);
                
                status = await _userService.IncrementTestAmount();
                if (!status) return (StatusCode(500, "Internal Server Error"));

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
            var currentUserName = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            User user = await _userManager.FindByNameAsync(currentUserName);

            if (user == null)
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
                user.highScoreWPM_30_sec,
                user.highScoreWPM_60_sec,
                user.joinedDate,
                user.preferredLanguage,
                user.preferredTime,
                user.secondsWritten,
                user.testAmount
            });
        }

        [HttpGet]
        public async Task<IActionResult> getAllUsersForLeaderboard()
        {
            try
            {
                List<User> users = _userRepository.GetAllUsers();
                List<LeaderboardUser> candidates = new List<LeaderboardUser>();

                foreach (var user in users)
                {
                    LeaderboardUser leaderboardUser = new LeaderboardUser();
                    List<int> highScores = new List<int>();
                    leaderboardUser.UserName = user.UserName;
                    leaderboardUser.Id = user.Id;
                     
                    var highestHighscore = new List<int>
                    {
                        user.highScoreWPM_15_sec ?? 0,
                        user.highScoreWPM_30_sec ?? 0,
                        user.highScoreWPM_60_sec ?? 0
                    }.Max();


                    leaderboardUser.highScoreWPM = highestHighscore;
                    candidates.Add(leaderboardUser);
                }

                candidates = candidates.OrderByDescending(candidate => candidate.highScoreWPM).Where(c => c.highScoreWPM > 0).ToList();

                return Ok(candidates);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }

        }



    }
}
