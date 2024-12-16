using Microsoft.EntityFrameworkCore;
using PoS_Placeholder.Server.Data;
using PoS_Placeholder.Server.Models;

namespace PoS_Placeholder.Server.Repositories;

public class GiftcardRepository : Repository<Giftcard>
{
    public GiftcardRepository(ApplicationDbContext db) : base(db)
    {
    }

    public async Task<Giftcard> GetGiftcardByIdAndBusinessIdAsync(string giftcardId, int businessId)
    {
        return await _db.Giftcards.FirstOrDefaultAsync(g => g.Id == giftcardId && g.BusinessId == businessId);
    }
    
    
}