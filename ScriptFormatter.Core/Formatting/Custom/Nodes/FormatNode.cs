using System.Collections.Generic;
using System.Linq;

namespace ScriptFormatter.Core.Formatting.Custom.Nodes
{
    public sealed class FormatNode
    {
        public string Kind { get; set; }

        public string Text { get; set; }

        public List<FormatNode> Children { get; } =
            new List<FormatNode>();

        public bool IsMultiline
        {
            get
            {
                return
                    Kind == "Case" ||
                    Children.Any(x => x.IsMultiline);
            }
        }

        public static FormatNode TextNode(
            string text)
        {
            return new FormatNode
            {
                Kind = "Text",
                Text = text
            };
        }
    }
}