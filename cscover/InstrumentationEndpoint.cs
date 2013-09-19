using System.IO;

namespace cscover
{
    public static class InstrumentationEndpoint
    {
        private static object _lock = new object();
    
        public static void Invoke(string output, string methodFullName, string document, string startLine, string endLine)
        {
            lock (_lock)
            {
                using (var writer = new StreamWriter(output, true))
                {
                    writer.WriteLine(startLine + " " + endLine + " " + document);
                }
            }
        }
    }
}

