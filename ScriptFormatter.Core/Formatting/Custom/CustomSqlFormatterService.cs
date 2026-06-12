using Microsoft.SqlServer.TransactSql.ScriptDom;
using ScriptFormatter.Core.Formatting.Custom.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScriptFormatter.Core.Formatting.Custom
{
    public sealed class FormattedSqlNode
    {
        public List<FormattedLine> Lines { get; } =
            new List<FormattedLine>();

        public bool IsMultiline
        {
            get { return Lines.Count > 1; }
        }
    }
    public sealed class FormattedLine
    {
        public int RelativeIndent { get; set; }

        public string Text { get; set; }

        public string LeadingComment { get; set; }

        public string InlineComment { get; set; }
    }
    public sealed class SqlCommentInfo
    {
        public int Offset { get; set; }

        public int Line { get; set; }

        public int Column { get; set; }

        public string Text { get; set; }

        public CommentPlacement Placement { get; set; }
    }
    public enum CommentPlacement
    {
        Leading,
        Inline,
        Trailing
    }
    public sealed class CustomSqlFormatterService
    {
        private bool _hasLeadingSemicolonBeforeCte;
        private readonly BooleanExpressionFormatter _booleanExpressionFormatter =
    new BooleanExpressionFormatter();
        private Dictionary<int, string> _inlineComments =
    new Dictionary<int, string>();
        private string _sourceSql;

        private string GetInlineComment(
    TSqlFragment fragment)
        {
            if (fragment == null)
            {
                return string.Empty;
            }

            if (_inlineComments.ContainsKey(fragment.StartOffset))
            {
                return " " + _inlineComments[fragment.StartOffset];
            }

            return string.Empty;
        }
        private IList<FormattedLine> FormatCallLikeLines(
    string name,
    IList<FormattedSqlNode> argumentNodes,
    TSqlFragment originalFragment)
        {
            bool hasMultilineArgument =
                argumentNodes.Any(x => x.IsMultiline);

            if (!hasMultilineArgument)
            {
                return new List<FormattedLine>
        {
            new FormattedLine
            {
                RelativeIndent = 0,
                Text = GetFragmentText(originalFragment)
            }
        };
            }

            var lines =
                new List<FormattedLine>();

            lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text = name
                });

            lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text = "("
                });

            for (int i = 0; i < argumentNodes.Count; i++)
            {
                FormattedSqlNode argumentNode =
                    argumentNodes[i];

                for (int j = 0; j < argumentNode.Lines.Count; j++)
                {
                    string prefix =
                        i > 0 && j == 0
                            ? ","
                            : string.Empty;

                    lines.Add(
                        new FormattedLine
                        {
                            RelativeIndent =
                                argumentNode.Lines[j].RelativeIndent + 1,

                            Text =
                                prefix +
                                argumentNode.Lines[j].Text
                        });
                }
            }

            lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text = ")"
                });

            return lines;
        }

        private FormattedSqlNode CreateSingleLineNode(
    string text)
        {
            var node =
                new FormattedSqlNode();

            node.Lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text = text
                });

            return node;
        }

        private FormattedSqlNode FormatScalarExpressionNode(
    ScalarExpression expression)
        {
            var node =
                new FormattedSqlNode();

            if (expression == null)
            {
                return node;
            }

            if (expression is SearchedCaseExpression searched)
            {
                node.Lines.AddRange(
                    FormatSearchedCaseExpressionLines(
                        searched));

                return node;
            }

            if (expression is SimpleCaseExpression simple)
            {
                node.Lines.AddRange(
                    FormatSimpleCaseExpressionLines(
                        simple));

                return node;
            }

            if (expression is FunctionCall functionCall)
            {
                node.Lines.AddRange(
                    FormatFunctionCallLines(
                        functionCall));

                return node;
            }

            if (expression is ConvertCall convertCall)
            {
                node.Lines.AddRange(
                    FormatConvertCallLines(
                        convertCall));

                return node;
            }

            if (expression is CoalesceExpression coalesceExpression)
            {
                node.Lines.AddRange(
                    FormatCoalesceExpressionLines(
                        coalesceExpression));

                return node;
            }

            node.Lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text = GetFragmentText(
                        expression)
                });

            return node;
        }

        private IList<FormattedLine> FormatCoalesceExpressionLines(
    CoalesceExpression coalesceExpression)
        {
            var expressionNodes =
                coalesceExpression.Expressions
                    .Select(FormatScalarExpressionNode)
                    .ToList();

            bool hasMultilineExpression =
                expressionNodes.Any(x => x.IsMultiline);

            if (!hasMultilineExpression)
            {
                return new List<FormattedLine>
        {
            new FormattedLine
            {
                RelativeIndent = 0,
                Text = GetFragmentText(coalesceExpression)
            }
        };
            }

            var lines =
                new List<FormattedLine>();

            lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text = "COALESCE"
                });

            lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text = "("
                });

            for (int i = 0; i < expressionNodes.Count; i++)
            {
                FormattedSqlNode expressionNode =
                    expressionNodes[i];

                for (int j = 0; j < expressionNode.Lines.Count; j++)
                {
                    string prefix =
                        i > 0 && j == 0
                            ? ","
                            : string.Empty;

                    lines.Add(
                        new FormattedLine
                        {
                            RelativeIndent =
                                expressionNode.Lines[j].RelativeIndent + 1,

                            Text =
                                prefix +
                                expressionNode.Lines[j].Text
                        });
                }
            }

            lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text = ")"
                });

            return lines;
        }
        private IList<FormattedLine> FormatConvertCallLines(
    ConvertCall convertCall)
        {
            FormattedSqlNode parameterNode =
                FormatScalarExpressionNode(
                    convertCall.Parameter);

            bool isMultiline =
                parameterNode.IsMultiline;

            if (!isMultiline)
            {
                return new List<FormattedLine>
        {
            new FormattedLine
            {
                RelativeIndent = 0,
                Text = GetFragmentText(convertCall)
            }
        };
            }

            var lines =
                new List<FormattedLine>();

            lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text = "CONVERT"
                });

            lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text = "("
                });

            lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 1,
                    Text = FormatDataType(
                        convertCall.DataType)
                });

            lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 1,
                    Text = ","
                });

            foreach (FormattedLine line in parameterNode.Lines)
            {
                lines.Add(
                    new FormattedLine
                    {
                        RelativeIndent =
                            line.RelativeIndent + 1,

                        Text =
                            line.Text
                    });
            }

            lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text = ")"
                });

            return lines;
        }
        private void CaptureInlineComments(
    TSqlScript script,
    List<SqlCommentInfo> comments)
        {
            _inlineComments.Clear();

            foreach (TSqlBatch batch in script.Batches)
            {
                foreach (TSqlStatement statement in batch.Statements)
                {
                    CaptureInlineCommentsFromStatement(
                        statement,
                        comments);
                }
            }
        }
        private void BindInlineComment(
    TSqlFragment fragment,
    List<SqlCommentInfo> comments)
        {
            if (fragment == null ||
                comments == null)
            {
                return;
            }

            string comment =
                FindInlineCommentForFragment(
                    fragment,
                    comments);

            if (!string.IsNullOrWhiteSpace(comment))
            {
                _inlineComments[fragment.StartOffset] =
                    comment;
            }
        }
        private void CaptureTableReferenceInlineComments(
    TableReference tableReference,
    List<SqlCommentInfo> comments)
        {
            if (tableReference == null)
            {
                return;
            }

            BindInlineComment(
                tableReference,
                comments);

            if (tableReference is QualifiedJoin join)
            {
                CaptureTableReferenceInlineComments(
                    join.FirstTableReference,
                    comments);

                CaptureTableReferenceInlineComments(
                    join.SecondTableReference,
                    comments);

                CaptureBooleanInlineComments(
                    join.SearchCondition,
                    comments);
            }
        }

        private void WriteLeadingCommentsBeforeOffset(
    SqlFormatWriter writer,
    int offset,
    int indentLevel)
        {
            if (_currentComments == null ||
                _currentComments.Count == 0)
            {
                return;
            }

            while (_currentCommentIndex < _currentComments.Count &&
                   _currentComments[_currentCommentIndex].Offset < offset)
            {
                writer.WriteLine(
                    indentLevel,
                    _currentComments[_currentCommentIndex].Text);

                _currentCommentIndex++;
            }
        }
        private void CaptureInlineCommentsFromQueryExpression(
    QueryExpression queryExpression,
    List<SqlCommentInfo> comments)
        {
            if (queryExpression is QuerySpecification query)
            {
                foreach (SelectElement element in query.SelectElements)
                {
                    BindInlineComment(
                        element,
                        comments);
                }

                if (query.FromClause != null)
                {
                    foreach (TableReference tableReference
                        in query.FromClause.TableReferences)
                    {
                        CaptureTableReferenceInlineComments(
                            tableReference,
                            comments);
                    }
                }

                CaptureBooleanInlineComments(
                    query.WhereClause?.SearchCondition,
                    comments);

                return;
            }

            if (queryExpression is BinaryQueryExpression binary)
            {
                CaptureInlineCommentsFromQueryExpression(
                    binary.FirstQueryExpression,
                    comments);

                CaptureInlineCommentsFromQueryExpression(
                    binary.SecondQueryExpression,
                    comments);

                return;
            }

            if (queryExpression is QueryParenthesisExpression parenthesis)
            {
                CaptureInlineCommentsFromQueryExpression(
                    parenthesis.QueryExpression,
                    comments);
            }
        }

        private string GetTableReferenceInlineComment(
    TableReference tableReference)
        {
            return GetInlineComment(
                tableReference);
        }
        private void CaptureBooleanInlineComments(
    BooleanExpression expression,
    List<SqlCommentInfo> comments)
        {
            if (expression == null)
            {
                return;
            }

            if (expression is BooleanBinaryExpression binary)
            {
                CaptureBooleanInlineComments(
                    binary.FirstExpression,
                    comments);

                CaptureBooleanInlineComments(
                    binary.SecondExpression,
                    comments);

                return;
            }

            if (expression is BooleanParenthesisExpression parenthesis)
            {
                CaptureBooleanInlineComments(
                    parenthesis.Expression,
                    comments);

                return;
            }

            BindInlineComment(
                expression,
                comments);
        }
        private string GetBooleanInlineComment(
    BooleanExpression expression)
        {
            return GetInlineComment(
                expression);
        }
        private bool HasUnsupportedInlineComment(
    List<SqlCommentInfo> comments)
        {
            foreach (SqlCommentInfo comment in comments)
            {
                if (comment.Placement !=
                    CommentPlacement.Inline)
                {
                    continue;
                }

                bool handled =
                    _inlineComments
                        .Values
                        .Any(x => x == comment.Text);

                if (!handled)
                {
                    return true;
                }
            }

            return false;
        }
        private string GetSetClauseInlineComment(
    SetClause setClause)
        {
            return GetInlineComment(
                setClause);
        }
        private void CaptureSetClauseInlineComments(
    IList<SetClause> setClauses,
    List<SqlCommentInfo> comments)
        {
            if (setClauses == null)
            {
                return;
            }

            foreach (SetClause setClause in setClauses)
            {
                BindInlineComment(
                    setClause,
                    comments);
            }
        }
        private void CaptureInlineCommentsFromStatement(
    TSqlStatement statement,
    List<SqlCommentInfo> comments)
        {
            if (statement is SelectStatement selectStatement)
            {
                CaptureInlineCommentsFromQueryExpression(
                    selectStatement.QueryExpression,
                    comments);
            }

            if (statement is UpdateStatement updateStatement)
            {
                CaptureSetClauseInlineComments(
                    updateStatement.UpdateSpecification.SetClauses,
                    comments);

                CaptureBooleanInlineComments(
                    updateStatement.UpdateSpecification.WhereClause?.SearchCondition,
                    comments);
            }

            if (statement is DeleteStatement deleteStatement)
            {
                CaptureBooleanInlineComments(
                    deleteStatement.DeleteSpecification.WhereClause?.SearchCondition,
                    comments);
            }
        }
        private string FindInlineCommentForFragment(
    TSqlFragment fragment,
    List<SqlCommentInfo> comments)
        {
            if (fragment == null ||
                comments == null)
            {
                return null;
            }

            int fragmentEndOffset =
                fragment.StartOffset +
                fragment.FragmentLength;

            int fragmentEndLine =
                GetLineNumberFromOffset(
                    _sourceSql,
                    fragmentEndOffset);

            SqlCommentInfo comment =
                comments
                    .Where(x =>
                        x.Placement == CommentPlacement.Inline &&
                        x.Line == fragmentEndLine &&
                        x.Offset >= fragmentEndOffset)
                    .OrderBy(x => x.Offset)
                    .FirstOrDefault();

            return comment?.Text;
        }
        private int GetLineNumberFromOffset(
    string sql,
    int offset)
        {
            int lineNumber =
                1;

            for (int i = 0; i < offset && i < sql.Length; i++)
            {
                if (sql[i] == '\n')
                {
                    lineNumber++;
                }
            }

            return lineNumber;
        }
        private List<SqlCommentInfo> ExtractComments(
    TSqlFragment fragment,
    string sql)
        {
            var comments =
                new List<SqlCommentInfo>();

            if (fragment.ScriptTokenStream == null)
            {
                return comments;
            }

            foreach (TSqlParserToken token in fragment.ScriptTokenStream)
            {
                if (token.TokenType != TSqlTokenType.SingleLineComment &&
                    token.TokenType != TSqlTokenType.MultilineComment)
                {
                    continue;
                }

                comments.Add(
                    new SqlCommentInfo
                    {
                        Offset = token.Offset,
                        Line = token.Line,
                        Column = token.Column,
                        Text = token.Text,
                        Placement =
                            HasCodeBeforeCommentOnSameLine(sql, token.Offset)
                                ? CommentPlacement.Inline
                                : CommentPlacement.Leading
                    });
            }

            return comments
                .OrderBy(x => x.Offset)
                .ToList();
        }
        private bool HasInlineComment(
    List<SqlCommentInfo> comments)
        {
            return comments.Any(
                x => x.Placement == CommentPlacement.Inline);
        }
        private bool HasCodeBeforeCommentOnSameLine(
    string sql,
    int commentOffset)
        {
            int lineStart =
                sql.LastIndexOf(
                    '\n',
                    Math.Max(0, commentOffset - 1));

            if (lineStart < 0)
            {
                lineStart = 0;
            }
            else
            {
                lineStart++;
            }

            string beforeComment =
                sql.Substring(
                    lineStart,
                    commentOffset - lineStart);

            return beforeComment.Trim().Length > 0;
        }
        public string Format(
    string sql)
        {
            _sourceSql =
                sql;

            _hasLeadingSemicolonBeforeCte =
                Regex.IsMatch(
                    sql,
                    @"^\s*;\s*WITH\b",
                    RegexOptions.IgnoreCase);

            var parser =
                new TSql160Parser(false);

            TSqlFragment fragment;

            IList<ParseError> errors;

            using (var reader =
                new StringReader(sql))
            {
                fragment =
                    parser.Parse(
                        reader,
                        out errors);
            }

            if (errors != null &&
                errors.Count > 0)
            {
                return sql;
            }

            List<SqlCommentInfo> comments =
                ExtractComments(
                    fragment,
                    sql);

            if (fragment is TSqlScript script)
            {
                CaptureInlineComments(
                    script,
                    comments);

                if (HasUnsupportedInlineComment(
                        comments))
                {
                    return sql;
                }

                _currentComments =
                    comments
                        .Where(x =>
                            x.Placement ==
                            CommentPlacement.Leading)
                        .OrderBy(x => x.Offset)
                        .ToList();

                _currentCommentIndex =
                    0;

                return FormatScript(
                    script);
            }

            return GetFragmentText(
                fragment);
        }
        private void WriteLeadingCommentsBeforeStatement(
    SqlFormatWriter writer,
    TSqlStatement statement,
    int indentLevel)
        {
            if (_currentComments == null ||
                _currentComments.Count == 0)
            {
                return;
            }

            while (_currentCommentIndex < _currentComments.Count &&
                   _currentComments[_currentCommentIndex].Offset < statement.StartOffset)
            {
                writer.WriteLine(
                    indentLevel,
                    _currentComments[_currentCommentIndex].Text);

                //writer.WriteRawLine(string.Empty);

                _currentCommentIndex++;
            }
        }
        private List<SqlCommentInfo> _currentComments =
    new List<SqlCommentInfo>();

        private int _currentCommentIndex;
        private string ApplyLeadingComments(
    string formattedSql,
    List<SqlCommentInfo> comments)
        {
            if (comments == null ||
                comments.Count == 0)
            {
                return formattedSql;
            }

            var builder =
                new StringBuilder();

            foreach (SqlCommentInfo comment in comments)
            {
                if (comment.Placement != CommentPlacement.Leading)
                {
                    continue;
                }

                builder.AppendLine(comment.Text);
                builder.AppendLine();
            }

            builder.Append(formattedSql);

            return builder.ToString();
        }
        private string FormatSelectStatement(
    SelectStatement selectStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            FormatCtes(
                writer,
                selectStatement.WithCtesAndXmlNamespaces,
                indentLevel);

            writer.WriteRaw(
                FormatQueryExpression(
                    selectStatement.QueryExpression,
                    indentLevel));

            FormatOrderBy(
                writer,
                selectStatement.QueryExpression.OrderByClause);

            FormatOffsetFetch(
                writer,
                selectStatement.QueryExpression.OffsetClause);

            return writer.ToString();
        }
        private void FormatOffsetFetch(
    SqlFormatWriter writer,
    OffsetClause offsetClause)
        {
            if (offsetClause == null)
            {
                return;
            }

            writer.WriteLine(
                0,
                GetFragmentText(offsetClause));
        }
        private string GetBinaryQueryOperatorText(
    BinaryQueryExpressionType expressionType,
    bool isAll)
        {
            if (expressionType ==
                BinaryQueryExpressionType.Union)
            {
                return isAll
                    ? "UNION ALL"
                    : "UNION";
            }

            if (expressionType ==
                BinaryQueryExpressionType.Except)
            {
                return "EXCEPT";
            }

            if (expressionType ==
                BinaryQueryExpressionType.Intersect)
            {
                return "INTERSECT";
            }

            return
                expressionType
                    .ToString()
                    .ToUpper();
        }
        private string FormatBinaryQueryExpression(
    BinaryQueryExpression binary,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteRaw(
                FormatQueryExpression(
                    binary.FirstQueryExpression,
                    indentLevel));

            writer.WriteRawLine(
                string.Empty);

            writer.WriteLine(
                indentLevel,
                GetBinaryQueryOperatorText(
                    binary.BinaryQueryExpressionType,
                    binary.All));

            writer.WriteRawLine(
                string.Empty);

            writer.WriteRaw(
                FormatQueryExpression(
                    binary.SecondQueryExpression,
                    indentLevel));

            return writer.ToString();
        }
        private string FormatQueryExpression(
    QueryExpression queryExpression,
    int indentLevel)
        {
            if (queryExpression is QuerySpecification query)
            {
                return FormatSelect(
                    query,
                    indentLevel);
            }

            if (queryExpression is BinaryQueryExpression binary)
            {
                return FormatBinaryQueryExpression(
                    binary,
                    indentLevel);
            }

            if (queryExpression is QueryParenthesisExpression parenthesis)
            {
                return FormatQueryExpression(
                    parenthesis.QueryExpression,
                    indentLevel);
            }

            return GetFragmentText(
                queryExpression);
        }
        private void FormatCtes(
    SqlFormatWriter writer,
    WithCtesAndXmlNamespaces withCtes,
    int indentLevel)
        {
            if (withCtes == null ||
                withCtes.CommonTableExpressions.Count == 0)
            {
                return;
            }

            for (int i = 0; i < withCtes.CommonTableExpressions.Count; i++)
            {
                CommonTableExpression cte =
                    withCtes.CommonTableExpressions[i];

                string cteHeader;

                if (i == 0)
                {
                    string withPrefix =
                        _hasLeadingSemicolonBeforeCte
                            ? ";WITH "
                            : "WITH ";

                    cteHeader =
                        withPrefix +
                        cte.ExpressionName.Value +
                        " AS";
                }
                else
                {
                    cteHeader =
                        "," +
                        cte.ExpressionName.Value +
                        " AS";
                }

                writer.WriteLine(
                    cteHeader);

                writer.WriteLine("(");

                WriteLeadingCommentsBeforeOffset(
                    writer,
                    cte.QueryExpression.StartOffset,
                    indentLevel + 1);

                string formattedQuery =
                    FormatQueryExpression(
                        cte.QueryExpression,
                        indentLevel);

                WriteIndentedFormattedQuery(
                    writer,
                    formattedQuery,
                    "\t");

                writer.WriteLine(")");
            }
        }
        private IList<FormattedLine> FormatSelectScalarExpression(
    SelectScalarExpression scalar)
        {
            IList<FormattedLine> expressionLines =
                FormatExpressionLines(
                    scalar.Expression);

            if (scalar.ColumnName != null &&
                expressionLines.Count > 0)
            {
                FormattedLine last =
                    expressionLines[
                        expressionLines.Count - 1];

                last.Text =
                    last.Text +
                    " AS " +
                    GetFragmentText(
                        scalar.ColumnName);
            }

            return expressionLines;
        }
        private IList<FormattedLine> FormatSelectElement(
     SelectElement selectElement)
        {
            if (selectElement is SelectScalarExpression scalar)
            {
                IList<FormattedLine> lines =
                    FormatSelectScalarExpression(
                        scalar);

                if (lines.Count > 0)
                {
                    lines[
                        lines.Count - 1].Text +=
                        GetInlineComment(
                            selectElement);
                }

                return lines;
            }
            if (selectElement is SelectSetVariable setVariable)
            {
                return FormatSelectSetVariable(
                    setVariable);
            }
            IList<FormattedLine> defaultLines =
                new List<FormattedLine>
                {
                    new FormattedLine
                    {
                        RelativeIndent = 0,
                        Text =
                            GetFragmentText(
                                selectElement) +
                            GetInlineComment(
                                selectElement)
                    }
                };

            return defaultLines;
        }
        private IList<FormattedLine> FormatSelectSetVariable(
    SelectSetVariable setVariable)
        {
            IList<FormattedLine> expressionLines =
                FormatExpressionLines(
                    setVariable.Expression);

            if (expressionLines == null ||
                expressionLines.Count == 0)
            {
                return new List<FormattedLine>();
            }

            if (expressionLines.Count == 1)
            {
                expressionLines[0].Text =
                    setVariable.Variable.Name +
                    " = " +
                    expressionLines[0].Text;

                return expressionLines;
            }

            expressionLines.Insert(
                0,
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text =
                        setVariable.Variable.Name +
                        " ="
                });

            for (int i = 1;
                 i < expressionLines.Count;
                 i++)
            {
                expressionLines[i].RelativeIndent++;
            }

            return expressionLines;
        }
        private IList<FormattedLine> FormatSearchedCaseExpressionLines(
    SearchedCaseExpression caseExpression)
        {
            var lines = new List<FormattedLine>();

            lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text = "CASE"
                });

            foreach (SearchedWhenClause whenClause in caseExpression.WhenClauses)
            {
                lines.Add(
                    new FormattedLine
                    {
                        RelativeIndent = 1,
                        Text =
                            "WHEN " +
                            GetFragmentText(whenClause.WhenExpression)
                    });

                IList<FormattedLine> thenLines =
                    FormatExpressionLines(
                        whenClause.ThenExpression);

                if (thenLines.Count == 1)
                {
                    lines.Add(
                        new FormattedLine
                        {
                            RelativeIndent = 2,
                            Text =
                                "THEN " +
                                thenLines[0].Text
                        });
                }
                else
                {
                    lines.Add(
                        new FormattedLine
                        {
                            RelativeIndent = 2,
                            Text = "THEN"
                        });

                    foreach (FormattedLine line in thenLines)
                    {
                        lines.Add(
                            new FormattedLine
                            {
                                RelativeIndent =
                                    line.RelativeIndent + 3,

                                Text =
                                    line.Text
                            });
                    }
                }
            }

            if (caseExpression.ElseExpression != null)
            {
                lines.Add(
                    new FormattedLine
                    {
                        RelativeIndent = 1,
                        Text = "ELSE"
                    });

                IList<FormattedLine> elseLines =
                    FormatExpressionLines(
                        caseExpression.ElseExpression);

                if (elseLines.Count == 1)
                {
                    lines.Add(
                        new FormattedLine
                        {
                            RelativeIndent = 2,
                            Text = elseLines[0].Text
                        });
                }
                else
                {
                    foreach (FormattedLine line in elseLines)
                    {
                        lines.Add(
                            new FormattedLine
                            {
                                RelativeIndent =
                                    line.RelativeIndent + 2,

                                Text =
                                    line.Text
                            });
                    }
                }
            }

            lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text = "END"
                });

            return lines;
        }
        private void WriteSelectElementLines(
    SqlFormatWriter writer,
    IList<FormattedLine> lines,
    bool isFirstColumn)
        {
            if (lines == null ||
                lines.Count == 0)
            {
                return;
            }

            bool isMultiline =
                lines.Count > 1;

            if (!isFirstColumn &&
                isMultiline)
            {
                writer.WriteLine(
                    1,
                    ",");
            }

            for (int i = 0; i < lines.Count; i++)
            {
                FormattedLine line =
                    lines[i];

                string prefix =
                    string.Empty;

                if (!isFirstColumn &&
                    !isMultiline &&
                    i == 0)
                {
                    prefix = ",";
                }

                writer.WriteLine(
                    1 + line.RelativeIndent,
                    prefix + line.Text);
            }
        }
        private IList<FormattedLine> FormatFunctionCallLines(
    FunctionCall functionCall)
        {
            IList<FormattedSqlNode> argumentNodes =
                functionCall.Parameters
                    .Select(FormatScalarExpressionNode)
                    .ToList();

            return FormatCallLikeLines(
                functionCall.FunctionName.Value,
                argumentNodes,
                functionCall);
        }
        private IList<FormattedLine> FormatExpressionLines(
    ScalarExpression expression)
        {
            return
                FormatScalarExpressionNode(expression)
                    .Lines;
        }
        private IList<FormattedLine> FormatSimpleCaseExpressionLines(
    SimpleCaseExpression caseExpression)
        {
            var lines =
                new List<FormattedLine>();

            string header =
                "CASE";

            if (caseExpression.InputExpression != null)
            {
                header +=
                    " " +
                    GetFragmentText(
                        caseExpression.InputExpression);
            }

            lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text = header
                });

            foreach (
                SimpleWhenClause whenClause
                in caseExpression.WhenClauses)
            {
                lines.Add(
                    new FormattedLine
                    {
                        RelativeIndent = 1,
                        Text =
                            "WHEN " +
                            GetFragmentText(
                                whenClause.WhenExpression)
                    });

                IList<FormattedLine> thenLines =
                    FormatExpressionLines(
                        whenClause.ThenExpression);

                if (thenLines.Count == 1)
                {
                    lines.Add(
                        new FormattedLine
                        {
                            RelativeIndent = 2,
                            Text =
                                "THEN " +
                                thenLines[0].Text
                        });
                }
                else
                {
                    lines.Add(
                        new FormattedLine
                        {
                            RelativeIndent = 2,
                            Text = "THEN"
                        });

                    foreach (
                        FormattedLine line
                        in thenLines)
                    {
                        lines.Add(
                            new FormattedLine
                            {
                                RelativeIndent =
                                    line.RelativeIndent + 3,

                                Text =
                                    line.Text
                            });
                    }
                }
            }

            if (caseExpression.ElseExpression != null)
            {
                lines.Add(
                    new FormattedLine
                    {
                        RelativeIndent = 1,
                        Text = "ELSE"
                    });

                IList<FormattedLine> elseLines =
                    FormatExpressionLines(
                        caseExpression.ElseExpression);

                if (elseLines.Count == 1)
                {
                    lines.Add(
                        new FormattedLine
                        {
                            RelativeIndent = 2,
                            Text =
                                elseLines[0].Text
                        });
                }
                else
                {
                    foreach (
                        FormattedLine line
                        in elseLines)
                    {
                        lines.Add(
                            new FormattedLine
                            {
                                RelativeIndent =
                                    line.RelativeIndent + 2,

                                Text =
                                    line.Text
                            });
                    }
                }
            }

            lines.Add(
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text = "END"
                });

            return lines;
        }
        private string FormatScript(
    TSqlScript script)
        {
            var writer =
                new SqlFormatWriter();

            foreach (TSqlBatch batch in script.Batches)
            {
                writer.WriteRaw(
                    FormatStatementList(
                        batch.Statements,
                        0));
            }

            WriteRemainingLeadingComments(
                writer,
                0);

            return writer.ToString();
        }
        private void WriteRemainingLeadingComments(
    SqlFormatWriter writer,
    int indentLevel)
        {
            if (_currentComments == null ||
                _currentComments.Count == 0)
            {
                return;
            }

            while (_currentCommentIndex <
                   _currentComments.Count)
            {
                writer.WriteLine(
                    indentLevel,
                    _currentComments[
                        _currentCommentIndex].Text);

                //writer.WriteRawLine(string.Empty);

                _currentCommentIndex++;
            }
        }
        private string FormatExpression(
    ScalarExpression expression)
        {
            if (expression is SearchedCaseExpression searchedCase)
            {
                return FormatSearchedCaseExpression(
                    searchedCase,
                    1);
            }

            return GetFragmentText(expression);
        }
        private string FormatSearchedCaseExpression(
    SearchedCaseExpression caseExpression,
    int indentLevel)
        {
            var writer = new SqlFormatWriter();

            writer.WriteLine("CASE");

            foreach (SearchedWhenClause whenClause in caseExpression.WhenClauses)
            {
                writer.WriteLine(
                    1,
                    "WHEN " +
                    GetFragmentText(whenClause.WhenExpression));

                writer.WriteLine(
                    2,
                    "THEN " +
                    FormatExpression(whenClause.ThenExpression));
            }

            if (caseExpression.ElseExpression != null)
            {
                writer.WriteLine(1, "ELSE");

                writer.WriteLine(
                    2,
                    FormatExpression(caseExpression.ElseExpression));
            }

            writer.WriteLine("END");

            return writer.ToString().TrimEnd();
        }

        private string FormatSelect(
     QuerySpecification query,
     int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            string selectText = "SELECT";

            if (query.TopRowFilter != null)
            {
                selectText +=
                    " " +
                    GetFragmentText(
                        query.TopRowFilter);
            }

            writer.WriteLine(
                indentLevel,
                selectText);

            for (int i = 0;
                i < query.SelectElements.Count;
                i++)
            {
                IList<FormattedLine> lines =
                    FormatSelectElement(
                        query.SelectElements[i]);

                if (lines == null ||
                    lines.Count == 0)
                {
                    continue;
                }

                if (i == 0)
                {
                    writer.WriteLine(
                        indentLevel + 1,
                        lines[0].Text);

                    for (int j = 1;
                        j < lines.Count;
                        j++)
                    {
                        writer.WriteLine(
                            indentLevel +
                            1 +
                            lines[j].RelativeIndent,
                            lines[j].Text);
                    }
                }
                else
                {
                    if (lines.Count == 1)
                    {
                        writer.WriteLine(
                            indentLevel + 1,
                            "," +
                            lines[0].Text);
                    }
                    else
                    {
                        writer.WriteLine(
                            indentLevel + 1,
                            ",");

                        for (int j = 0;
                            j < lines.Count;
                            j++)
                        {
                            writer.WriteLine(
                                indentLevel +
                                1 +
                                lines[j].RelativeIndent,
                                lines[j].Text);
                        }
                    }
                }
            }

            FormatFromClause(
                writer,
                query.FromClause,
                indentLevel);

            FormatWhere(
                writer,
                query.WhereClause,
                indentLevel);

            FormatGroupBy(
                writer,
                query.GroupByClause);

            FormatHaving(
                writer,
                query.HavingClause);
            /*
            FormatOrderBy(
                writer,
                query.OrderByClause);*/

            return writer.ToString();
        }
        private void WriteIndentedMultilineText(
    SqlFormatWriter writer,
    string text,
    int indentLevel,
    string firstLinePrefix)
        {
            string[] lines =
                text
                    .Replace("\r\n", "\n")
                    .Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                string prefix =
                    i == 0 && firstLinePrefix != null
                        ? firstLinePrefix
                        : string.Empty;

                writer.WriteLine(
                    indentLevel,
                    prefix + lines[i]);
            }
        }
        private void FormatTableReference(
    SqlFormatWriter writer,
    TableReference tableReference,
    int indentLevel,
    bool includeFromKeyword)
        {
            if (tableReference is QualifiedJoin qualifiedJoin)
            {
                FormatQualifiedJoin(
                    writer,
                    qualifiedJoin,
                    indentLevel,
                    includeFromKeyword);

                return;
            }

            if (tableReference is QueryDerivedTable derivedTable)
            {
                FormatDerivedTable(
                    writer,
                    derivedTable,
                    indentLevel,
                    includeFromKeyword);

                return;
            }

            string prefix =
                includeFromKeyword
                    ? "FROM "
                    : string.Empty;

            writer.WriteRawLine(
                writer.Indent(indentLevel) +
                prefix +
                GetFragmentText(tableReference));
        }
        private void FormatFromClause(
    SqlFormatWriter writer,
    FromClause fromClause,
    int indentLevel)
        {
            if (fromClause == null ||
                fromClause.TableReferences.Count == 0)
            {
                return;
            }

            FormatTableReference(
                writer,
                fromClause.TableReferences[0],
                indentLevel,
                true);

            for (int i = 1;
                i < fromClause.TableReferences.Count;
                i++)
            {
                writer.WriteLine(
                    indentLevel + 1,
                    "," +
                    GetFragmentText(
                        fromClause.TableReferences[i]));
            }
        }
        private void FormatQualifiedJoin(
    SqlFormatWriter writer,
    QualifiedJoin join,
    int indentLevel,
    bool includeFromKeyword)
        {
            FormatTableReference(
                writer,
                join.FirstTableReference,
                indentLevel,
                includeFromKeyword);

            int joinIndentLevel =
                indentLevel + 1;

            int onIndentLevel =
                joinIndentLevel + 1;

            string joinIndent =
                writer.Indent(
                    joinIndentLevel);

            string joinKeyword =
                GetJoinKeyword(
                    join.QualifiedJoinType);

            if (join.SecondTableReference
                is QueryDerivedTable)
            {
                writer.WriteRawLine(
                    joinIndent +
                    joinKeyword);

                FormatTableReference(
                    writer,
                    join.SecondTableReference,
                    joinIndentLevel,
                    false);
            }
            else
            {
                writer.WriteRawLine(
    joinIndent +
    joinKeyword +
    " " +
    GetFragmentText(
        join.SecondTableReference) +
    GetInlineComment(
        join.SecondTableReference));
            }

            FormatJoinCondition(
                writer,
                join.SearchCondition,
                onIndentLevel,
                true);
        }

        private string FormatCaseExpression(
    CaseExpression caseExpression,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            string indent =
                writer.Indent(indentLevel);

            writer.WriteRawLine(indent + "CASE");

            if (caseExpression is SearchedCaseExpression searched)
            {
                foreach (SearchedWhenClause whenClause in searched.WhenClauses)
                {
                    writer.WriteRawLine(
                        indent +
                        "\tWHEN " +
                        GetFragmentText(whenClause.WhenExpression));

                    writer.WriteRawLine(
                        indent +
                        "\tTHEN " +
                        GetFragmentText(whenClause.ThenExpression));
                }
            }
            else if (caseExpression is SimpleCaseExpression simple)
            {
                if (simple.InputExpression != null)
                {
                    writer.WriteRawLine(
                        indent +
                        "\t" +
                        GetFragmentText(simple.InputExpression));
                }

                foreach (SimpleWhenClause whenClause in simple.WhenClauses)
                {
                    writer.WriteRawLine(
                        indent +
                        "\tWHEN " +
                        GetFragmentText(whenClause.WhenExpression));

                    writer.WriteRawLine(
                        indent +
                        "\tTHEN " +
                        GetFragmentText(whenClause.ThenExpression));
                }
            }

            if (caseExpression.ElseExpression != null)
            {
                writer.WriteRawLine(
                    indent +
                    "\tELSE " +
                    GetFragmentText(caseExpression.ElseExpression));
            }

            writer.WriteRawLine(indent + "END");

            return writer.ToString().TrimEnd();
        }
        private void FormatDerivedTable(
            SqlFormatWriter writer,
            QueryDerivedTable derivedTable,
            int indentLevel,
            bool includeFromKeyword)
        {
            if (includeFromKeyword)
            {
                writer.WriteLine("FROM");
            }

            string indent = writer.Indent(indentLevel);

            writer.WriteRawLine(indent + "(");

            string formattedQuery =
                FormatQueryExpression(
                    derivedTable.QueryExpression,
                    indentLevel);

            WriteIndentedFormattedQuery(
                writer,
                formattedQuery,
                indent + "\t");

            string closingLine = indent + ")";

            if (derivedTable.Alias != null)
            {
                closingLine += " " + derivedTable.Alias.Value;
            }

            writer.WriteRawLine(closingLine);
        }
        private string FormatBeginEndBlock(
    BeginEndBlockStatement block,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteLine(indentLevel, "BEGIN");
            /*
            foreach (TSqlStatement statement in block.StatementList.Statements)
            {
                string formattedStatement =
                    FormatStatement(
                        statement,
                        indentLevel + 1);

                writer.WriteRaw(formattedStatement);
            }*/

            writer.WriteRaw(
    FormatStatementList(
        block.StatementList.Statements,
        indentLevel + 1));

            writer.WriteLine(indentLevel, "END");

            return writer.ToString();
        }

        private string FormatIfStatement(
    IfStatement ifStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteRaw(FormatIfPredicate(ifStatement.Predicate, indentLevel));

            if (ifStatement.ThenStatement
                is BeginEndBlockStatement thenBlock)
            {
                writer.WriteRaw(
                    FormatBeginEndBlock(
                        thenBlock,
                        indentLevel));
            }
            else
            {
                writer.WriteRaw(
                    FormatStatement(
                        ifStatement.ThenStatement,
                        indentLevel + 1));
            }

            if (ifStatement.ElseStatement != null)
            {
                if (ifStatement.ElseStatement
                    is IfStatement nestedIf)
                {
                    string nestedText =
                        FormatIfStatement(
                            nestedIf,
                            indentLevel);

                    nestedText =
                        nestedText.TrimStart();

                    writer.WriteRaw(
                        writer.Indent(
                            indentLevel) +
                        "ELSE " +
                        nestedText);
                }
                else
                {
                    writer.WriteLine(
                        indentLevel,
                        "ELSE");

                    if (ifStatement.ElseStatement
                        is BeginEndBlockStatement elseBlock)
                    {
                        writer.WriteRaw(
                            FormatBeginEndBlock(
                                elseBlock,
                                indentLevel));
                    }
                    else
                    {
                        writer.WriteRaw(
                            FormatStatement(
                                ifStatement.ElseStatement,
                                indentLevel + 1));
                    }
                }
            }

            return writer.ToString();
        }
        private string FormatTryCatchStatement(
    TryCatchStatement tryCatch,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteLine(
                indentLevel,
                "BEGIN TRY");

            writer.WriteRaw(
                FormatStatementList(
                    tryCatch.TryStatements.Statements,
                    indentLevel + 1));

            writer.WriteLine(
                indentLevel,
                "END TRY");

            writer.WriteLine(
                indentLevel,
                "BEGIN CATCH");

            writer.WriteRaw(
                FormatStatementList(
                    tryCatch.CatchStatements.Statements,
                    indentLevel + 1));

            writer.WriteLine(
                indentLevel,
                "END CATCH");

            return writer.ToString();
        }

        private void FormatProcedureParameters(
    SqlFormatWriter writer,
    IList<ProcedureParameter> parameters,
    int indentLevel)
        {
            if (parameters == null ||
                parameters.Count == 0)
            {
                return;
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                ProcedureParameter parameter =
                    parameters[i];

                string parameterText =
                    parameter.VariableName.Value +
                    " " +
                    FormatDataType(
                        parameter.DataType);

                if (parameter.Modifier ==
                    ParameterModifier.Output)
                {
                    parameterText +=
                        " OUTPUT";
                }

                if (parameter.Value != null)
                {
                    parameterText +=
                        " = " +
                        GetFragmentText(
                            parameter.Value);
                }

                if (i == 0)
                {
                    writer.WriteLine(
                        indentLevel,
                        parameterText);
                }
                else
                {
                    writer.WriteLine(
                        indentLevel,
                        "," +
                        parameterText);
                }
            }
        }
        private string FormatDataType(
    DataTypeReference dataType)
        {
            return Regex.Replace(
                GetFragmentText(dataType),
                @"\b([A-Za-z0-9_]+)\s+\(",
                "$1(");
        }
        private string FormatCreateProcedureStatement(
    CreateProcedureStatement procedure,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteLine(
    indentLevel,
    "CREATE PROCEDURE " +
    GetFragmentText(
        procedure.ProcedureReference));

            FormatProcedureParameters(
                writer,
                procedure.Parameters,
                indentLevel + 1);

            writer.WriteLine(
                indentLevel,
                "AS");

            if (procedure.StatementList != null)
            {
                writer.WriteRaw(
                    FormatStatementList(
                        procedure.StatementList.Statements,
                        indentLevel));
            }

            return writer.ToString();
        }
        private string FormatExecuteStatement(
    ExecuteStatement executeStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            if (executeStatement.ExecuteSpecification
                .ExecutableEntity
                is ExecutableProcedureReference procedureReference)
            {
                writer.WriteLine(
                    indentLevel,
                    "EXEC " +
                    GetFragmentText(
                        procedureReference.ProcedureReference));

                IList<ExecuteParameter> parameters =
                    procedureReference.Parameters;

                for (int i = 0;
                    i < parameters.Count;
                    i++)
                {
                    string parameterText =
                        GetFragmentText(
                            parameters[i]);

                    if (i == 0)
                    {
                        writer.WriteLine(
                            indentLevel + 1,
                            parameterText);
                    }
                    else
                    {
                        writer.WriteLine(
                            indentLevel + 1,
                            "," +
                            parameterText);
                    }
                }

                return writer.ToString();
            }

            writer.WriteLine(
                indentLevel,
                GetFragmentText(
                    executeStatement));

            return writer.ToString();
        }
        private void FormatInsertSource(
    SqlFormatWriter writer,
    InsertSource insertSource,
    int indentLevel)
        {
            if (insertSource is SelectInsertSource selectSource)
            {
                writer.WriteRaw(
                    FormatQueryExpression(
                        selectSource.Select,
                        indentLevel));

                return;
            }

            if (insertSource is ValuesInsertSource valuesSource)
            {
                writer.WriteLine(indentLevel, "VALUES");

                for (int i = 0; i < valuesSource.RowValues.Count; i++)
                {
                    if (i > 0)
                    {
                        writer.WriteLine(indentLevel, ",");
                    }

                    FormatRowValue(
                        writer,
                        valuesSource.RowValues[i],
                        indentLevel);
                }

                return;
            }

            if (insertSource is ExecuteInsertSource executeSource)
            {
                writer.WriteRaw(
                    FormatExecuteSpecification(
                        executeSource.Execute,
                        indentLevel));

                return;
            }

            writer.WriteLine(
                indentLevel,
                GetFragmentText(insertSource));
        }
        private string FormatExecuteSpecification(
    ExecuteSpecification executeSpecification,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            if (executeSpecification.ExecutableEntity
                is ExecutableProcedureReference procedureReference)
            {
                writer.WriteLine(
                    indentLevel,
                    "EXEC " +
                    GetFragmentText(
                        procedureReference.ProcedureReference));

                for (int i = 0;
                    i < procedureReference.Parameters.Count;
                    i++)
                {
                    string parameterText =
                        GetFragmentText(
                            procedureReference.Parameters[i]);

                    if (i == 0)
                    {
                        writer.WriteLine(
                            indentLevel + 1,
                            parameterText);
                    }
                    else
                    {
                        writer.WriteLine(
                            indentLevel + 1,
                            "," + parameterText);
                    }
                }

                return writer.ToString();
            }

            writer.WriteLine(
                indentLevel,
                GetFragmentText(
                    executeSpecification));

            return writer.ToString();
        }
        private void FormatRowValue(
    SqlFormatWriter writer,
    RowValue rowValue,
    int indentLevel)
        {
            writer.WriteLine(indentLevel, "(");

            for (int i = 0; i < rowValue.ColumnValues.Count; i++)
            {
                string valueText =
                    GetFragmentText(
                        rowValue.ColumnValues[i]);

                if (i == 0)
                {
                    writer.WriteLine(indentLevel + 1, valueText);
                }
                else
                {
                    writer.WriteLine(indentLevel + 1, "," + valueText);
                }
            }

            writer.WriteLine(indentLevel, ")");
        }
        private void FormatInsertColumns(
    SqlFormatWriter writer,
    IList<ColumnReferenceExpression> columns,
    int indentLevel)
        {
            if (columns == null ||
                columns.Count == 0)
            {
                return;
            }

            writer.WriteLine(indentLevel, "(");

            for (int i = 0; i < columns.Count; i++)
            {
                string columnText =
                    GetFragmentText(columns[i]);

                if (i == 0)
                {
                    writer.WriteLine(indentLevel + 1, columnText);
                }
                else
                {
                    writer.WriteLine(indentLevel + 1, "," + columnText);
                }
            }

            writer.WriteLine(indentLevel, ")");
        }
        private string FormatInsertStatement(
    InsertStatement insertStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            InsertSpecification specification =
                insertStatement.InsertSpecification;

            writer.WriteLine(
                indentLevel,
                "INSERT INTO " +
                GetFragmentText(
                    specification.Target));

            FormatInsertColumns(
                writer,
                specification.Columns,
                indentLevel);

            FormatInsertSource(
                writer,
                specification.InsertSource,
                indentLevel);

            return writer.ToString();
        }
        private string FormatUpdateStatement(
    UpdateStatement updateStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            UpdateSpecification specification =
                updateStatement.UpdateSpecification;

            writer.WriteLine(
                indentLevel,
                "UPDATE " +
                GetFragmentText(
                    specification.Target));

            writer.WriteLine(
                indentLevel,
                "SET");

            for (int i = 0;
     i < specification.SetClauses.Count;
     i++)
            {
                IList<FormattedLine> lines =
                    FormatSetClauseLines(
                        specification.SetClauses[i]);

                if (lines == null ||
                    lines.Count == 0)
                {
                    continue;
                }

                if (i == 0)
                {
                    writer.WriteLine(
                        indentLevel + 1,
                        lines[0].Text);
                }
                else
                {
                    writer.WriteLine(
                        indentLevel + 1,
                        "," + lines[0].Text);
                }

                for (int j = 1;
                     j < lines.Count;
                     j++)
                {
                    writer.WriteLine(
                        indentLevel +
                        1 +
                        lines[j].RelativeIndent,
                        lines[j].Text);
                }
            }

            FormatFromClause(
                writer,
                specification.FromClause,
                indentLevel);

            FormatWhere(
                writer,
                specification.WhereClause,
                indentLevel);

            return writer.ToString();
        }
        private string FormatDeleteStatement(
    DeleteStatement deleteStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            DeleteSpecification specification =
                deleteStatement.DeleteSpecification;

            writer.WriteLine(
                indentLevel,
                "DELETE " +
                GetFragmentText(
                    specification.Target));

            FormatFromClause(
                writer,
                specification.FromClause,
                indentLevel);

            FormatWhere(
                writer,
                specification.WhereClause,
                indentLevel);

            return writer.ToString();
        }
        private string FormatAlterProcedureStatement(
    AlterProcedureStatement procedure,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteLine(
                indentLevel,
                "ALTER PROCEDURE " +
                GetFragmentText(
                    procedure.ProcedureReference));

            FormatProcedureParameters(
                writer,
                procedure.Parameters,
                indentLevel + 1);

            writer.WriteLine(
                indentLevel,
                "AS");

            if (procedure.StatementList != null)
            {
                writer.WriteRaw(
                    FormatStatementList(
                        procedure.StatementList.Statements,
                        indentLevel));
            }

            return writer.ToString();
        }
        private string FormatCreateOrAlterProcedureStatement(
    CreateOrAlterProcedureStatement procedure,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteLine(
                indentLevel,
                "CREATE OR ALTER PROCEDURE " +
                GetFragmentText(
                    procedure.ProcedureReference));

            FormatProcedureParameters(
                writer,
                procedure.Parameters,
                indentLevel + 1);

            writer.WriteLine(
                indentLevel,
                "AS");

            if (procedure.StatementList != null)
            {
                writer.WriteRaw(
                    FormatStatementList(
                        procedure.StatementList.Statements,
                        indentLevel));
            }

            return writer.ToString();
        }
        private string FormatCreateViewStatement(
    CreateViewStatement viewStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteLine(
                indentLevel,
                "CREATE VIEW " +
                GetFragmentText(
                    viewStatement.SchemaObjectName));

            writer.WriteLine(
                indentLevel,
                "AS");

            if (viewStatement.SelectStatement != null)
            {
                writer.WriteRaw(
                    FormatSelectStatement(
                        viewStatement.SelectStatement,
                        indentLevel));
            }

            return writer.ToString();
        }
        private string FormatAlterViewStatement(
    AlterViewStatement viewStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteLine(
                indentLevel,
                "ALTER VIEW " +
                GetFragmentText(
                    viewStatement.SchemaObjectName));

            writer.WriteLine(
                indentLevel,
                "AS");

            if (viewStatement.SelectStatement != null)
            {
                writer.WriteRaw(
                    FormatSelectStatement(
                        viewStatement.SelectStatement,
                        indentLevel));
            }

            return writer.ToString();
        }
        private string FormatCreateOrAlterFunctionStatement(
    CreateOrAlterFunctionStatement functionStatement,
    int indentLevel)
        {
            return FormatFunctionStatement(
                "CREATE OR ALTER FUNCTION",
                functionStatement,
                indentLevel);
        }
        private string FormatCreateFunctionStatement(
    CreateFunctionStatement functionStatement,
    int indentLevel)
        {
            return FormatFunctionStatement(
                "CREATE FUNCTION",
                functionStatement,
                indentLevel);
        }

        private string FormatAlterFunctionStatement(
    AlterFunctionStatement functionStatement,
    int indentLevel)
        {
            return FormatFunctionStatement(
                "ALTER FUNCTION",
                functionStatement,
                indentLevel);
        }
        private void FormatFunctionParameters(
    SqlFormatWriter writer,
    IList<ProcedureParameter> parameters,
    int indentLevel)
        {
            if (parameters == null ||
                parameters.Count == 0)
            {
                writer.WriteRawLine("()");

                return;
            }

            writer.WriteLine(
                indentLevel - 1,
                "(");

            for (int i = 0; i < parameters.Count; i++)
            {
                ProcedureParameter parameter =
                    parameters[i];

                string parameterText =
                    parameter.VariableName.Value +
                    " " +
                    FormatDataType(
                        parameter.DataType);

                if (parameter.Value != null)
                {
                    parameterText +=
                        " = " +
                        GetFragmentText(
                            parameter.Value);
                }

                if (i == 0)
                {
                    writer.WriteLine(
                        indentLevel,
                        parameterText);
                }
                else
                {
                    writer.WriteLine(
                        indentLevel,
                        "," + parameterText);
                }
            }

            writer.WriteLine(
                indentLevel - 1,
                ")");
        }
        /*private string FormatCreateOrAlterFunctionStatement(
    CreateOrAlterFunctionStatement functionStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteLine(
                indentLevel,
                "CREATE OR ALTER FUNCTION " +
                GetFragmentText(
                    functionStatement.Name));

            FormatFunctionParameters(
                writer,
                functionStatement.Parameters,
                indentLevel + 1);

            writer.WriteLine(
    indentLevel,
    FormatFunctionReturnType(
        functionStatement.ReturnType,
        indentLevel));

            writer.WriteLine(
                indentLevel,
                "AS");

            if (functionStatement.StatementList != null)
            {
                writer.WriteRaw(
                    FormatStatementList(
                        functionStatement.StatementList.Statements,
                        indentLevel));
            }

            return writer.ToString();
        }*/
        private string FormatFunctionReturnType(
    FunctionReturnType returnType,
    int indentLevel)
        {
            if (returnType is ScalarFunctionReturnType scalarReturnType)
            {
                return
                    "RETURNS " +
                    FormatDataType(
                        scalarReturnType.DataType);
            }

            if (returnType is SelectFunctionReturnType)
            {
                return "RETURNS TABLE";
            }

            if (returnType is TableValuedFunctionReturnType)
            {
                return "RETURNS TABLE";
            }

            return
                "RETURNS " +
                GetFragmentText(returnType);
        }
        private string FormatMergeStatement(
    MergeStatement mergeStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            MergeSpecification specification =
                mergeStatement.MergeSpecification;

            writer.WriteLine(
                indentLevel,
                "MERGE " +
                GetFragmentText(
                    specification.Target));

            writer.WriteLine(
                indentLevel,
                "USING " +
                GetFragmentText(
                    specification.TableReference));

            writer.WriteLine(
                indentLevel,
                "ON " +
                FormatBooleanExpressionInline(
                    specification.SearchCondition));

            foreach (
                MergeActionClause actionClause
                in specification.ActionClauses)
            {
                writer.WriteRaw(
                    FormatMergeActionClause(
                        actionClause,
                        indentLevel));
            }

            string result =
                writer.ToString()
                    .TrimEnd();

            if (!result.EndsWith(";"))
            {
                result += ";";
            }

            return result + Environment.NewLine;
        }
        private string FormatMergeActionClause(
    MergeActionClause actionClause,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteLine(
                indentLevel,
                FormatMergeCondition(
                    actionClause));

            if (actionClause.Action is UpdateMergeAction updateAction)
            {
                FormatMergeUpdateAction(
                    writer,
                    updateAction,
                    indentLevel + 1);

                return writer.ToString();
            }

            if (actionClause.Action is InsertMergeAction insertAction)
            {
                FormatMergeInsertAction(
                    writer,
                    insertAction,
                    indentLevel + 1);

                return writer.ToString();
            }

            if (actionClause.Action is DeleteMergeAction)
            {
                writer.WriteLine(
                    indentLevel + 1,
                    "DELETE");

                return writer.ToString();
            }

            writer.WriteLine(
                indentLevel + 1,
                GetFragmentText(
                    actionClause.Action));

            return writer.ToString();
        }
        private string FormatMergeCondition(
    MergeActionClause actionClause)
        {
            string conditionText;

            if (actionClause.Condition == MergeCondition.Matched)
            {
                conditionText = "WHEN MATCHED";
            }
            else if (actionClause.Condition == MergeCondition.NotMatched)
            {
                conditionText = "WHEN NOT MATCHED";
            }
            else if (actionClause.Condition == MergeCondition.NotMatchedBySource)
            {
                conditionText = "WHEN NOT MATCHED BY SOURCE";
            }
            else
            {
                conditionText =
                    "WHEN " +
                    actionClause.Condition.ToString();
            }

            if (actionClause.SearchCondition != null)
            {
                conditionText +=
                    " AND " +
                    FormatBooleanExpressionInline(
                        actionClause.SearchCondition);
            }

            conditionText += " THEN";

            return conditionText;
        }
        private void FormatMergeUpdateAction(
    SqlFormatWriter writer,
    UpdateMergeAction updateAction,
    int indentLevel)
        {
            writer.WriteLine(
                indentLevel,
                "UPDATE");

            writer.WriteLine(
                indentLevel,
                "SET");

            for (int i = 0;
                i < updateAction.SetClauses.Count;
                i++)
            {
                string setText =
                    GetFragmentText(
                        updateAction.SetClauses[i]);

                if (i == 0)
                {
                    writer.WriteLine(
                        indentLevel + 1,
                        setText);
                }
                else
                {
                    writer.WriteLine(
                        indentLevel + 1,
                        "," + setText);
                }
            }
        }
        private void FormatMergeInsertAction(
    SqlFormatWriter writer,
    InsertMergeAction insertAction,
    int indentLevel)
        {
            writer.WriteLine(
                indentLevel,
                "INSERT");

            if (insertAction.Columns != null &&
                insertAction.Columns.Count > 0)
            {
                writer.WriteLine(
                    indentLevel,
                    "(");

                for (int i = 0;
                    i < insertAction.Columns.Count;
                    i++)
                {
                    string columnText =
                        GetFragmentText(
                            insertAction.Columns[i]);

                    if (i == 0)
                    {
                        writer.WriteLine(
                            indentLevel + 1,
                            columnText);
                    }
                    else
                    {
                        writer.WriteLine(
                            indentLevel + 1,
                            "," + columnText);
                    }
                }

                writer.WriteLine(
                    indentLevel,
                    ")");
            }

            if (insertAction.Source != null)
            {
                writer.WriteLine(
                    indentLevel,
                    "VALUES");

                for (int i = 0;
                    i < insertAction.Source.RowValues.Count;
                    i++)
                {
                    if (i > 0)
                    {
                        writer.WriteLine(
                            indentLevel,
                            ",");
                    }

                    FormatRowValue(
                        writer,
                        insertAction.Source.RowValues[i],
                        indentLevel);
                }
            }
        }
        private string FormatStatement(
    TSqlStatement statement,
    int indentLevel)
        {
            if (statement is MergeStatement mergeStatement)
            {
                return FormatMergeStatement(
                    mergeStatement,
                    indentLevel);
            }
            if (statement is CreateFunctionStatement createFunction)
            {
                return FormatCreateFunctionStatement(
                    createFunction,
                    indentLevel);
            }

            if (statement is AlterFunctionStatement alterFunction)
            {
                return FormatAlterFunctionStatement(
                    alterFunction,
                    indentLevel);
            }

            if (statement is CreateOrAlterFunctionStatement createOrAlterFunction)
            {
                return FormatCreateOrAlterFunctionStatement(
                    createOrAlterFunction,
                    indentLevel);
            }
            if (statement is CreateViewStatement createView)
            {
                return FormatCreateViewStatement(
                    createView,
                    indentLevel);
            }

            if (statement is AlterViewStatement alterView)
            {
                return FormatAlterViewStatement(
                    alterView,
                    indentLevel);
            }

            if (statement is CreateOrAlterViewStatement createOrAlterView)
            {
                return FormatCreateOrAlterViewStatement(
                    createOrAlterView,
                    indentLevel);
            }
            if (statement is UpdateStatement updateStatement)
            {
                return FormatUpdateStatement(
                    updateStatement,
                    indentLevel);
            }
            if (statement is CreateOrAlterProcedureStatement createOrAlterProcedure)
            {
                return FormatCreateOrAlterProcedureStatement(
                    createOrAlterProcedure,
                    indentLevel);
            }
            if (statement is AlterProcedureStatement alterProcedure)
            {
                return FormatAlterProcedureStatement(
                    alterProcedure,
                    indentLevel);
            }
            if (statement is InsertStatement insertStatement)
            {
                return FormatInsertStatement(
                    insertStatement,
                    indentLevel);
            }
            if (statement is DeleteStatement deleteStatement)
            {
                return FormatDeleteStatement(
                    deleteStatement,
                    indentLevel);
            }
            if (statement is CreateProcedureStatement createProcedure)
            {
                return FormatCreateProcedureStatement(
                    createProcedure,
                    indentLevel);
            }
            if (statement is ExecuteStatement executeStatement)
            {
                return FormatExecuteStatement(
                    executeStatement,
                    indentLevel);
            }
            if (statement is BeginTransactionStatement beginTransaction)
            {
                return FormatSimpleStatement(
                    beginTransaction,
                    indentLevel);
            }

            if (statement is CommitTransactionStatement commitTransaction)
            {
                return FormatSimpleStatement(
                    commitTransaction,
                    indentLevel);
            }

            if (statement is RollbackTransactionStatement rollbackTransaction)
            {
                return FormatSimpleStatement(
                    rollbackTransaction,
                    indentLevel);
            }
            if (statement is SelectStatement selectStatement)
            {
                return FormatSelectStatement(selectStatement, indentLevel);
            }

            if (statement is IfStatement ifStatement)
            {
                return FormatIfStatement(
                    ifStatement,
                    indentLevel);
            }

            if (statement is BeginEndBlockStatement block)
            {
                return FormatBeginEndBlock(
                    block,
                    indentLevel);
            }
            if (statement is TryCatchStatement tryCatch)
            {
                return FormatTryCatchStatement(
                    tryCatch,
                    indentLevel);
            }
            if (statement is WhileStatement whileStatement)
            {
                return FormatWhileStatement(
                    whileStatement,
                    indentLevel);
            }
            if (statement is ReturnStatement returnStatement)
            {
                return FormatReturnStatement(
                    returnStatement,
                    indentLevel);
            }
            if (statement is ThrowStatement throwStatement)
            {
                return FormatThrowStatement(
                    throwStatement,
                    indentLevel);
            }

            if (statement is RaiseErrorStatement raiseError)
            {
                return FormatRaiseErrorStatement(
                    raiseError,
                    indentLevel);
            }
            if (statement is PrintStatement printStatement)
            {
                return FormatPrintStatement(
                    printStatement,
                    indentLevel);
            }
            if (statement is DeclareVariableStatement declareStatement)
            {
                return FormatDeclareStatement(
                    declareStatement,
                    indentLevel);
            }
            if (statement is SetVariableStatement setVariableStatement)
            {
                return FormatSetVariableStatement(
                    setVariableStatement,
                    indentLevel);
            }
            if (statement is SetOnOffStatement setOnOffStatement)
            {
                return FormatSimpleStatement(
                    setOnOffStatement,
                    indentLevel);
            }
            return GetFragmentText(statement);
        }
        private string FormatCreateOrAlterViewStatement(
    CreateOrAlterViewStatement viewStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteLine(
                indentLevel,
                "CREATE OR ALTER VIEW " +
                GetFragmentText(
                    viewStatement.SchemaObjectName));

            writer.WriteLine(
                indentLevel,
                "AS");

            if (viewStatement.SelectStatement != null)
            {
                writer.WriteRaw(
                    FormatSelectStatement(
                        viewStatement.SelectStatement,
                        indentLevel));
            }

            return writer.ToString();
        }
        private string FormatStatementList(
    IList<TSqlStatement> statements,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            int transactionDepth =
                0;

            string previousCategory =
                null;

            for (int i = 0; i < statements.Count; i++)
            {
                TSqlStatement statement =
                    statements[i];

                WriteLeadingCommentsBeforeStatement(
                    writer,
                    statement,
                    indentLevel + transactionDepth);

                string currentCategory =
                    GetStatementCategory(statement);

                if (previousCategory != null &&
                    previousCategory != currentCategory)
                {
                    writer.WriteRawLine(string.Empty);
                }

                if (statement is CommitTransactionStatement ||
                    statement is RollbackTransactionStatement)
                {
                    if (transactionDepth > 0)
                    {
                        transactionDepth--;
                    }
                }

                writer.WriteRaw(
                    FormatStatement(
                        statement,
                        indentLevel + transactionDepth));

                if (statement is BeginTransactionStatement)
                {
                    transactionDepth++;
                }

                previousCategory =
                    currentCategory;
            }

            return writer.ToString();
        }
        private string GetStatementCategory(
    TSqlStatement statement)
        {
            if (statement is DeclareVariableStatement)
            {
                return "Declaration";
            }

            if (statement is SetVariableStatement)
            {
                return "Assignment";
            }

            if (statement is IfStatement ||
                statement is WhileStatement)
            {
                return "ControlFlow";
            }

            if (statement is BeginTransactionStatement ||
                statement is CommitTransactionStatement ||
                statement is RollbackTransactionStatement)
            {
                return "Transaction";
            }

            if (statement is InsertStatement ||
                statement is UpdateStatement ||
                statement is DeleteStatement)
            {
                return "Dml";
            }

            if (statement is SelectStatement)
            {
                return "Query";
            }

            if (statement is ExecuteStatement)
            {
                return "Execution";
            }

            if (statement is ThrowStatement ||
                statement is RaiseErrorStatement)
            {
                return "ErrorHandling";
            }

            if (statement is PrintStatement)
            {
                return "Utility";
            }

            if (statement is ReturnStatement)
            {
                return "Return";
            }

            if (statement is TryCatchStatement ||
                statement is BeginEndBlockStatement)
            {
                return "Block";
            }

            return "Other";
        }
        private string FormatSimpleStatement(
    TSqlStatement statement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteLine(
                indentLevel,
                GetFragmentText(statement));

            return writer.ToString();
        }
        private string FormatSetVariableStatement(
    SetVariableStatement setVariableStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteLine(
                indentLevel,
                GetFragmentText(setVariableStatement));

            return writer.ToString();
        }
        private string FormatDeclareStatement(
     DeclareVariableStatement declareStatement,
     int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteLine(
                indentLevel,
                "DECLARE");

            for (int i = 0;
                i < declareStatement.Declarations.Count;
                i++)
            {
                DeclareVariableElement variable =
                    declareStatement.Declarations[i];

                string variableText =
                    variable.VariableName.Value +
                    " " +
                    FormatDataType(
                        variable.DataType);

                if (variable.Value != null)
                {
                    variableText +=
                        " = " +
                        GetFragmentText(
                            variable.Value);
                }

                if (i == 0)
                {
                    writer.WriteLine(
                        indentLevel + 1,
                        variableText);
                }
                else
                {
                    writer.WriteLine(
                        indentLevel + 1,
                        "," + variableText);
                }
            }

            return writer.ToString();
        }
        private string FormatPrintStatement(
    PrintStatement printStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteLine(
                indentLevel,
                GetFragmentText(printStatement));

            return writer.ToString();
        }
        private string FormatRaiseErrorStatement(
    RaiseErrorStatement raiseError,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteLine(
                indentLevel,
                GetFragmentText(
                    raiseError));

            return writer.ToString();
        }
        private string FormatThrowStatement(
    ThrowStatement throwStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            string text =
                GetFragmentText(
                    throwStatement);

            if (!text.TrimStart()
                .StartsWith(";"))
            {
                text =
                    ";" +
                    text;
            }

            writer.WriteLine(
                indentLevel,
                text);

            return writer.ToString();
        }
        private string FormatReturnStatement(
    ReturnStatement returnStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            if (returnStatement.Expression == null)
            {
                writer.WriteLine(
                    indentLevel,
                    "RETURN");

                return writer.ToString();
            }

            IList<FormattedLine> lines =
                FormatExpressionLines(
                    returnStatement.Expression);

            if (lines == null ||
                lines.Count == 0)
            {
                writer.WriteLine(
                    indentLevel,
                    "RETURN " +
                    GetFragmentText(
                        returnStatement.Expression));

                return writer.ToString();
            }

            if (lines.Count == 1)
            {
                writer.WriteLine(
                    indentLevel,
                    "RETURN " +
                    lines[0].Text);

                return writer.ToString();
            }

            writer.WriteLine(
                indentLevel,
                "RETURN");

            for (int i = 0; i < lines.Count; i++)
            {
                writer.WriteLine(
                    indentLevel + 1 + lines[i].RelativeIndent,
                    lines[i].Text);
            }

            return writer.ToString();
        }
        private string FormatWhileStatement(
    WhileStatement whileStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            writer.WriteLine(
                indentLevel,
                "WHILE " +
                GetFragmentText(
                    whileStatement.Predicate));

            if (whileStatement.Statement
                is BeginEndBlockStatement block)
            {
                writer.WriteRaw(
                    FormatBeginEndBlock(
                        block,
                        indentLevel));
            }
            else
            {
                writer.WriteRaw(
                    FormatStatement(
                        whileStatement.Statement,
                        indentLevel + 1));
            }

            return writer.ToString();
        }
        private void FormatWhere(
    SqlFormatWriter writer,
    WhereClause whereClause,
    int indentLevel)
        {
            if (whereClause == null)
            {
                return;
            }

            if (whereClause.SearchCondition is ExistsPredicate existsPredicate)
            {
                writer.WriteLine(
                    indentLevel,
                    "WHERE EXISTS");

                FormatExistsPredicateBody(
                    writer,
                    existsPredicate,
                    indentLevel);

                return;
            }
            if (whereClause.SearchCondition is BooleanNotExpression notExpression &&
    notExpression.Expression is ExistsPredicate notExistsPredicate)
            {
                writer.WriteLine(
                    indentLevel,
                    "WHERE NOT EXISTS");

                FormatExistsPredicateBody(
                    writer,
                    notExistsPredicate,
                    indentLevel);

                return;
            }
            writer.WriteLine(
                indentLevel,
                "WHERE");

            int conditionIndentLevel =
                whereClause.SearchCondition is BooleanParenthesisExpression
                    ? indentLevel
                    : indentLevel + 1;

            FormatBooleanExpression(
                writer,
                whereClause.SearchCondition,
                conditionIndentLevel);
        }
        private string GetComparisonOperatorText(
    BooleanComparisonType comparisonType)
        {
            if (comparisonType ==
                BooleanComparisonType.Equals)
            {
                return "=";
            }

            if (comparisonType ==
                BooleanComparisonType.GreaterThan)
            {
                return ">";
            }

            if (comparisonType ==
                BooleanComparisonType.GreaterThanOrEqualTo)
            {
                return ">=";
            }

            if (comparisonType ==
                BooleanComparisonType.LessThan)
            {
                return "<";
            }

            if (comparisonType ==
                BooleanComparisonType.LessThanOrEqualTo)
            {
                return "<=";
            }

            if (comparisonType ==
                BooleanComparisonType.NotEqualToBrackets)
            {
                return "<>";
            }

            if (comparisonType ==
                BooleanComparisonType.NotEqualToExclamation)
            {
                return "!=";
            }

            return comparisonType.ToString();
        }
        private string FormatComparisonExpression(
    BooleanComparisonExpression comparison)
        {
            string leftText =
                GetFragmentText(comparison.FirstExpression);

            string rightText =
                GetFragmentText(comparison.SecondExpression);

            string operatorText =
                GetComparisonOperatorText(
                    comparison.ComparisonType);

            return
                leftText +
                " " +
                operatorText +
                " " +
                rightText;
        }

        private void FormatBooleanExpression(
            SqlFormatWriter writer,
            BooleanExpression expression,
            int indentLevel)
        {
            if (expression is BooleanBinaryExpression binary)
            {
                FormatBooleanExpression(
                    writer,
                    binary.FirstExpression,
                    indentLevel);

                string op =
                    binary.BinaryExpressionType ==
                    BooleanBinaryExpressionType.And
                        ? "AND"
                        : "OR";

                if (binary.SecondExpression is BooleanParenthesisExpression)
                {
                    writer.WriteLine(indentLevel, op);

                    FormatBooleanExpression(
                        writer,
                        binary.SecondExpression,
                        indentLevel);
                }
                else
                {
                    writer.WriteLine(
    indentLevel,
    op + " " +
    FormatBooleanExpressionInline(
        binary.SecondExpression) +
    GetInlineComment(
        binary.SecondExpression));
                }

                return;
            }

            if (expression is BooleanParenthesisExpression parenthesis)
            {
                writer.WriteLine(indentLevel, "(");

                FormatBooleanExpression(
                    writer,
                    parenthesis.Expression,
                    indentLevel + 1);

                writer.WriteLine(indentLevel, ")");

                return;
            }

            WriteBooleanLeaf(
                writer,
                expression,
                indentLevel);
        }


        private void WriteBooleanLeaf(
    SqlFormatWriter writer,
    BooleanExpression expression,
    int indentLevel)
        {
            if (expression is ExistsPredicate existsPredicate)
            {
                FormatExistsPredicate(
                    writer,
                    existsPredicate,
                    indentLevel);

                return;
            }

            if (expression is InPredicate inPredicate &&
                inPredicate.Subquery != null)
            {
                FormatInSubqueryPredicate(
                    writer,
                    inPredicate,
                    indentLevel);

                return;
            }

            if (expression is BooleanComparisonExpression comparison)
            {
                writer.WriteLine(
                    indentLevel,
                    FormatComparisonExpression(comparison) +
                    GetInlineComment(expression));

                return;
            }

            writer.WriteLine(
                indentLevel,
                GetFragmentText(expression) +
                GetInlineComment(expression));
        }
        private void FormatInSubqueryPredicate(
    SqlFormatWriter writer,
    InPredicate inPredicate,
    int indentLevel)
        {
            string expressionText =
                GetFragmentText(
                    inPredicate.Expression);

            string keyword =
                inPredicate.NotDefined
                    ? " NOT IN"
                    : " IN";

            writer.WriteLine(
                indentLevel,
                expressionText +
                keyword);

            writer.WriteLine(
                indentLevel,
                "(");

            if (inPredicate.Subquery.QueryExpression != null)
            {
                writer.WriteRaw(
                    FormatQueryExpression(
                        inPredicate.Subquery.QueryExpression,
                        indentLevel + 1));
            }
            else
            {
                writer.WriteLine(
                    indentLevel + 1,
                    GetFragmentText(
                        inPredicate.Subquery));
            }

            writer.WriteLine(
                indentLevel,
                ")");
        }

        private void FormatJoinCondition(
    SqlFormatWriter writer,
    BooleanExpression expression,
    int indentLevel,
    bool isFirstCondition)
        {
            string indent =
                writer.Indent(indentLevel);

            if (expression is BooleanBinaryExpression binary)
            {
                FormatJoinCondition(
                    writer,
                    binary.FirstExpression,
                    indentLevel,
                    isFirstCondition);

                string op =
                    binary.BinaryExpressionType ==
                    BooleanBinaryExpressionType.And
                        ? "AND"
                        : "OR";

                if (binary.SecondExpression
                    is BooleanParenthesisExpression)
                {
                    writer.WriteRawLine(
                        indent + op);

                    FormatJoinCondition(
                        writer,
                        binary.SecondExpression,
                        indentLevel,
                        true);
                }
                else
                {
                    writer.WriteRawLine(
                        indent +
                        op +
                        " " +
                        FormatBooleanExpressionInline(
                            binary.SecondExpression));
                }

                return;
            }

            if (expression is BooleanParenthesisExpression parenthesis)
            {
                writer.WriteRawLine(
                    indent + "(");

                FormatJoinCondition(
                    writer,
                    parenthesis.Expression,
                    indentLevel + 1,
                    true);

                writer.WriteRawLine(
                    indent + ")");

                return;
            }

            string text =
                FormatBooleanExpressionInline(
                    expression);

            if (isFirstCondition)
            {
                writer.WriteRawLine(
                    indent +
                    "ON " +
                    text);
            }
            else
            {
                writer.WriteRawLine(
                    indent +
                    text);
            }
        }

        private string FormatIsNullExpression(
    BooleanIsNullExpression expression)
        {
            string expressionText =
                GetFragmentText(
                    expression.Expression);

            if (expression.IsNot)
            {
                return
                    expressionText +
                    " IS NOT NULL";
            }

            return
                expressionText +
                " IS NULL";
        }
        private string FormatLikePredicate(
    LikePredicate expression)
        {
            string firstExpression =
                GetFragmentText(
                    expression.FirstExpression);

            string secondExpression =
                GetFragmentText(
                    expression.SecondExpression);

            if (expression.NotDefined)
            {
                return
                    firstExpression +
                    " NOT LIKE " +
                    secondExpression;
            }

            return
                firstExpression +
                " LIKE " +
                secondExpression;
        }

        private string FormatBooleanExpressionInline(
    BooleanExpression expression)
        {
            return _booleanExpressionFormatter.FormatInline(
                expression);
        }
        private void FormatExistsPredicateBody(
    SqlFormatWriter writer,
    ExistsPredicate expression,
    int indentLevel)
        {
            writer.WriteLine(
                indentLevel,
                "(");

            if (expression.Subquery.QueryExpression != null)
            {
                writer.WriteRaw(
                    FormatQueryExpression(
                        expression.Subquery.QueryExpression,
                        indentLevel + 1));
            }
            else
            {
                writer.WriteLine(
                    indentLevel + 1,
                    GetFragmentText(
                        expression.Subquery));
            }

            writer.WriteLine(
                indentLevel,
                ")");
        }
        private void FormatExistsPredicate(
    SqlFormatWriter writer,
    ExistsPredicate expression,
    int indentLevel)
        {
            writer.WriteLine(
                indentLevel,
                "EXISTS");

            FormatExistsPredicateBody(
                writer,
                expression,
                indentLevel);
        }
        private string FormatInPredicate(
    InPredicate expression)
        {
            string expressionText =
                GetFragmentText(
                    expression.Expression);

            string valuesText =
                string.Join(
                    ", ",
                    expression.Values
                        .Select(
                            GetFragmentText));

            string inValues =
                "(" +
                valuesText +
                ")";

            if (expression.NotDefined)
            {
                return
                    expressionText +
                    " NOT IN " +
                    inValues;
            }

            return
                expressionText +
                " IN " +
                inValues;
        }
        private string FormatBetweenExpression(
    BooleanTernaryExpression expression)
        {
            string firstExpression =
                GetFragmentText(
                    expression.FirstExpression);

            string secondExpression =
                GetFragmentText(
                    expression.SecondExpression);

            string thirdExpression =
                GetFragmentText(
                    expression.ThirdExpression);

            string betweenKeyword =
                expression.TernaryExpressionType ==
                BooleanTernaryExpressionType.NotBetween
                    ? " NOT BETWEEN "
                    : " BETWEEN ";

            return
                firstExpression +
                betweenKeyword +
                secondExpression +
                " AND " +
                thirdExpression;
        }
        private void FormatGroupBy(
    SqlFormatWriter writer,
    GroupByClause groupByClause)
        {
            if (groupByClause == null ||
                groupByClause.GroupingSpecifications.Count == 0)
            {
                return;
            }

            writer.WriteLine("GROUP BY");

            for (int i = 0; i < groupByClause.GroupingSpecifications.Count; i++)
            {
                IList<FormattedLine> lines =
                    FormatGroupingSpecificationLines(
                        groupByClause.GroupingSpecifications[i]);

                WriteClauseItemLines(
                    writer,
                    lines,
                    i == 0);
            }
        }
        private void WriteClauseItemLines(
    SqlFormatWriter writer,
    IList<FormattedLine> lines,
    bool isFirstItem)
        {
            if (lines == null ||
                lines.Count == 0)
            {
                return;
            }

            bool isMultiline =
                lines.Count > 1;

            if (!isFirstItem &&
                isMultiline)
            {
                writer.WriteLine(1, ",");
            }

            for (int i = 0; i < lines.Count; i++)
            {
                FormattedLine line =
                    lines[i];

                string prefix =
                    string.Empty;

                if (!isFirstItem &&
                    !isMultiline &&
                    i == 0)
                {
                    prefix = ",";
                }

                writer.WriteLine(
                    1 + line.RelativeIndent,
                    prefix + line.Text);
            }
        }
        private IList<FormattedLine> FormatGroupingSpecificationLines(
    GroupingSpecification groupingSpecification)
        {
            if (groupingSpecification is ExpressionGroupingSpecification expressionGrouping)
            {
                return FormatExpressionLines(
                    expressionGrouping.Expression);
            }

            return new List<FormattedLine>
    {
        new FormattedLine
        {
            RelativeIndent = 0,
            Text = GetFragmentText(groupingSpecification)
        }
    };
        }

        private void FormatHaving(
            SqlFormatWriter writer,
            HavingClause havingClause)
        {
            if (havingClause == null)
            {
                return;
            }

            writer.WriteLine("HAVING");

            FormatBooleanExpression(
                writer,
                havingClause.SearchCondition,
                1);
        }
        private void FormatOrderBy(
    SqlFormatWriter writer,
    OrderByClause orderByClause)
        {
            if (orderByClause == null ||
                orderByClause.OrderByElements.Count == 0)
            {
                return;
            }

            writer.WriteLine("ORDER BY");

            for (int i = 0;
                i < orderByClause.OrderByElements.Count;
                i++)
            {
                IList<FormattedLine> lines =
                    FormatOrderByElementLines(
                        orderByClause.OrderByElements[i]);

                WriteClauseItemLines(
                    writer,
                    lines,
                    i == 0);
            }
        }
        private string FormatIfPredicate(
    BooleanExpression predicate,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            if (predicate is ExistsPredicate existsPredicate)
            {
                writer.WriteLine(
                    indentLevel,
                    "IF EXISTS");

                FormatExistsPredicateBody(
                    writer,
                    existsPredicate,
                    indentLevel);

                return writer.ToString();
            }

            if (predicate is BooleanNotExpression notExpression &&
                notExpression.Expression is ExistsPredicate notExistsPredicate)
            {
                writer.WriteLine(
                    indentLevel,
                    "IF NOT EXISTS");

                FormatExistsPredicateBody(
                    writer,
                    notExistsPredicate,
                    indentLevel);

                return writer.ToString();
            }

            writer.WriteLine(
                indentLevel,
                "IF");

            writer.WriteLine(
                indentLevel,
                "(");

            BooleanExpression expression =
    predicate;

            if (predicate is BooleanParenthesisExpression parenthesis)
            {
                expression =
                    parenthesis.Expression;
            }

            FormatBooleanExpression(
                writer,
                expression,
                indentLevel + 1);

            writer.WriteLine(
                indentLevel,
                ")");

            return writer.ToString();
        }
        private IList<FormattedLine> FormatSetClauseLines(
    SetClause setClause)
        {
            if (setClause is AssignmentSetClause assignment)
            {
                string leftSide =
                    GetFragmentText(
                        assignment.Column);

                IList<FormattedLine> valueLines =
                    FormatExpressionLines(
                        assignment.NewValue);

                if (valueLines == null ||
                    valueLines.Count == 0)
                {
                    return new List<FormattedLine>
            {
                new FormattedLine
                {
                    RelativeIndent = 0,
                    Text =
                        GetFragmentText(setClause) +
                        GetInlineComment(setClause)
                }
            };
                }

                if (valueLines.Count == 1)
                {
                    valueLines[0].Text =
                        leftSide +
                        " = " +
                        valueLines[0].Text +
                        GetInlineComment(setClause);

                    return valueLines;
                }

                valueLines.Insert(
                    0,
                    new FormattedLine
                    {
                        RelativeIndent = 0,
                        Text =
                            leftSide +
                            " ="
                    });

                for (int i = 1;
                     i < valueLines.Count;
                     i++)
                {
                    valueLines[i].RelativeIndent++;
                }

                valueLines[valueLines.Count - 1].Text +=
                    GetInlineComment(setClause);

                return valueLines;
            }

            return new List<FormattedLine>
    {
        new FormattedLine
        {
            RelativeIndent = 0,
            Text =
                GetFragmentText(setClause) +
                GetInlineComment(setClause)
        }
    };
        }
        private IList<FormattedLine> FormatOrderByElementLines(
   ExpressionWithSortOrder orderBy)
        {
            IList<FormattedLine> lines =
                FormatExpressionLines(
                    orderBy.Expression);

            string suffix =
                orderBy.SortOrder ==
                Microsoft.SqlServer.TransactSql.ScriptDom.SortOrder.Descending
                    ? " DESC"
                    : string.Empty;

            if (lines.Count > 0)
            {
                lines[
                    lines.Count - 1]
                    .Text += suffix;
            }

            return lines;
        }

        private string GetJoinKeyword(
            QualifiedJoinType joinType)
        {
            if (joinType == QualifiedJoinType.Inner)
            {
                return "INNER JOIN";
            }

            if (joinType == QualifiedJoinType.LeftOuter)
            {
                return "LEFT JOIN";
            }

            if (joinType == QualifiedJoinType.RightOuter)
            {
                return "RIGHT JOIN";
            }

            if (joinType == QualifiedJoinType.FullOuter)
            {
                return "FULL JOIN";
            }

            return "JOIN";
        }

        private string GetFragmentText(
            TSqlFragment fragment)
        {
            var generator = new Sql160ScriptGenerator();

            string text;
            generator.GenerateScript(fragment, out text);

            return text.Trim();
        }
        private string FormatFunctionStatement(
    string functionKeyword,
    FunctionStatementBody functionStatement,
    int indentLevel)
        {
            var writer =
                new SqlFormatWriter();

            string functionHeader =
    functionKeyword +
    " " +
    GetFragmentText(
        functionStatement.Name);

            if (functionStatement.Parameters == null ||
                functionStatement.Parameters.Count == 0)
            {
                writer.WriteLine(
                    indentLevel,
                    functionHeader + "()");
            }
            else
            {
                writer.WriteLine(
                    indentLevel,
                    functionHeader);

                FormatFunctionParameters(
                    writer,
                    functionStatement.Parameters,
                    indentLevel + 1);
            }

            writer.WriteLine(
                indentLevel,
                FormatFunctionReturnType(
                    functionStatement.ReturnType,
                    indentLevel));

            writer.WriteLine(
                indentLevel,
                "AS");

            if (functionStatement.ReturnType
                is SelectFunctionReturnType selectReturnType)
            {
                writer.WriteLine(
                    indentLevel,
                    "RETURN");

                writer.WriteLine(
                    indentLevel,
                    "(");

                string formattedQuery =
                    FormatQueryExpression(
                        selectReturnType.SelectStatement.QueryExpression,
                        0);

                WriteIndentedFormattedQuery(
                    writer,
                    formattedQuery,
                    writer.Indent(
                        indentLevel + 1));

                writer.WriteLine(
                    indentLevel,
                    ")");
            }
            else if (functionStatement.StatementList != null)
            {
                writer.WriteRaw(
                    FormatStatementList(
                        functionStatement.StatementList.Statements,
                        indentLevel));
            }

            return writer.ToString();
        }
        private void WriteIndentedFormattedQuery(
            SqlFormatWriter writer,
            string formattedQuery,
            string prefix)
        {
            string[] lines =
                formattedQuery
                    .Replace("\r\n", "\n")
                    .Split('\n');

            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    writer.WriteRawLine(prefix + line);
                }
            }
        }
    }
}
