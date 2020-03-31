using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SandboxHelpers
{
    public class OutputHandler
    {
        private List<string> WrittenLines { get; set; }
        public OutputHandler()
        {
            WrittenLines = new List<string>();
        }
        public void WriteLine(string line)
        {
            WrittenLines.Add(line);
        }

        public List<string> GetLines()
        {
            return WrittenLines;
        }
    }
}
