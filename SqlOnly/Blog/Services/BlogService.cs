using Blog.Data;
using Blog.Data.Entities;

namespace Blog.Services;

public class BlogService
{
    private readonly BlogDbContext _dbContext;

    public BlogService(BlogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Post> CreatePost(int userId, string title, string text)
    {
        var post = new Post(userId, title, text);

        return await CreatePost(post);
    }

    public async Task<Post> CreatePost(Post post)
    {
        _dbContext.Set<Post>().Add(post);

        await _dbContext.SaveChangesAsync();

        return post;
    }

    public async Task<Comment> CreateComment(int postId, int userId, string text)
    {
        var post = new Comment(postId, userId, text);

        _dbContext.Set<Comment>().Add(post);

        await _dbContext.SaveChangesAsync();

        return post;
    }
}