namespace Accelergreat.Idp.Database.Entities;

public class BlogEntry
{
    public Blog ParentBlog { get; set; } = null!;

    public int ParentBlogId { get; set; }

    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime Posted { get; set; }
}