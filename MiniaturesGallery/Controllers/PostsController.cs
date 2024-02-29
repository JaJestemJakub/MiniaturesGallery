using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly UserManager<IdentityUser> _userManager;
        private const int _pageSize = 10;

        public PostsController(IPostService postService, IAttachmentsService attachmentsService, IAuthorizationService authorizationService, UserManager<IdentityUser> userManager)
        {
            _postService = postService;
            _attachmentsService = attachmentsService;
            _authorizationService = authorizationService;
            _userManager = userManager;
        }

        // GET: Posts
        [AllowAnonymous]
        public async Task<IActionResult> Index([FromQuery] string searchString, [FromQuery] string orderByFilter, [FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo, [FromQuery] int? pageNumber)
        {
            ViewBag.SearchString = searchString;
            ViewBag.OrderByFilter = orderByFilter;
            if (dateFrom == DateTime.MinValue)
                dateFrom = DateTime.Today.AddMonths(-1);
            ViewBag.DateFrom = dateFrom.ToString("yyyy-MM-dd");
            if (dateTo == DateTime.MinValue)
                dateTo = DateTime.Today;
            ViewBag.DateTo = dateTo.ToString("yyyy-MM-dd");

            var tmpList = _postService.Get(searchString, orderByFilter, dateFrom, dateTo, User.GetLoggedInUserId<string>());
            var PgList = await PaginatedList<PostAbs>.CreateAsync(tmpList, pageNumber ?? 1, _pageSize);
            PgList.Action = nameof(PostsController.Index);

            return View(PgList);
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

            var tmpList = _postService.Get(searchString, orderByFilter, dateFrom, dateTo, User.GetLoggedInUserId<string>());
            var PgList = await PaginatedList<PostAbs>.CreateAsync(tmpList, pageNumber ?? 1, _pageSize);
            PgList.Action = nameof(PostsController.Index);

            return View(PgList);
        }


        // GET: Posts
        [AllowAnonymous]
        public async Task<IActionResult> ScrollIndexOfUser([FromQuery] string filtrUserID, [FromQuery] int? pageNumber)
        {
            var tmpList = _postService.GetOfUser(filtrUserID, User.GetLoggedInUserId<string>());
            var PgList = await PaginatedList<PostAbs>.CreateAsync(tmpList, pageNumber ?? 1, _pageSize);
            PgList.Action = nameof(PostsController.ScrollIndexOfUser);
            PgList.UserID = filtrUserID;
            ViewBag.UserName = _userManager.FindByIdAsync(filtrUserID).Result.UserName;

            return View(PgList);
        }

        // GET: Posts/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details([FromRoute] int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = _postService.Get((int)id, User.GetLoggedInUserId<string>());
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // GET: Posts/CreatePost
        public IActionResult CreatePost()
        {
            return View();
        }

        // POST: Posts/CreatePost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost([FromForm][Bind("ID,Topic,Text")] Post post)
        {
            if (ModelState.IsValid)
            {
                int id = _postService.Create(post, User.GetLoggedInUserId<string>());
                return RedirectToAction(nameof(Edit), new { id = id });
            }
            return View(post);
        }

        // GET: Posts/CreateAnnouncement
        public IActionResult CreateAnnouncement()
        {
            return View();
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnnouncement([FromForm][Bind("ID,Topic,Text,PrivateNote")] Announcement post)
        {
            if (ModelState.IsValid)
            {
                int id = _postService.Create(post, User.GetLoggedInUserId<string>());
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

            var post = _postService.Get((int)id, User.GetLoggedInUserId<string>());
            if (post == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, post, Operations.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            PostEditViewModel viewModel = new PostEditViewModel { PostAbs = post };
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

            var postFromDB = _postService.Get((int)id, User.GetLoggedInUserId<string>());
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
                    _postService.Update(post);
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

            var post = _postService.Get((int)id, User.GetLoggedInUserId<string>());
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
            var post = _postService.Get((int)id, User.GetLoggedInUserId<string>());
            if (post != null)
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, post, Operations.Delete);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }

                _postService.Delete(id);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAttachment([FromRoute] int id, [FromForm][Bind("ID", "Files")] PostEditViewModel postViewModel)
        {
            var post = _postService.Get((int)id, User.GetLoggedInUserId<string>());
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
                _attachmentsService.Create(postViewModel.Files, id, User.GetLoggedInUserId<string>());
            }
            return RedirectToAction(nameof(Edit), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachment([FromRoute] int id)
        {
            Attachment att = _attachmentsService.Get(id);

            if (att == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, att, Operations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            _attachmentsService.Delete(id);
            return RedirectToAction(nameof(Edit), new { id = att.PostID });
        }
    }
}
