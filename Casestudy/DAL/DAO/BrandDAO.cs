using Casestudy.DAL.DomainClasses;
using Microsoft.EntityFrameworkCore;

namespace Casestudy.DAL.DAO
{
    public class CategoryDAO
    {
        private readonly AppDbContext _db;
        public CategoryDAO(AppDbContext ctx)
        {
            _db = ctx;
        }
        public async Task<List<Brand>> GetAll()
        {
            return await _db.Brand!.ToListAsync();
        }
    }
}
