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
            var query = _context.EnglishWords.OrderBy(e => Guid.NewGuid()).AsQueryable();
            query = query
                .Skip(toSkip)
                .Take(toTake);
            return query.ToList();
        }
    }
}
