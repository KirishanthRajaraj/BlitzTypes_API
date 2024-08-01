using BlitzTypes_API.Data;
using BlitzTypes_API.Models;

namespace BlitzTypes_API.Repositories
{
    public class WordsRepository
    {
        public readonly BlitzTypesContext _context;
        public WordsRepository(BlitzTypesContext context)
        {
            _context = context;
        }

        public List<T> GetAllWords<T>() where T : WordBase
        {
            return _context.Set<T>().OrderBy(e => e.Id).ToList();
        }

        public List<T> GetWords<T>(int toSkip, int toTake) where T : WordBase
        {
            var query = _context.Set<T>()
                                .OrderBy(e => Guid.NewGuid())
                                .Skip(toSkip)
                                .Take(toTake);

            return query.ToList();
        }
    }
}
