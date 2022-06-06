namespace Blog.Data.Entities;

public class User
{
    protected User()
    {

    }

    public User(string name)
    {
        Name = name;
    }

    public int UserId { get; private set; }
    public string Name { get; private set; }

    public IList<Comment> Comments { get; private set; } = new List<Comment>();
    public IList<Post> Posts { get; private set; } = new List<Post>();
}