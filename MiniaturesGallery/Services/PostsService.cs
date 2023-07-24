using Microsoft.EntityFrameworkCore;
using MiniaturesGallery.Data;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;

namespace MiniaturesGallery.Services
{
    public interface IPostService
    {
        IQueryable<Post> Get();
        IQueryable<Post> GetSortedBy(string searchString, string orderByFilter, DateTime dateFrom, DateTime dateTo, int? pageNumber, string UserID);
        IQueryable<Post> GetOfUser(string filtrUserID, int? pageNumber, string UserID);
        Post Get(int id, string UserID);
        Task<int> CreateAsync(Post post, string UserID);
        Task UpdateAsync(Post post);
        Task DeleteAsync(int id);
        bool Exists(int id);
    }

    public class PostsService : IPostService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAttachmentsService _attachmentsService;

        public PostsService(ApplicationDbContext context, IAttachmentsService attachmentsService)
        {
            _attachmentsService = attachmentsService;
            _context = context;
        }

        public IQueryable<Post> Get()
        {
            return _context.Posts.AsQueryable();
        }

        public IQueryable<Post> GetOfUser(string filtrUserID, int? pageNumber, string UserID)
        {
            IQueryable<Post> posts;

            posts = _context.Posts
                .Include(a => a.Attachments)
                .Include(a => a.User)
                .AsQueryable()
                .Where(x => x.UserID == filtrUserID)
                .OrderByDescending(x => x.ID);

            foreach (Post post in posts)
            {
                post.NoOfComments = _context.Comments.Count(x => x.PostID == post.ID);

                post.Rates = new List<Rate>();
                post.NoOfRates = _context.Rates.Count(x => x.PostID == post.ID);
                if (_context.Rates.Any(x => x.PostID == post.ID && x.UserID == UserID))
                {
                    post.Rates.Add(_context.Rates.First(x => x.PostID == post.ID && x.UserID == UserID));
                }
            }

            return posts;
        }

        public IQueryable<Post> GetSortedBy(string searchString, string orderByFilter, DateTime dateFrom, DateTime dateTo, int? pageNumber, string UserID)
        {
            IQueryable<Post> posts;
            if (String.IsNullOrEmpty(searchString) == false)
                posts = _context.Posts
                    .Include(a => a.Attachments)
                    .Include(a => a.User)
                    .AsQueryable()
                    .Where(s => s.Topic.Contains(searchString)
                    || s.Text.Contains(searchString)
                    );
            else
                posts = _context.Posts
                    .Include(a => a.Attachments)
                    .Include(a => a.User)
                    .AsQueryable();

            if (dateFrom != DateTime.MinValue && dateTo != DateTime.MinValue)
            {
                posts = posts.Where(x => x.CrateDate >= dateFrom && x.CrateDate < dateTo.AddDays(1));
            }

            if (String.IsNullOrEmpty(orderByFilter) == false)
            {
                bool desc = orderByFilter.EndsWith("_desc") ? true : false;
                string sortProperty = orderByFilter.EndsWith("_desc") ? orderByFilter.Replace("_desc", "") : orderByFilter;

                posts = posts.OrderBy(sortProperty, desc);
            }
            else
            {
                posts = posts.OrderBy(nameof(Post.ID), true);
            }

            foreach (Post post in posts)
            {
                post.NoOfComments = _context.Comments.Count(x => x.PostID == post.ID);

                post.Rates = new List<Rate>();
                post.NoOfRates = _context.Rates.Count(x => x.PostID == post.ID);
                if (_context.Rates.Any(x => x.PostID == post.ID && x.UserID == UserID))
                {
                    post.Rates.Add(_context.Rates.First(x => x.PostID == post.ID && x.UserID == UserID));
                }
            }

            return posts;
        }

        public Post Get(int id, string UserID)
        {
            var post = _context.Posts
                .Include(a => a.Attachments)
                .Include(a => a.User)
                .Include(a => a.Coments)
                .FirstOrDefault(m => m.ID == id);

            if(post.Coments != null)
            {
                foreach (var comment in post.Coments)
                {
                    if (comment.UserID != null)
                        comment.User = _context.Users.FirstOrDefault(x => x.Id == comment.UserID);
                    if (comment.CommentID != null)
                    {
                        var parent = post.Coments.FirstOrDefault(x => x.ID == comment.CommentID);
                        if (parent != null)
                        {
                            parent.Comments.Add(comment);
                        }
                        post.Coments.Remove(comment);
                    }
                }
            }

            post.Rates = new List<Rate>();
            post.NoOfRates = _context.Rates.Count(x => x.PostID == post.ID);
            if (_context.Rates.Any(x => x.PostID == post.ID && x.UserID == UserID))
            {
                post.Rates.Add(_context.Rates.First(x => x.PostID == post.ID && x.UserID == UserID));
            }

            return post;
        }

        public async Task<int> CreateAsync(Post post, string UserID)
        {
            post.CrateDate = DateTime.Now;
            post.UserID = UserID;
            _context.Add(post);
            await _context.SaveChangesAsync();

            return post.ID;
        }

        public async Task UpdateAsync(Post post)
        {
            Post postFromDB = await _context.Posts.FirstAsync(x => x.ID == post.ID);
            postFromDB.Topic = post.Topic;
            postFromDB.Text = post.Text;

            _context.Update(postFromDB);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var post = await _context.Posts
                .Include(a => a.Attachments)
                .Include(a => a.User)
                .Include(a => a.Coments)
                .Include(a => a.Rates)
                .FirstOrDefaultAsync(m => m.ID == id);

            await _attachmentsService.DeleteAllAsync(post.ID);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        public bool Exists(int id)
        {
            return _context.Posts.Any(e => e.ID == id);
        }
    }
}
