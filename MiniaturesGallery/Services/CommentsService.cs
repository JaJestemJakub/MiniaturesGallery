using Microsoft.EntityFrameworkCore;
using MiniaturesGallery.Data;
using MiniaturesGallery.Models;

namespace MiniaturesGallery.Services
{
    public interface ICommentsService
    {
        Task<List<Comment>> GetAsync();
        Task<Comment> GetAsync(int id);
        Task<int> CreateAsync(Comment comment);
        Task UpdateAsync(Comment comment);
        Task DeleteAsync(int id);
        bool Exists(int id);
    }

    public class CommentsService : ICommentsService
    {
        private readonly ApplicationDbContext _context;

        public CommentsService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<int> CreateAsync(Comment comment)
        {
            comment.CrateDate = DateTime.Now;
            _context.Add(comment);
            await _context.SaveChangesAsync();
            return comment.ID;
        }

        public async Task DeleteAsync(int id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(m => m.ID == id);
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        public bool Exists(int id)
        {
            return (_context.Comments?.Any(e => e.ID == id)).GetValueOrDefault();
        }

        public async Task<List<Comment>> GetAsync()
        {
            return await _context.Comments.ToListAsync();
        }

        public async Task<Comment> GetAsync(int id)
        {
            return await _context.Comments.FirstOrDefaultAsync(m => m.ID == id);
        }

        public async Task UpdateAsync(Comment comment)
        {
            Comment commentFromDB = await _context.Comments.FirstOrDefaultAsync(m => m.ID == comment.ID);
            commentFromDB.Body = comment.Body;
            _context.Update(commentFromDB);
            await _context.SaveChangesAsync();
        }
    }
}
