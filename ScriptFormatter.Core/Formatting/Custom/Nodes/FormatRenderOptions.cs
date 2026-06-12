using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptFormatter.Core.Formatting.Custom.Nodes
{
    public sealed class FormatRenderOptions
    {
        public bool UseLeadingComma { get; set; } =
            true;

        public bool ForceMultiline { get; set; }

        public int BaseIndentLevel { get; set; }
    }
}