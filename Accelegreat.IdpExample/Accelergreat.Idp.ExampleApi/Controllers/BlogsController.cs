using Accelergreat.Idp.Database.Contexts;
using Accelergreat.Idp.Database.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Accelergreat.Idp.ExampleApi.Controllers;

[ApiController]
[Route("blogs")]
public class BlogsController : ControllerBase
{
    private readonly BloggingContext _context;

    public BlogsController(BloggingContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IEnumerable<Blog>> GetBlogs()
    {
        return await _context.Blogs.ToListAsync();
    }

    [HttpPost]
    public async Task<int> CreateBlog(Blog blog)
    {
        await _context.Blogs.AddAsync(blog);
        await _context.SaveChangesAsync();
        return blog.Id;
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteBlog(Blog blog)
    {
        _context.Blogs.Remove(blog);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}