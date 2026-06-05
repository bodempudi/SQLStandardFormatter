using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptFormatter.Core.Errors
{
    public sealed class SqlErrorInfo
    {
        public int Line { get; set; }

        public int Column { get; set; }

        public string Message { get; set; }

        public string Source { get; set; }
    }
}
