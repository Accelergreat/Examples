using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Data.Entities;

public class Comment
{
    protected Comment()
    {

    }

    public Comment(int postId, int userId, string text)
    {
        PostId = postId;
        UserId = userId;
        Text = text;
    }

    public int CommentId { get; private set; }
    public int UserId { get; private set; }
    public int PostId { get; private set; }
    public string Text { get; private set; }

    public User User { get; private set; } = null!;
    public Post Post { get; private set; } = null!;
}