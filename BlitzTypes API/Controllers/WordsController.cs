using BlitzTypes_API.Models;
using BlitzTypes_API.Models.Enums;
using BlitzTypes_API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BlitzTypes_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class WordsController : Controller
    {
        private readonly WordsRepository _wordsRepository;

        public WordsController(WordsRepository englishWordsRepository)
        {
            _wordsRepository = englishWordsRepository;
        }

        [HttpGet]
        public IActionResult GetAllWords(Language language)
        {
            List<WordBase> words;

            switch (language)
            {
                case Language.English:
                    words = _wordsRepository.GetAllWords<GermanWord>().Cast<WordBase>().ToList();
                    break;
                case Language.German:
                    words = _wordsRepository.GetAllWords<GermanWord>().Cast<WordBase>().ToList();
                    break;
                default:
                    return BadRequest("Unsupported language.");
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(words);
        }

        [HttpGet]
        public IActionResult GetWords(Language language, int ToSkip, int ToTake)
        {
            List<WordBase> words;

            switch (language)
            {
                case Language.English:
                    words = _wordsRepository.GetWords<EnglishWord>(ToSkip, ToTake).Cast<WordBase>().ToList();
                    break;
                case Language.German:
                    words = _wordsRepository.GetWords<GermanWord>(ToSkip, ToTake).Cast<WordBase>().ToList();
                    break;
                default:
                    return BadRequest("Unsupported language.");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(words);
        }

    }
}
