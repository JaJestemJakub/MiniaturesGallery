using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using MiniaturesGallery.Data;
using MiniaturesGallery.Models;
using MiniaturesGallery.ViewModels;

namespace MiniaturesGallery.Controllers
{
    public class PostsController : Controller
    {
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

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.ID == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Topic,Text")] Post post)
        {
            if (ModelState.IsValid)
            {
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Posts/Edit/5
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

            PostEditViewModel viewModel = new PostEditViewModel { Post = post };
            return View(viewModel);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    _context.Update(post);
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
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Posts'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAttachment(int id, [Bind("ID", "Files")] PostEditViewModel postViewModel)
        {
            if (ModelState.IsValid)
            {
                if (postViewModel.Files != null && postViewModel.Files.Count > 0)
                {
                    foreach (IFormFile f in postViewModel.Files)
                    {
                        string FolderPath = Path.Combine(hostingEnvironment.WebRootPath, "Files", id.ToString());
                        if (Directory.Exists(FolderPath) == false)
                            Directory.CreateDirectory(FolderPath);
                        string FolderSlashFile = Path.Combine(id.ToString(), f.FileName);
                        string FilePath = Path.Combine(hostingEnvironment.WebRootPath, "Files", FolderSlashFile);

                        using (FileStream fs = new FileStream(FilePath, FileMode.Create))
                            f.CopyTo(fs);
                        Attachment att = new Attachment { FileName = f.FileName, FullFileName = FolderSlashFile, PostID = id };
                        _context.Add(att);
                    }
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Edit), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            Attachment att = await _context.Attachments
                .Include(a => a.Post)
                .FirstOrDefaultAsync(x => x.ID == id);
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
