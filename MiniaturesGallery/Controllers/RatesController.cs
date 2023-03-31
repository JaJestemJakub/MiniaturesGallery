using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniaturesGallery.Data;
using MiniaturesGallery.Extensions;
using MiniaturesGallery.Models;

namespace MiniaturesGallery.Controllers
{
    [Authorize]
    public class RatesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Rates
        public async Task<IActionResult> Index()
        {
              return _context.Rates != null ? 
                          View(await _context.Rates.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Rates'  is null.");
        }

        // GET: Rates/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Rates == null)
            {
                return NotFound();
            }

            var rate = await _context.Rates
                .FirstOrDefaultAsync(m => m.ID == id);
            if (rate == null)
            {
                return NotFound();
            }

            return View(rate);
        }

        // GET: Rates/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Rates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Rating,PostID,UserID")] Rate rate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rate);
                await _context.SaveChangesAsync();

                ActualizeRating(rate.PostID);
                return RedirectToAction(nameof(PostsController.Details), typeof(PostsController).ControllerName(), new { ID = rate.PostID });
            }
            return RedirectToAction(nameof(PostsController.Details), typeof(PostsController).ControllerName(), new { ID = rate.PostID });
        }

        // GET: Rates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Rates == null)
            {
                return NotFound();
            }

            var rate = await _context.Rates.FindAsync(id);
            if (rate == null)
            {
                return NotFound();
            }
            return View(rate);
        }

        // POST: Rates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Rating,PostID,UserID")] Rate rate)
        {
            if (id != rate.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rate);
                    await _context.SaveChangesAsync();

                    ActualizeRating(rate.PostID);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RateExists(rate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(PostsController.Details), typeof(PostsController).ControllerName(), new { ID = rate.PostID });
            }
            return RedirectToAction(nameof(PostsController.Details), typeof(PostsController).ControllerName(), new { ID = rate.PostID });
        }

        // GET: Rates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Rates == null)
            {
                return NotFound();
            }

            var rate = await _context.Rates
                .FirstOrDefaultAsync(m => m.ID == id);
            if (rate == null)
            {
                return NotFound();
            }

            return View(rate);
        }

        // POST: Rates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Rates == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Rates'  is null.");
            }
            var rate = await _context.Rates.FindAsync(id);
            if (rate != null)
            {
                _context.Rates.Remove(rate);
            }
            
            await _context.SaveChangesAsync();

            ActualizeRating(rate.PostID);
            return RedirectToAction(nameof(Index));
        }

        public bool ActualizeRating(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return false;
            }

            Post post = _context.Posts
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

        private bool RateExists(int id)
        {
          return (_context.Rates?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
