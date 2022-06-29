using Accelergreat.Idp.Database.Contexts;
using Accelergreat.Idp.Database.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Accelergreat.Idp.ExampleApi.Controllers;

[ApiController]
[Route("blog-entries")]
public class BlogEntriesController : ControllerBase
{
    private readonly BloggingContext _context;

    public BlogEntriesController(BloggingContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IEnumerable<BlogEntry>> GetBlogEntries()
    {
        return await _context.BlogEntries.ToListAsync();
    }

    [HttpPost]
    public async Task<int> CreateBlogEntry(BlogEntry blogEntry)
    {
        await _context.BlogEntries.AddAsync(blogEntry);
        await _context.SaveChangesAsync();
        return blogEntry.Id;
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteBlogEntry(BlogEntry blogEntry)
    {
        _context.BlogEntries.Remove(blogEntry);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateBlogEntry(BlogEntry blogEntry)
    {
        var persistedBlogEntry = await _context.BlogEntries.SingleOrDefaultAsync(b => b.Id == blogEntry.Id);

        if (persistedBlogEntry == null)
        {
            return NotFound();
        }

        persistedBlogEntry.Title = blogEntry.Title;
        persistedBlogEntry.Content = blogEntry.Content;
        return NoContent();
    }
}