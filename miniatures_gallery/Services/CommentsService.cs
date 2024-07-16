using Microsoft.EntityFrameworkCore;
using MiniaturesGallery.Data;
using MiniaturesGallery.Models;

namespace MiniaturesGallery.Services
{
    public interface ICommentsService
    {
        IQueryable<Comment> GetAll();
        Comment Get(int id);
        int Create(Comment comment);
        void Update(Comment comment);
        void Delete(int id);
        bool Exists(int id);
    }

    public class CommentsService : ICommentsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AttachmentsService> _logger;

        public CommentsService(ApplicationDbContext context, ILogger<AttachmentsService> logger)
        {
            _context = context;
            _logger = logger;

        }
        public int Create(Comment comment)
        {
            comment.CrateDate = DateTime.Now;
            _context.Add(comment);
            _context.SaveChanges();
            return comment.ID;
        }

        public void Delete(int id)
        {
            var comment = _context.Comments.Include(x => x.Comments).FirstOrDefault(m => m.ID == id);

            if(comment.Comments.Any())
            {
                comment.Body = "Deleted.";
                _context.Update(comment);
                _context.SaveChanges();
            }
            else
            {
                _logger.LogInformation($"Comment ID: {id} PostID: {comment.PostID} CommentID: {comment.CommentID} Of: {comment.UserID} DELETE invoked");

                _context.Comments.Remove(comment);
                _context.SaveChanges();
            }
        }

        public bool Exists(int id)
        {
            return (_context.Comments?.Any(e => e.ID == id)).GetValueOrDefault();
        }

        public IQueryable<Comment> GetAll()
        {
            return _context.Comments.AsQueryable();
        }

        public Comment Get(int id)
        {
            return _context.Comments.FirstOrDefault(m => m.ID == id);
        }

        public void Update(Comment comment)
        {
            Comment commentFromDB = _context.Comments.FirstOrDefault(m => m.ID == comment.ID);
            commentFromDB.Body = comment.Body;
            _context.Update(commentFromDB);
            _context.SaveChanges();
        }
    }
}
