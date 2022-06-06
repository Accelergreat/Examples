namespace Blog.Data.Entities;

public class Post
{
    protected Post()
    {

    }

    public Post(int userId, string title, string text)
    {
        Title = title;
        UserId = userId;
        Text = text;
    }

    public int PostId { get; private set; }
    public int UserId { get; private set; }
    public string Title { get; private set; }
    public string Text { get; private set; }

    public User User { get; private set; } = null!;
    public IList<Comment> Comments { get; private set; } = new List<Comment>();
}