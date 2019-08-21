using System.Collections.Generic;

namespace Loly.Models.Api
{
    public class Discovery
    {
        public string Path { get; set; }
        public bool Watch { get; set; }
        public IList<string> Exclusions { get; set; }
    }
}