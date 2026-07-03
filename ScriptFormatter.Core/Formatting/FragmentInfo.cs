using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptFormatter.Core.Formatting
{
    public sealed class FragmentInfo
    {
        public TSqlFragment Fragment { get; set; }

        public int StartOffset { get; set; }

        public int EndOffset { get; set; }

        public int EndLine { get; set; }
    }
}
