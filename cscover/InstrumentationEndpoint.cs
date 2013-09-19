using System;

namespace cscover
{
    public static class InstrumentationEndpoint
    {
        public static void Invoke(string info)
        {
            Console.WriteLine(string.IsNullOrEmpty(info));
            //Console.WriteLine(info);
            //methodFullName = methodFullName ?? "";
            //document = document ?? "";
            //startLine = startLine ?? "";
            //endLine = endLine ?? "";
            //Console.WriteLine("Entered " + methodFullName + " in " + document + ":" + startLine + "-" + endLine);
        }
    }
}

