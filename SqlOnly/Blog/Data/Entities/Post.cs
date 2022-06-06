using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public IReadOnlyCollection<Comment> Comments { get; private set; } = Array.Empty<Comment>();
}