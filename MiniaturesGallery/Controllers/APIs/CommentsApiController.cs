﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniaturesGallery.HelpClasses;
using MiniaturesGallery.Models;
using MiniaturesGallery.Services;

namespace MiniaturesGallery.Controllers.APIs
{
    [Route("CommentsApiController")]
    [ApiController]
    public class CommentsApiController : ControllerBase
    {
        private readonly ICommentsService _commentsService;
        private readonly IAuthorizationService _authorizationService;

        public CommentsApiController(IAuthorizationService authorizationService, ICommentsService commentsService)
        {
            _authorizationService = authorizationService;
            _commentsService = commentsService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetAsync([FromRoute] int id)
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
            return Ok(comment);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromForm][Bind("ID,Body,PostID,CommentID,UserID")] Comment comment)
        {
            int id = await _commentsService.CreateAsync(comment);

            return Created($"PostsApiController/{id}", null);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete([FromRoute][Bind("ID")] int id)
        {
            var comment = await _commentsService.GetAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            var isAuthorized = await _authorizationService.AuthorizeAsync(User, comment, Operations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            await _commentsService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPut]
        public async Task<ActionResult> Put([FromForm][Bind("ID,Body")] Comment comment)
        {
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

            await _commentsService.UpdateAsync(comment);
            return Ok();
        }
    }
}