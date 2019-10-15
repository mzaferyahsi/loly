namespace Loly.App.Models
{
    public interface ILink
    {
        string Href { get; }
        string Rel { get; }
        string Method { get; }
        
    }
}