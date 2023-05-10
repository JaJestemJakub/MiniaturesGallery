using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using MiniaturesGallery.Data;
using MiniaturesGallery.Extensions;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;
using MiniaturesGallery.ViewModels;

namespace MiniaturesGallery.Controllers
{
    public class PostsController : Controller
    {
        private const int _pageSize = 10;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment hostingEnvironment;

        public PostsController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
              return View(await _context.Posts.ToListAsync());
        }

        // GET: Posts
        public async Task<IActionResult> ScrollIndex(string searchString, string orderByFilter, DateTime dateFrom, DateTime dateTo, int? pageNumber)
        {
            ViewBag.SearchString = searchString;
            ViewBag.OrderByFilter = orderByFilter;
            if (dateFrom == DateTime.MinValue)
                dateFrom = DateTime.Today.AddMonths(-1);
            ViewBag.DateFrom = dateFrom.ToString("yyyy-MM-dd");
            if (dateTo == DateTime.MinValue)
                dateTo = DateTime.Today;
            ViewBag.DateTo = dateTo.ToString("yyyy-MM-dd");

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
                if (_context.Rates.Any(x => x.PostID == post.ID && x.UserID == User.GetLoggedInUserId<string>()))
                {
                    post.Rates.Add(_context.Rates.First(x => x.PostID == post.ID && x.UserID == User.GetLoggedInUserId<string>()));
                }
            }

            return View(await PaginatedList<Post>.CreateAsync(posts, pageNumber ?? 1, _pageSize));
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }
            
            var post = await _context.Posts
                .Include(a => a.Attachments)
                .Include(a => a.User)
                .Include(a => a.Coments)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (post == null)
            {
                return NotFound();
            }

            foreach(var comment in post.Coments)
            {
                if (comment.UserID != null)
                    comment.User = _context.Users.FirstOrDefault(x => x.Id == comment.UserID);
                if(comment.CommentID != null)
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
            if (_context.Rates.Any(x => x.PostID == post.ID && x.UserID == User.GetLoggedInUserId<string>()))
            {
                post.Rates.Add(_context.Rates.First(x => x.PostID == post.ID && x.UserID == User.GetLoggedInUserId<string>()));
            }

            return View(post);
        }

        // GET: Posts/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("ID,Topic,Text")] Post post)
        {
            if (ModelState.IsValid)
            {
                post.CrateDate = DateTime.Now;
                post.UserID = User.GetLoggedInUserId<string>();
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Edit), new { id = post.ID });
            }
            return View(post);
        }

        // GET: Posts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(a => a.Attachments)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (post == null)
            {
                return NotFound();
            }

            post.Rates = new List<Rate>();
            post.NoOfRates = _context.Rates.Count(x => x.PostID == post.ID);
            if (_context.Rates.Any(x => x.PostID == post.ID && x.UserID == User.GetLoggedInUserId<string>()))
            {
                post.Rates.Add(_context.Rates.First(x => x.PostID == post.ID && x.UserID == User.GetLoggedInUserId<string>()));
            }

            PostEditViewModel viewModel = new PostEditViewModel { Post = post };
            return View(viewModel);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Topic,Text")] Post post)
        {
            if (id != post.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Post postFromDB = await _context.Posts.FirstAsync(x => x.ID == post.ID);
                    postFromDB.Topic = post.Topic;
                    postFromDB.Text = post.Text;
                    _context.Update(postFromDB);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = post.ID });
            }
            return View(post);
        }

        // GET: Posts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(a => a.Attachments)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Posts'  is null.");
            }
            var post = await _context.Posts
                .Include(a => a.Attachments)
                .Include(a => a.Coments)
                .FirstAsync(x => x.ID == id);
            if (post != null)
            {
                if(post.Attachments != null && post.Attachments.Any()) //Delete all attachments if any
                {
                    string FolderPath = Path.Combine(hostingEnvironment.WebRootPath, "Files", post.ID.ToString());
                    foreach (var att in post.Attachments)
                    {
                        string FilePath = Path.Combine(hostingEnvironment.WebRootPath, "Files", att.FullFileName);
                        
                        System.IO.File.Delete(FilePath);
                    }
                    string[] files = Directory.GetFiles(FolderPath);
                    if (files.Length == 0)
                        Directory.Delete(FolderPath);
                }
                _context.Posts.Remove(post);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddAttachment(int id, [Bind("ID", "Files")] PostEditViewModel postViewModel)
        {
            if (ModelState.IsValid)
            {
                if (postViewModel.Files != null && postViewModel.Files.Count > 0)
                {
                    foreach (IFormFile f in postViewModel.Files)
                    {
                        if(f.IsImage())
                        {
                            string FolderPath = Path.Combine(hostingEnvironment.WebRootPath, "Files", id.ToString());
                            if (Directory.Exists(FolderPath) == false)
                                Directory.CreateDirectory(FolderPath);
                            string FolderSlashFile = Path.Combine(id.ToString(), f.FileName);
                            string FilePath = Path.Combine(hostingEnvironment.WebRootPath, "Files", FolderSlashFile);

                            using (FileStream fs = new FileStream(FilePath, FileMode.Create))
                                f.CopyTo(fs);
                            Attachment att = new Attachment(User.GetLoggedInUserId<string>()) { FileName = f.FileName, FullFileName = FolderSlashFile, PostID = id };
                            _context.Add(att);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Edit), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            Attachment att = await _context.Attachments
                .Include(a => a.Post)
                .FirstAsync(x => x.ID == id);
            _context.Attachments.Remove(att);

            string FilePath = Path.Combine(hostingEnvironment.WebRootPath, "Files", att.FullFileName);
            string FolderPath = Path.Combine(hostingEnvironment.WebRootPath, "Files", att.PostID.ToString());

            System.IO.File.Delete(FilePath);
            string[] files = Directory.GetFiles(FolderPath);
            if (files.Length == 0)
                Directory.Delete(FolderPath);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Edit), new { id = att.PostID });
        }

        private bool PostExists(int id)
        {
          return _context.Posts.Any(e => e.ID == id);
        }
    }
}
