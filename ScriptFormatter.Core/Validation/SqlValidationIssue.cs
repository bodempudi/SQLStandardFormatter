using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ScriptFormatter.Core.Validation
{
    public class SqlValidationIssue
    {
        public int Line {  get; set; }
        public string Message {  get; set; }
        public int Column {  get; set; }

        public string Source { get; set; } = "Validator";
    }
}
