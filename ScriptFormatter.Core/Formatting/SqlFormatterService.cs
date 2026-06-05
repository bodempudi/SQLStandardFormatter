using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptFormatter.Core.Formatting
{
    public class SqlFormatterService
    {
        public string Format(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                return sql;
            }

            var parser =
                new TSql160Parser(false);

            TSqlFragment fragment;

            using (var reader =
                new StringReader(sql))
            {
                IList<ParseError> errors;

                fragment =
                    parser.Parse(
                        reader,
                        out errors);

                if (errors != null &&
                    errors.Count > 0)
                {
                    return sql;
                }
            }

            var generator =
                new Sql160ScriptGenerator(
                    CreateOptions());

            string formattedSql;

            generator.GenerateScript(
                fragment,
                out formattedSql);

            return formattedSql;
        }

        private static SqlScriptGeneratorOptions
            CreateOptions()
        {
            return new SqlScriptGeneratorOptions
            {
                KeywordCasing =
                    KeywordCasing.Uppercase,

                IncludeSemicolons = false,

                IndentationSize = 4,

                NewLineBeforeFromClause = true,

                NewLineBeforeWhereClause = true,

                NewLineBeforeGroupByClause = true,

                NewLineBeforeOrderByClause = true,

                AlignClauseBodies = false
            };
        }
    }
}
