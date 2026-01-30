using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PostsService.Application.Commands;
using PostsService.Application.Interfaces;
using PostsService.DTOs.Requests;
using PostsService.DTOs.Response;

namespace PostsService.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/posts")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        // GET api/posts
        [HttpGet]
        public async Task<ActionResult<IReadOnlyCollection<PostResponse>>> GetAll()
        {
            var posts = await _postService.GetAllAsync();
            return Ok(posts);
        }

        // GET api/posts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PostResponse>> GetById(string id)
        {
            var post = await _postService.GetByIdAsync(id);
            return Ok(post);
        }

        // POST api/posts
        [HttpPost]
        public async Task<IActionResult> Create(CreatePostRequest request)
        {
            var postId = await _postService.CreateAsync(
                new CreatePostCommand(
                    request.Caption,
                    request.Content,
                    request.UserId
                )
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = postId },
                null
            );
        }

        // PUT api/posts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Replace(string id, ReplacePostRequest request)
        {
            await _postService.ReplaceAsync(
                new ReplacePostCommand(
                    id,
                    request.Caption,
                    request.Content
                )
            );

            return NoContent();
        }

        // PATCH api/posts/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePartial(string id, UpdatePostRequest request)
        {
            await _postService.UpdateAsync(
                new UpdatePostCommand(
                    id,
                    request.Caption,
                    request.Content
                )
            );

            return NoContent();
        }

        // DELETE api/posts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _postService.DeleteAsync(id);
            return NoContent();
        }
    }
}
