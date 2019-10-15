namespace Loly.App.Models
{
    public class Link : ILink
    {
        public string Href { get; private set; }
        public string Rel { get; private set; }
        public string Method { get; private set; }
        
        public Link(string href, string rel, string method) {
            this.Href = href;
            this.Rel = rel;
            this.Method = method;
        }
    }
}