using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptFormatter.Core.Formatting.Custom.Expressions
{
    public class BooleanExpressionFormatter
    {
        private readonly Sql160ScriptGenerator _generator;

        public BooleanExpressionFormatter()
        {
            _generator = new Sql160ScriptGenerator();
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
        public string FormatInline(
	BooleanExpression expression)
{
	if (expression == null)
	{
		return string.Empty;
	}

	if (expression is BooleanComparisonExpression comparison)
	{
		return FormatComparisonExpression(
			comparison);
	}

	if (expression is BooleanIsNullExpression isNull)
	{
		return FormatIsNullExpression(
			isNull);
	}
    if (expression is LikePredicate like)
{
	return FormatLikePredicate(
		like);
}

	return GetFragmentText(
		expression);
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

        private string GetComparisonOperatorText(
            BooleanComparisonType comparisonType)
        {
            if (comparisonType == BooleanComparisonType.Equals)
            {
                return "=";
            }

            if (comparisonType == BooleanComparisonType.GreaterThan)
            {
                return ">";
            }

            if (comparisonType == BooleanComparisonType.GreaterThanOrEqualTo)
            {
                return ">=";
            }

            if (comparisonType == BooleanComparisonType.LessThan)
            {
                return "<";
            }

            if (comparisonType == BooleanComparisonType.LessThanOrEqualTo)
            {
                return "<=";
            }

            if (comparisonType == BooleanComparisonType.NotEqualToBrackets)
            {
                return "<>";
            }

            if (comparisonType == BooleanComparisonType.NotEqualToExclamation)
            {
                return "!=";
            }

            return comparisonType.ToString();
        }

        private string GetFragmentText(
            TSqlFragment fragment)
        {
            string text;

            _generator.GenerateScript(
                fragment,
                out text);

            return text.Trim();
        }
    }
}
