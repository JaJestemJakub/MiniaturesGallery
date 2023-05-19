using Microsoft.EntityFrameworkCore;
using MiniaturesGallery.Data;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;

namespace MiniaturesGallery.Services
{
    public interface IRatesService
    {
        Task<List<Rate>> GetAsync();
        Task<Rate> GetAsync(int id);
        Task CreateAsync(Rate rate);
        Task UpdateAsync(Rate rate);
        Task DeleteAsync(int id);
        bool Exists(int id);
    }
    public class RatesService : IRatesService
    {
        private readonly ApplicationDbContext _context;

        public RatesService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Rate rate)
        {
            _context.Add(rate);
            await _context.SaveChangesAsync();

            ActualizeRating(rate.PostID);
        }

        public async Task DeleteAsync(int id)
        {
            var rate = await _context.Rates.FirstOrDefaultAsync(m => m.ID == id);

            _context.Rates.Remove(rate);
            await _context.SaveChangesAsync();

            ActualizeRating(rate.PostID);
        }

        public bool Exists(int id)
        {
            return _context.Rates.Any(e => e.ID == id);
        }

        public async Task<List<Rate>> GetAsync()
        {
            return await _context.Rates.ToListAsync();
        }

        public async Task<Rate> GetAsync(int id)
        {
            return await _context.Rates.FirstOrDefaultAsync(m => m.ID == id);
        }

        public async Task UpdateAsync(Rate rate)
        {
            var rateFromDB = await _context.Rates.FirstOrDefaultAsync(m => m.ID == rate.ID);

            rateFromDB.Rating = rate.Rating;
            _context.Update(rateFromDB);
            await _context.SaveChangesAsync();

            ActualizeRating(rateFromDB.PostID);
        }

        private bool ActualizeRating(int? id)
        {
            if (id == null)
            {
                return false;
            }

            Post post = _context.Posts
                .Include(a => a.Rates)
                .First(x => x.ID == id);
            float newRating = 0;
            foreach (var r in post.Rates)
                newRating += r.Rating;

            if (post.Rates.Count > 0)
                post.Rating = newRating / post.Rates.Count;
            else
                post.Rating = 0;

            _context.Update(post);
            _context.SaveChanges();
            return true;
        }
    }
}
