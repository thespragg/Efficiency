using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Models
{
    public class CompilerResponse
    {
        public string Status { get; set; }
        public List<string> Errors { get; set; }
        public List<string> ConsoleLogs { get; set; }
    }
}
