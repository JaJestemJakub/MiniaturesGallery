using Microsoft.EntityFrameworkCore;
using MiniaturesGallery.Data;
using MiniaturesGallery.Models;

namespace MiniaturesGallery.Services
{
    public interface IRatesService
    {
        List<Rate> Get();
        Rate Get(int id);
        int Create(Rate rate);
        void Update(Rate rate);
        void Delete(int id);
        bool Exists(int id);
    }
    public class RatesService : IRatesService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AttachmentsService> _logger;

        public RatesService(ApplicationDbContext context, ILogger<AttachmentsService> logger)
        {
            _context = context;
            _logger = logger;

        }

        public int Create(Rate rate)
        {
            _context.Add(rate);
            _context.SaveChanges();

            ActualizeRating(rate.PostID);

            return rate.ID;
        }

        public void Delete(int id)
        {
            var rate = _context.Rates.FirstOrDefault(m => m.ID == id);

            _logger.LogInformation($"Rate ID: {rate.ID} PostID: {rate.PostID} Of: {rate.UserID} DELETE invoked");

            _context.Rates.Remove(rate);
            _context.SaveChanges();

            ActualizeRating(rate.PostID);
        }

        public bool Exists(int id)
        {
            return _context.Rates.Any(e => e.ID == id);
        }

        public List<Rate> Get()
        {
            return _context.Rates.ToList();
        }

        public Rate Get(int id)
        {
            return _context.Rates.FirstOrDefault(m => m.ID == id);
        }

        public void Update(Rate rate)
        {
            var rateFromDB =  _context.Rates.FirstOrDefault(m => m.ID == rate.ID);

            rateFromDB.Rating = rate.Rating;
            _context.Update(rateFromDB);
            _context.SaveChanges();

            ActualizeRating(rateFromDB.PostID);
        }

        private bool ActualizeRating(int? id)
        {
            if (id == null)
            {
                return false;
            }

            Post post = _context.PostsAbs.OfType<Post>()
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
