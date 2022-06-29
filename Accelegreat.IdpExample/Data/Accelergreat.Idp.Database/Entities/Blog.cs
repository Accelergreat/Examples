namespace Accelergreat.Idp.Database.Entities
{
    public class Blog
    {
        public int Id { get; set; }

        public IEnumerable<BlogEntry> BlogEntries { get; set; } = new List<BlogEntry>();
    }
}
