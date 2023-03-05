using BlitzTypes_API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BlitzTypes_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class EnglishWordsController : Controller
    {
        private readonly EnglishWordsRepository _englishWordsRepository;

        public EnglishWordsController(EnglishWordsRepository englishWordsRepository)
        {
            _englishWordsRepository = englishWordsRepository;
        }

        [HttpGet]
        public IActionResult GetAllEnglishWords()
        { 
            var englishWords = _englishWordsRepository.GetAllEnglishWords();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(englishWords);
        }

        [HttpGet]
        public IActionResult GetEnglishWords(int ToSkip, int ToTake)
        {

            var englishWords = _englishWordsRepository.GetEnglishWords(ToSkip, ToTake);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(englishWords);
        }
    }
}
