using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Tgs.Logging
{
    public class TgsApiLoggerOptions
    {
        public bool IsManual { get; set; } = false;
        public IEnumerable<PathString> IgnorePaths { get; set; } = [];

        public IEnumerable<PathString> AllowPaths { get; set; } = [];
    }
}
