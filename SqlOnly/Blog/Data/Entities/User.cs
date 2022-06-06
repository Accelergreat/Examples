using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public IReadOnlyCollection<Comment> Comments { get; private set; } = Array.Empty<Comment>();
    public IReadOnlyCollection<Post> Posts { get; private set; } = Array.Empty<Post>();
}