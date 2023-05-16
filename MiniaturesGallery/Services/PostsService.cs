using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using MiniaturesGallery.Data;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;

namespace MiniaturesGallery.Services
{
    public interface IPostService
    {
        Task<List<Post>> GetAsync();
        Task<PaginatedList<Post>> GetAsyncSortedBy(string searchString, string orderByFilter, DateTime dateFrom, DateTime dateTo, int? pageNumber, string UserID);
        Task<Post> GetAsync(int id, string UserID);
        Task<int> CreateAsync(Post post, string UserID);
        Task UpdateAsync(Post post);
        Task DeleteAsync(int id);
        bool Exists(int id);
    }

    public class PostsService : IPostService
    {
        private const int _pageSize = 10;
        private readonly ApplicationDbContext _context;
        private readonly IAttachmentsService _attachmentsService;

        public PostsService(ApplicationDbContext context, IAttachmentsService attachmentsService)
        {
            _attachmentsService = attachmentsService;
            _context = context;
        }

        public async Task<List<Post>> GetAsync()
        {
            return await _context.Posts.ToListAsync();
        }

        public async Task<PaginatedList<Post>> GetAsyncSortedBy(string searchString, string orderByFilter, DateTime dateFrom, DateTime dateTo, int? pageNumber, string UserID)
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
                if (orderByFilter != "")
                {
                    int.TryParse(orderByFilter, out int orderByInt);
                    switch ((OrderBy)orderByInt)
                    {
                        case OrderBy.DateDesc:
                            posts = posts.OrderByDescending(x => x.CrateDate);
                            break;
                        case OrderBy.DateAsc:
                            posts = posts.OrderBy(x => x.CrateDate);
                            break;
                        case OrderBy.RatesDesc:
                            posts = posts.OrderByDescending(x => x.Rating);
                            break;
                        case OrderBy.RatesAsc:
                            posts = posts.OrderBy(x => x.Rating);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    posts = posts.OrderByDescending(x => x.ID);
                }
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

            return await PaginatedList<Post>.CreateAsync(posts, pageNumber ?? 1, _pageSize);
        }

        public async Task<Post> GetAsync(int id, string UserID)
        {
            var post = await _context.Posts
                .Include(a => a.Attachments)
                .Include(a => a.User)
                .Include(a => a.Coments)
                .FirstOrDefaultAsync(m => m.ID == id);

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
