using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using MathParserTK;


namespace ZR_MasterTools
{
    public class ExpressionHelper
    {
        /// <summary>
        /// 传入表达式，输出计算结果
        /// </summary>
        public static double Evaluate(string expression, DivisionType divisionType = DivisionType.Float) {
            /*
            DataTable loDataTable = new DataTable();
            DataColumn loDataColumn = new DataColumn("Eval", typeof(double), expression);
            loDataTable.Columns.Add(loDataColumn);
            loDataTable.Rows.Add(0);
            return (double)(loDataTable.Rows[0]["Eval"]);
            */
            MathParser parser = new MathParser();
            return parser.Parse(expression, true, divisionType);
        }


        /// <summary>
        /// 传入带表达式的文本，如“伤害提高[40*100+1200]点”，返回带计算结果的文本，如“伤害提高5200点”
        /// </summary>
        public static string GetTextAfterCalculateExpression(string textWithExpression) {
            string newText = string.Empty;
            List<string> expressions = GetExpressionFromText(textWithExpression, out newText);
            string[] results = GetExpressionResults(expressions);
            newText = string.Format(newText, results);
            return newText;
        }

        // 把文本中的表达式部分替换为占位符，如“伤害提高[40*100+1200]点，持续[1*2]秒” 替换为“伤害提高{0}点，持续{1}秒”，并返回提取出来的表达式数组
        static List<string> GetExpressionFromText(string textWithExpression, out string textAfterReplaceExpression) {
            textAfterReplaceExpression = string.Empty;
            List<string> expressions = new List<string>();
            int expressionCount = 0;
            for (int i = 0; i < textWithExpression.Length; i++) {
                if (textWithExpression[i] == '[') {
                    string expression = string.Empty;
                    while (textWithExpression[++i] != ']') {
                        expression += textWithExpression[i];
                    }
                    expressions.Add(expression);
                    textAfterReplaceExpression += "{" + expressionCount + "}";
                    expressionCount++;
                    continue;
                }
                textAfterReplaceExpression += textWithExpression[i];
            }
            return expressions;
        }

        // 依次计算数组中的表达式并返回对应结果
        static string[] GetExpressionResults(List<string> expressions) {
            string[] results = new string[expressions.Count];
            for (int i = 0; i < expressions.Count; i++) {
                results[i] = Evaluate(expressions[i]).ToString();
            }
            return results;
        }
    }
}