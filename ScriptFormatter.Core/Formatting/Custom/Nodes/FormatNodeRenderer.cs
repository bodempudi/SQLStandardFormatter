using System.Collections.Generic;

namespace ScriptFormatter.Core.Formatting.Custom.Nodes
{
    public sealed class FormatNodeRenderer
    {
        public IList<FormattedLine> Render(
            FormatNode node)
        {
            var lines =
                new List<FormattedLine>();

            RenderNode(
                node,
                0,
                lines);

            return lines;
        }

        private void RenderNode(
            FormatNode node,
            int indent,
            IList<FormattedLine> lines)
        {
            if (node == null)
            {
                return;
            }

            if (node.Kind == "Text")
            {
                lines.Add(
                    new FormattedLine
                    {
                        RelativeIndent = indent,
                        Text = node.Text
                    });

                return;
            }

            if (node.Kind == "Case")
            {
                lines.Add(
                    new FormattedLine
                    {
                        RelativeIndent = indent,
                        Text = "CASE"
                    });

                foreach (FormatNode child in node.Children)
                {
                    RenderNode(
                        child,
                        indent + 1,
                        lines);
                }

                lines.Add(
                    new FormattedLine
                    {
                        RelativeIndent = indent,
                        Text = "END"
                    });

                return;
            }

            if (node.Kind == "When")
            {
                lines.Add(
                    new FormattedLine
                    {
                        RelativeIndent = indent,
                        Text = "WHEN " + node.Text
                    });

                return;
            }

            if (node.Kind == "Then")
            {
                lines.Add(
                    new FormattedLine
                    {
                        RelativeIndent = indent + 1,
                        Text = "THEN " + node.Text
                    });

                return;
            }

            if (node.Kind == "Else")
            {
                lines.Add(
                    new FormattedLine
                    {
                        RelativeIndent = indent,
                        Text = "ELSE"
                    });

                lines.Add(
                    new FormattedLine
                    {
                        RelativeIndent = indent + 1,
                        Text = node.Text
                    });

                return;
            }

            lines.Add(
                new FormattedLine
                {
                    RelativeIndent = indent,
                    Text = node.Text
                });
        }
    }
}