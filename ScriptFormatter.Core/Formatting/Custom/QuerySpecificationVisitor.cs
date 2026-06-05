using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptFormatter.Core.Formatting.Custom
{
    public class QuerySpecificationVisitor : TSqlFragmentVisitor
    {
        public QuerySpecification QuerySpecification
        {
            get;
            private set;
        }

        public override void ExplicitVisit(
            QuerySpecification node)
        {
            // Capture first SELECT query block.
            if (QuerySpecification == null)
            {
                QuerySpecification = node;
            }
        }
    }
}
