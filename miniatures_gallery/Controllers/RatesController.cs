using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniaturesGallery.Extensions;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;
using MiniaturesGallery.Services;

namespace MiniaturesGallery.Controllers
{
    public class RatesController : Controller
    {
        private readonly IRatesService _ratesService;
        private readonly IAuthorizationService _authorizationService;

        public RatesController(IAuthorizationService authorizationService, IRatesService ratesService)
        {
            _ratesService = ratesService;
            _authorizationService = authorizationService;
        }

        // GET: Rates
        public async Task<IActionResult> Index()
        {
            return View(_ratesService.GetAll());
        }

        // GET: Rates/Details/5
        public async Task<IActionResult> Details([FromRoute] int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rate = _ratesService.Get((int)id);
            if (rate == null)
            {
                return NotFound();
            }

            return View(rate);
        }

        // GET: Rates/Create
        public IActionResult Create([FromRoute] int postId)
        {
            //for Authorization redirection purpose
            return (RedirectToAction(nameof(PostsController.Details), typeof(PostsController).ControllerName(), new { id = postId }));
        }

        // POST: Rates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm][Bind("ID,Rating,PostID,UserID")] Rate rate)
        {
            if (ModelState.IsValid)
            {
                _ratesService.Create(rate);

                return Redirect(HttpContext.Request.Headers["Referer"]);
            }
            return Redirect(HttpContext.Request.Headers["Referer"]);
        }

        // GET: Rates/Edit/5
        public async Task<IActionResult> Edit([FromRoute] int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rate = _ratesService.Get((int)id);
            if (rate == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, rate, Operations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            return View(rate);
        }

        // POST: Rates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromForm][Bind("ID,Rating")] Rate rate)
        {
            if (id != rate.ID)
            {
                return NotFound();
            }

            var rateFromDB = _ratesService.Get((int)id);
            if (rateFromDB == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, rateFromDB, Operations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _ratesService.Update(rate);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_ratesService.Exists(rate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Redirect(HttpContext.Request.Headers["Referer"]);
            }
            return Redirect(HttpContext.Request.Headers["Referer"]);
        }

        // GET: Rates/Delete/5
        public async Task<IActionResult> Delete([FromRoute] int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rate = _ratesService.Get((int)id);
            if (rate == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, rate, Operations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            return View(rate);
        }

        // POST: Rates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
        {
            var rate = _ratesService.Get(id);
            if (rate != null)
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, rate, Operations.Delete);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
                _ratesService.Delete(id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
