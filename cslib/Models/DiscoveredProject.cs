using System.Collections.Generic;

namespace cslib
{
    public class DiscoveredProject
    {
        public string Path { get; set; }
        public IEnumerable<DiscoveredSourceFile> DiscoveredFiles { get; set; }
    }
}

