﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MiniaturesGallery.Extensions;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;
using MiniaturesGallery.Services;
using MiniaturesGallery.ViewModels;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MiniaturesGallery.Controllers
{
    public class PostsAngularController : Controller
    {
        private readonly IPostService _postService;
        private readonly IAuthorizationService _authorizationService;
        private const int _pageSize = 10;

        public PostsAngularController(IPostService postService, IAttachmentsService attachmentsService, IAuthorizationService authorizationService, UserManager<IdentityUser> userManager)
        {
            _postService = postService;
            _authorizationService = authorizationService;
        }

        private async Task<JsonResult> GetPostsInJsonAsync()
        {
            List<Post> tmpList = await _postService.Get().ToListAsync();
            string json_data = JsonConvert.SerializeObject(
                tmpList,
                Formatting.Indented,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            JsonResult tmp = Json(json_data);
            return tmp;
        }

        [AllowAnonymous]
        public async Task<IActionResult> IndexAngular()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> Get()
        {
            return await GetPostsInJsonAsync();
        }

        [AllowAnonymous]
        public async Task<JsonResult> Info([FromRoute] int? id)
        {
            var post = _postService.Get((int)id, User.GetLoggedInUserId<string>());

            string json_data = JsonConvert.SerializeObject(
                post,
                Formatting.Indented,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            JsonResult tmp = Json(json_data);
            return tmp;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Add([FromBody][Bind("ID,Topic,Text")] Post post)
        {
            await _postService.CreateAsync(post, User.GetLoggedInUserId<string>());

            return await GetPostsInJsonAsync();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Update([FromBody][Bind("ID,Topic,Text")] Post post)
        {
            var postFromDB = _postService.Get((int)post.ID, User.GetLoggedInUserId<string>());
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
                return await GetPostsInJsonAsync();
            }
            return await GetPostsInJsonAsync();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Remove([FromBody][Bind("ID")] Post post)
        {
            var postFromDB = _postService.Get((int)post.ID, User.GetLoggedInUserId<string>());
            if (postFromDB != null)
            {
                var isAuthorized = await _authorizationService.AuthorizeAsync(User, postFromDB, Operations.Delete);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }

                await _postService.DeleteAsync(postFromDB.ID);
            }

            return await GetPostsInJsonAsync();
        }
    }
}
