using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace ScriptFormatter.Core.Formatting.Custom.Nodes
{
    public sealed class ScalarExpressionNodeBuilder
    {
        /*public FormatNode Build(
            ScalarExpression expression,
            System.Func<TSqlFragment, string> getText)
        {
            if (expression == null)
            {
                return FormatNode.TextNode(string.Empty);
            }

            if (expression is SearchedCaseExpression searchedCase)
            {
                return BuildSearchedCase(
                    searchedCase,
                    getText);
            }

            return FormatNode.TextNode(
                getText(expression));
        }
        private FormatNode BuildSearchedCase(
    SearchedCaseExpression caseExpression,
    System.Func<TSqlFragment, string> getText)
        {
            var root =
                new FormatNode
                {
                    Text = "CASE",
                    ForceMultiline = true,
                    ChildrenAreAlreadyIndented = true
                };

            foreach (SearchedWhenClause whenClause
                in caseExpression.WhenClauses)
            {
                root.Children.Add(
                    FormatNode.TextNode(
                        "WHEN " + getText(whenClause.WhenExpression),
                        1));

                root.Children.Add(
                    FormatNode.TextNode(
                        "THEN " + getText(whenClause.ThenExpression),
                        2));
            }

            if (caseExpression.ElseExpression != null)
            {
                root.Children.Add(
                    FormatNode.TextNode(
                        "ELSE",
                        1));

                root.Children.Add(
                    FormatNode.TextNode(
                        getText(caseExpression.ElseExpression),
                        2));
            }

            root.Children.Add(
                FormatNode.TextNode(
                    "END",
                    0));

            return root;
        }*/
    }
}