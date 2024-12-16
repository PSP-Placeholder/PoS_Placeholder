using PoS_Placeholder.Server.Models;
using Microsoft.EntityFrameworkCore;
using PoS_Placeholder.Server.Data;

namespace PoS_Placeholder.Server.Repositories
{
    public class UserRepository : Repository<User>
    {
        public UserRepository(ApplicationDbContext db) : base(db) { }

        public async Task<User> GetEmployeeByIdAndBusinessAsync(string employeeId, int businessId)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == employeeId && u.BusinessId == businessId);
        }

        public async Task<IEnumerable<User>> GetEmployeesByBusinessIdAsync(int businessId)
        {
            return await _db.Users.Where(u => u.BusinessId == businessId).ToListAsync();
        }
        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
