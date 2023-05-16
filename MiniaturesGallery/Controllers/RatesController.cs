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
            return View(await _ratesService.GetAsync());
        }

        // GET: Rates/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rate = await _ratesService.GetAsync((int)id);
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
                await _ratesService.CreateAsync(rate);

                return Redirect(HttpContext.Request.Headers["Referer"]);
            }
            return Redirect(HttpContext.Request.Headers["Referer"]);
        }

        // GET: Rates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rate = await _ratesService.GetAsync((int)id);
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Rating")] Rate rate)
        {
            if (id != rate.ID)
            {
                return NotFound();
            }

            var rateFromDB = await _ratesService.GetAsync((int)id);
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
                    await _ratesService.UpdateAsync(rate);
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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rate = await _ratesService.GetAsync((int)id);
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rate = await _ratesService.GetAsync(id);
            if (rate != null)
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, rate, Operations.Delete);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
                await _ratesService.DeleteAsync(id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
