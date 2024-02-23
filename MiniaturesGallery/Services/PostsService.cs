using Microsoft.EntityFrameworkCore;
using MiniaturesGallery.Data;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;

namespace MiniaturesGallery.Services
{
    public interface IPostService
    {
        IQueryable<Post> GetAll();
        IQueryable<Post> Get(string? searchString, string? orderByFilter, DateTime? dateFrom, DateTime? dateTo, string UserID);
        IQueryable<Post> GetOfUser(string filtrUserID, string UserID);
        Post Get(int id, string UserID);
        int Create(Post post, string UserID);
        void Update(Post post);
        void Delete(int id);
        bool Exists(int id);
        bool Any();
    }

    public class PostsService : IPostService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAttachmentsService _attachmentsService;
        private readonly ILogger<AttachmentsService> _logger;

        public PostsService(ApplicationDbContext context, IAttachmentsService attachmentsService, ILogger<AttachmentsService> logger)
        {
            _attachmentsService = attachmentsService;
            _context = context;
            _logger = logger;
        }

        public IQueryable<Post> GetOfUser(string filtrUserID, string UserID)
        {
            IQueryable<Post> posts;

            posts = _context.PostsAbs.OfType<Post>()
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

        public IQueryable<Post> GetAll()
        {
            IQueryable<Post> posts;

            posts = _context.PostsAbs.OfType<Post>()
                    .Include(a => a.Attachments)
                    .Include(a => a.User)
                    .AsQueryable();

            foreach (Post post in posts)
            {
                post.NoOfComments = _context.Comments.Count(x => x.PostID == post.ID);

                post.Rates = new List<Rate>();
                post.NoOfRates = _context.Rates.Count(x => x.PostID == post.ID);
            }

            return posts;
        }

        public IQueryable<Post> Get(string? searchString, string? orderByFilter, DateTime? dateFrom, DateTime? dateTo, string UserID)
        {
            IQueryable<Post> posts;
            if (String.IsNullOrEmpty(searchString) == false)
                posts = _context.PostsAbs.OfType<Post>()
                    .Include(a => a.Attachments)
                    .Include(a => a.User)
                    .AsQueryable()
                    .Where(s => s.Topic.Contains(searchString)
                    || s.Text.Contains(searchString)
                    );
            else
                posts = _context.PostsAbs.OfType<Post>()
                    .Include(a => a.Attachments)
                    .Include(a => a.User)
                    .AsQueryable();

            DateTime dateFromTmp = ((dateFrom == DateTime.MinValue || dateFrom is null) ? DateTime.Today.AddMonths(-1) : (DateTime)dateFrom);
            DateTime dateToTmp = ((dateTo == DateTime.MinValue || dateTo is null) ? DateTime.Today : (DateTime)dateTo);
            
            posts = posts.Where(x => x.CrateDate >= dateFromTmp && x.CrateDate < dateToTmp.AddDays(1));

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
            var post = _context.PostsAbs.OfType<Post>()
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

        public int Create(Post post, string UserID)
        {
            post.CrateDate = DateTime.Now;
            post.UserID = UserID;
            _context.Add(post);
            _context.SaveChanges();

            return post.ID;
        }

        public void Update(Post post)
        {
            Post postFromDB = _context.PostsAbs.OfType<Post>().First(x => x.ID == post.ID);
            postFromDB.Topic = post.Topic;
            postFromDB.Text = post.Text;

            _context.Update(postFromDB);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var post = _context.PostsAbs.OfType<Post>()
                .Include(a => a.Attachments)
                .Include(a => a.User)
                .Include(a => a.Coments)
                .Include(a => a.Rates)
                .FirstOrDefault(m => m.ID == id);

            _logger.LogInformation($"Post ID: {id} Of: {post.UserID} DELETE invoked");

            _attachmentsService.DeleteAll(post.ID);
            _context.PostsAbs.Remove(post);
            _context.SaveChanges();
        }

        public bool Exists(int id)
        {
            return _context.PostsAbs.OfType<Post>().Any(e => e.ID == id);
        }

        public bool Any()
        {
            return _context.PostsAbs.Any();
        }
    }
}
