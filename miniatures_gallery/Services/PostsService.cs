using Microsoft.EntityFrameworkCore;
using MiniaturesGallery.Data;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;

namespace MiniaturesGallery.Services
{
    public interface IPostService
    {
        IQueryable<PostAbs> GetAll();
        IQueryable<PostAbs> Get(string? searchString, string? orderByFilter, DateTime? dateFrom, DateTime? dateTo, string UserID);
        IQueryable<PostAbs> GetOfUser(string filtrUserID, string UserID);
        PostAbs Get(int id, string UserID);
        int Create(PostAbs post, string UserID);
        void Update(PostAbs post);
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

        public IQueryable<PostAbs> GetOfUser(string filtrUserID, string UserID)
        {
            IQueryable<PostAbs> posts;

            posts = _context.PostsAbs
                .Include(a => (a as Post).Attachments)
                .Include(a => a.User)
                .AsQueryable()
                .Where(x => x.UserID == filtrUserID)
                .OrderByDescending(x => x.ID);

            foreach (PostAbs postAbs in posts)
            {
                if(postAbs is Post)
                {
                    Post post = postAbs as Post;
                    post.NoOfComments = _context.Comments.Count(x => x.PostID == post.ID);

                    post.Rates = new List<Rate>();
                    post.NoOfRates = _context.Rates.Count(x => x.PostID == post.ID);
                    if (_context.Rates.Any(x => x.PostID == post.ID && x.UserID == UserID))
                    {
                        post.Rates.Add(_context.Rates.First(x => x.PostID == post.ID && x.UserID == UserID));
                    }
                }
            }

            return posts;
        }

        public IQueryable<PostAbs> GetAll()
        {
            IQueryable<PostAbs> posts;

            posts = _context.PostsAbs
                    .Include(a => (a as Post).Attachments)
                    .Include(a => a.User)
                    .AsQueryable();


            foreach (PostAbs postAbs in posts)
            {
                if (postAbs is Post)
                {
                    Post post = postAbs as Post;
                    post.NoOfComments = _context.Comments.Count(x => x.PostID == post.ID);

                    post.Rates = new List<Rate>();
                    post.NoOfRates = _context.Rates.Count(x => x.PostID == post.ID);
                }
            }

            return posts;
        }

        public IQueryable<PostAbs> Get(string? searchString, string? orderByFilter, DateTime? dateFrom, DateTime? dateTo, string UserID)
        {
            IQueryable<PostAbs> postsAbs;
            if (String.IsNullOrEmpty(searchString) == false)
                postsAbs = _context.PostsAbs
                    .Include(a => (a as Post).Attachments)
                    .Include(a => a.User)
                    .AsQueryable()
                    .Where(s => s.Topic.Contains(searchString)
                    || s.Text.Contains(searchString)
                    );
            else
                postsAbs = _context.PostsAbs
                    .Include(a => (a as Post).Attachments)
                    .Include(a => a.User)
                    .AsQueryable();

            DateTime dateFromTmp = ((dateFrom == DateTime.MinValue || dateFrom is null) ? DateTime.Today.AddMonths(-1) : (DateTime)dateFrom);
            DateTime dateToTmp = ((dateTo == DateTime.MinValue || dateTo is null) ? DateTime.Today : (DateTime)dateTo);
            
            postsAbs = postsAbs.Where(x => x.CrateDate >= dateFromTmp && x.CrateDate < dateToTmp.AddDays(1));

            if (String.IsNullOrEmpty(orderByFilter) == false)
            {
                bool desc = orderByFilter.EndsWith("_desc") ? true : false;
                string sortProperty = orderByFilter.EndsWith("_desc") ? orderByFilter.Replace("_desc", "") : orderByFilter;

                if(sortProperty == nameof(Post.Rating))
                {
                    postsAbs = postsAbs.OfType<Post>(); //only Post have Rating
                    postsAbs = (postsAbs as IQueryable<Post>).OrderBy(sortProperty, desc);
                }
                else
                {
                    postsAbs = postsAbs.OrderBy(sortProperty, desc);
                }
            }
            else
            {
                postsAbs = postsAbs.OrderBy(nameof(Post.ID), true);
            }

            foreach (PostAbs postAbs in postsAbs)
            {
                if (postAbs is Post)
                {
                    Post post = postAbs as Post;
                    post.NoOfComments = _context.Comments.Count(x => x.PostID == post.ID);

                    post.Rates = new List<Rate>();
                    post.NoOfRates = _context.Rates.Count(x => x.PostID == post.ID);
                    if (_context.Rates.Any(x => x.PostID == post.ID && x.UserID == UserID))
                    {
                        post.Rates.Add(_context.Rates.First(x => x.PostID == post.ID && x.UserID == UserID));
                    }
                }
            }

            return postsAbs;
        }

        public PostAbs Get(int id, string UserID)
        {
            var postAbs = _context.PostsAbs
                .Include(a => (a as Post).Attachments)
                .Include(a => a.User)
                .Include(a => (a as Post).Coments)
                .FirstOrDefault(m => m.ID == id);

            if(postAbs is Post)
            {
                Post post = postAbs as Post;
                if (post.Coments != null)
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

            return postAbs;
        }

        public int Create(PostAbs post, string UserID)
        {
            post.CrateDate = DateTime.Now;
            post.UserID = UserID;
            _context.Add(post);
            _context.SaveChanges();

            return post.ID;
        }

        public void Update(PostAbs post)
        {
            PostAbs postFromDB = _context.PostsAbs.First(x => x.ID == post.ID);
            postFromDB.Topic = post.Topic;
            postFromDB.Text = post.Text;

            _context.Update(postFromDB);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var post = _context.PostsAbs
                .Include(a => (a as Post).Attachments)
                .Include(a => a.User)
                .Include(a => (a as Post).Coments)
                .Include(a => (a as Post).Rates)
                .FirstOrDefault(m => m.ID == id);

            _logger.LogInformation($"Post ID: {id} Of: {post.UserID} DELETE invoked");

            _attachmentsService.DeleteAll(post.ID);
            _context.PostsAbs.Remove(post);
            _context.SaveChanges();
        }

        public bool Exists(int id)
        {
            return _context.PostsAbs.Any(e => e.ID == id);
        }

        public bool Any()
        {
            return _context.PostsAbs.Any();
        }
    }
}
