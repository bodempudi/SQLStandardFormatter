using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace ScriptFormatter.Core.Formatting.Custom.Nodes
{
    public sealed class FormatNodeBuilder
    {
        public FormatNode BuildScalarExpression(
            ScalarExpression expression,
            Func<TSqlFragment, string> getText)
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
            Func<TSqlFragment, string> getText)
        {
            var node =
                new FormatNode
                {
                    Kind = "Case",
                    Text = "CASE"
                };

            foreach (SearchedWhenClause whenClause
                in caseExpression.WhenClauses)
            {
                var whenNode =
                    new FormatNode
                    {
                        Kind = "When",
                        Text =
                            getText(
                                whenClause.WhenExpression)
                    };

                var thenNode =
                    new FormatNode
                    {
                        Kind = "Then",
                        Text =
                            getText(
                                whenClause.ThenExpression)
                    };

                node.Children.Add(whenNode);
                node.Children.Add(thenNode);
            }

            if (caseExpression.ElseExpression != null)
            {
                node.Children.Add(
                    new FormatNode
                    {
                        Kind = "Else",
                        Text =
                            getText(
                                caseExpression.ElseExpression)
                    });
            }

            return node;
        }
    }
}