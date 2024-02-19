using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniaturesGallery.Extensions;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;
using MiniaturesGallery.Services;

namespace MiniaturesGallery.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ICommentsService _commentsService;
        private readonly IAuthorizationService _authorizationService;

        public CommentsController(IAuthorizationService authorizationService, ICommentsService commentsService)
        {
            _commentsService = commentsService;
            _authorizationService = authorizationService;
        }

        // GET: Comments
        public async Task<IActionResult> Index()
        {
            return View(await _commentsService.GetAsync());
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details([FromRoute] int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _commentsService.GetAsync((int)id);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromForm][Bind("ID,Body,PostID,CommentID,UserID")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                await _commentsService.CreateAsync(comment);
                return RedirectToAction(nameof(PostsController.Details), typeof(PostsController).ControllerName(), new { ID = comment.PostID });
            }
            return RedirectToAction(nameof(PostsController.Details), typeof(PostsController).ControllerName(), new { ID = comment.PostID });
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit([FromRoute] int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _commentsService.GetAsync((int)id);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromForm][Bind("ID,Body")] Comment comment)
        {
            if (id != comment.ID)
            {
                return NotFound();
            }

            Comment commentFromDB = await _commentsService.GetAsync(comment.ID);
            if (commentFromDB == null)
            {
                return NotFound();
            }
            var isAuthorized = await _authorizationService.AuthorizeAsync(User, comment, Operations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _commentsService.UpdateAsync(comment);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_commentsService.Exists(comment.ID))
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
        public async Task<IActionResult> Delete([FromRoute] int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _commentsService.GetAsync((int)id);
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
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
        {
            var comment = await _commentsService.GetAsync(id);
            if (comment != null)
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, comment, Operations.Delete);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }

                await _commentsService.DeleteAsync(id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
