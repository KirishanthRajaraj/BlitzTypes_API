using BlitzTypes_API.Data;
using BlitzTypes_API.Models;

namespace BlitzTypes_API.Repositories
{
    public class EnglishWordsRepository
    {
        public readonly BlitzTypesContext _context;
        public EnglishWordsRepository(BlitzTypesContext context)
        {
            _context = context;
        }

        public List<EnglishWord> GetAllEnglishWords()
        {
            return _context.EnglishWords.OrderBy(e => e.Id).ToList();
        }

        public List<EnglishWord> GetEnglishWords(int toSkip, int toTake)
        {
            List<EnglishWord> result = new List<EnglishWord>();

            var query = _context.EnglishWords
                                .OrderBy(e => Guid.NewGuid())
                                .Skip(toSkip)
                                .Take(toTake);

            return query.ToList();
        }

        public int AverageWordLength()
        {
            var query = _context.EnglishWords.AsQueryable();
            List<EnglishWord> allEnglishWords = query.ToList();
            var wordLengthSum = 0;
            foreach (var word in allEnglishWords)
            {
                wordLengthSum += word.Words.ToString().Length;
            }
            var averageWordLength = wordLengthSum / allEnglishWords.Count;
            return averageWordLength;
        }
    }
}
