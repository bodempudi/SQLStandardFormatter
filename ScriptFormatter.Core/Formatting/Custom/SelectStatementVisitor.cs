using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace ScriptFormatter.Core.Formatting.Custom
{
    public sealed class SelectStatementVisitor
        : TSqlFragmentVisitor
    {
        public SelectStatement SelectStatement
        {
            get;
            private set;
        }

        public override void ExplicitVisit(
            SelectStatement node)
        {
            if (SelectStatement == null)
            {
                SelectStatement = node;
            }
        }
    }
}