using System.Collections.Generic;

namespace cslib
{
    public interface IProjectDiscovery
    {
        IEnumerable<DiscoveredProject> DiscoverProjects(string file);
    }
}

