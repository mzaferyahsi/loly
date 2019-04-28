namespace Loly.Agent.Models
{
    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public Severity Severity { get; set; }    
    }
}