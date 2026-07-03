using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptFormatter.Core.Formatting.Custom
{
    public class FormatterMessage
    {
        public MessageType Type { get; set; }

        public int? Line { get; set; }

        public int? Column { get; set; }

        public string Message { get; set; }
    }
}
