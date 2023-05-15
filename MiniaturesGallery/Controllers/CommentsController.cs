using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MiniaturesGallery.Data;
using MiniaturesGallery.Extensions;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;

namespace MiniaturesGallery.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;

        public CommentsController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        // GET: Comments
        public async Task<IActionResult> Index()
        {
              return _context.Comments != null ? 
                          View(await _context.Comments.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Comments'  is null.");
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Comments == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(m => m.ID == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Body,PostID,CommentID,UserID")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.CrateDate = DateTime.Now;
                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(PostsController.Details), typeof(PostsController).ControllerName(), new { ID = comment.PostID });
            }
            return RedirectToAction(nameof(PostsController.Details), typeof(PostsController).ControllerName(), new { ID = comment.PostID});
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Comments == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, comment, Operations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Body")] Comment comment)
        {
            if (id != comment.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Comment commentFromDB = await _context.Comments.FirstAsync(x => x.ID == comment.ID);

                    var isAuthorized = await _authorizationService.AuthorizeAsync(User, comment, Operations.Update);
                    if (!isAuthorized.Succeeded)
                    {
                        return Forbid();
                    }

                    commentFromDB.Body = comment.Body;
                    _context.Update(commentFromDB);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.ID))
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
            return View(comment);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Comments == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(m => m.ID == id);
            if (comment == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, comment, Operations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Comments == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Comments'  is null.");
            }
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, comment, Operations.Delete);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
            

            return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(int id)
        {
          return (_context.Comments?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
