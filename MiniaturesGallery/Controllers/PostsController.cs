using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using MiniaturesGallery.Data;
using MiniaturesGallery.Extensions;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;
using MiniaturesGallery.Services;
using MiniaturesGallery.ViewModels;

namespace MiniaturesGallery.Controllers
{
    public class PostsController : Controller
    {       
        private readonly IPostService _postService;
        private readonly IAttachmentsService _attachmentsService;
        private readonly IAuthorizationService _authorizationService;

        public PostsController(IPostService postService, IAttachmentsService attachmentsService, IAuthorizationService authorizationService)
        {
            _postService = postService;
            _attachmentsService = attachmentsService;
            _authorizationService = authorizationService;
        }

        // GET: Posts
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _postService.GetAsync());
        }

        // GET: Posts
        [AllowAnonymous]
        public async Task<IActionResult> ScrollIndex([FromQuery] string searchString, [FromQuery] string orderByFilter, [FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo, [FromQuery] int? pageNumber)
        {
            ViewBag.SearchString = searchString;
            ViewBag.OrderByFilter = orderByFilter;
            if (dateFrom == DateTime.MinValue)
                dateFrom = DateTime.Today.AddMonths(-1);
            ViewBag.DateFrom = dateFrom.ToString("yyyy-MM-dd");
            if (dateTo == DateTime.MinValue)
                dateTo = DateTime.Today;
            ViewBag.DateTo = dateTo.ToString("yyyy-MM-dd");

            return View(await _postService.GetAsyncSortedBy(searchString, orderByFilter, dateFrom, dateTo, pageNumber, User.GetLoggedInUserId<string>()));
        }

        // GET: Posts/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details([FromRoute] int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _postService.GetAsync((int) id, User.GetLoggedInUserId<string>());
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm][Bind("ID,Topic,Text")] Post post)
        {
            if (ModelState.IsValid)
            {
                int id = await _postService.CreateAsync(post, User.GetLoggedInUserId<string>());
                return RedirectToAction(nameof(Edit), new { id = id });
            }
            return View(post);
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit([FromRoute] int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _postService.GetAsync((int)id, User.GetLoggedInUserId<string>());
            if (post == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, post, Operations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            PostEditViewModel viewModel = new PostEditViewModel { Post = post };
            return View(viewModel);
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromForm][Bind("ID,Topic,Text")] Post post)
        {
            if (id != post.ID)
            {
                return NotFound();
            }

            var postFromDB = await _postService.GetAsync((int)id, User.GetLoggedInUserId<string>());
            if (postFromDB == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, postFromDB, Operations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _postService.UpdateAsync(post);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_postService.Exists(post.ID))
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
        public async Task<IActionResult> Delete([FromRoute] int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _postService.GetAsync((int)id, User.GetLoggedInUserId<string>());
            if (post == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, post, Operations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
        {
            var post = await _postService.GetAsync((int)id, User.GetLoggedInUserId<string>());
            if (post != null)
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, post, Operations.Delete);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }

                await _postService.DeleteAsync(id);
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAttachment([FromRoute] int id, [FromForm][Bind("ID", "Files")] PostEditViewModel postViewModel)
        {
            var post = await _postService.GetAsync((int)id, User.GetLoggedInUserId<string>());
            if (post == null)
            {
                return NotFound();
            }
            var isAuthorized = await _authorizationService.AuthorizeAsync(User, post, Operations.Create);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                await _attachmentsService.CreateAsync(postViewModel.Files, id, User.GetLoggedInUserId<string>());
            }
            return RedirectToAction(nameof(Edit), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachment([FromRoute] int id)
        {
            Attachment att = await _attachmentsService.GetAsync(id);

            if (att == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, att, Operations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            await _attachmentsService.DeleteAsync(id);
            return RedirectToAction(nameof(Edit), new { id = att.PostID });
        }
    }
}
