using CC_Lab3.LexerDir;
using CC_Lab3.TreeDir;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CC_Lab3.ParserDir
{
    public class Parser
    {
        private static Regex _identificatorRegex = new("^[a-z]+$");
        private static Regex _constantRegex = new("^[0-9]+$");
        private const string _eps = "Eps";
        public Tree Parse(Lexer lexer)
        {
            var programNode = ParseProgram(lexer);

            return new Tree(programNode);
        }

        private TreeNode ParseProgram(Lexer lexer)
        {
            var childNode = ParseBlock(lexer);

            return new TreeNode("<program>", childNode.Attribute, new[] { childNode });
        }

        private TreeNode ParseBlock(Lexer lexer)
        {
            var childNodes = new TreeNode[3];

            if (lexer.NextToken != "{")
                throw new ArgumentException($"Ошибка во время парсинга символа на позиции {lexer.NextTokenPosition}");

            lexer.MoveNext();
            childNodes[0] = new TreeNode("{", "{");


            childNodes[1] = ParseOperatorsList(lexer);

            if (lexer.NextToken != "}")
                throw new ArgumentException($"Ошибка во время парсинга символа на позиции {lexer.NextTokenPosition}");

            lexer.MoveNext();
            childNodes[2] = new TreeNode("}", "}");

            return new TreeNode("<block>", $"{childNodes[0].Attribute} {childNodes[1].Attribute} {childNodes[2].Attribute}", childNodes);
        }

        private TreeNode ParseOperatorsList(Lexer lexer)
        {
            var childNodes = new TreeNode[2];

            childNodes[0] = ParseOperator(lexer);
            childNodes[1] = ParseTail(lexer);

            return new TreeNode("<operators list>", $"{childNodes[0].Attribute} {childNodes[1].Attribute}", childNodes);
        }

        private TreeNode ParseTail(Lexer lexer)
        {
            if (lexer.NextToken != ";")
                throw new ArgumentException($"Ошибка во время парсинга символа на позиции {lexer.NextTokenPosition}");

            var pointDotNode = new TreeNode(";", ";");
            lexer.MoveNext();

            var TailNode = ParseTailRecursion(lexer);

            return new TreeNode("<tail>",$"{pointDotNode.Attribute} {TailNode.Attribute}", new[] { pointDotNode, TailNode });
        }

        private TreeNode ParseTailRecursion(Lexer lexer)
        {
            TreeNode[] childNodes;

            if (IsIdentificator(lexer.NextToken) || lexer.NextToken == "{")
            {
                var operatorNode = ParseOperator(lexer);
                var tailNode = ParseTail(lexer);

                childNodes = new[] { operatorNode, tailNode };
            }
            else
            {
                childNodes = new[] { new TreeNode(_eps, "") };
            }

            return new TreeNode("<tail>", string.Join(" ", childNodes.Select(x => x.Attribute)), childNodes);
        }

        private TreeNode ParseOperator(Lexer lexer)
        {
            TreeNode[] childNodes;

            var next = lexer.NextToken;

            if (IsIdentificator(next) && IsNotReserved(next))
            {
                var identificatorNode = ParseIdentificator(lexer);

                if (lexer.NextToken != "=")
                    throw new ArgumentException($"Ошибка во время парсинга символа на позиции {lexer.NextTokenPosition}");

                var equalsNode = new TreeNode("=", "=");
                lexer.MoveNext();

                var expressionNode = ParseExpression(lexer);

                childNodes = new[] { identificatorNode, equalsNode, expressionNode };
            }
            else
            {
                childNodes = new[] { ParseBlock(lexer) };
            }

            return new TreeNode("<operator>", string.Join(" ", childNodes.Select(x => x.Attribute)), childNodes);
        }
        private bool IsIdentificator(string text)
        {
            return _identificatorRegex.IsMatch(text);
        }
        private TreeNode ParseIdentificator(Lexer lexer)
        {
            var identificator = lexer.NextToken;

            lexer.MoveNext();

            return new TreeNode("<id>", identificator, new[] { new TreeNode(identificator, identificator) });
        }

        private TreeNode ParseExpression(Lexer lexer)
        {
            List<TreeNode> childNodes = new List<TreeNode>();
            childNodes.Add(ParseSimpleExpression(lexer));
            string attributes = childNodes[0].Attribute;
            if (IsRelationOperation(lexer.NextToken))
            {
                var relation = ParseRelationOperation(lexer);
                var rightSimpleExpression = ParseSimpleExpression(lexer);
                attributes = $"{childNodes[0].Attribute} {rightSimpleExpression.Attribute} {relation.Attribute}";
                childNodes.Add(relation);
                childNodes.Add(rightSimpleExpression);
            }
            return new TreeNode("<expression>", attributes, childNodes.ToArray());
        }

        private bool IsRelationOperation(string text)
        {
            string[] signs = new[] { "==", "<>", "<", ">", "<=", ">=" };

            return signs.Contains(text);
        }

        private TreeNode ParseRelationOperation(Lexer lexer)
        {
            var sign = lexer.NextToken;

            lexer.MoveNext();

            return new TreeNode("<Relation operation>", sign, new[] { new TreeNode(sign, sign) });
        }

        private TreeNode ParseSimpleExpression(Lexer lexer)
        {
            TreeNode[] childNodes;
            string attributes;
            if (IsSign(lexer.NextToken))
            {
                var signNode = ParseSign(lexer);
                var termNode = ParseTerm(lexer);
                var simpleExpressionRecursionNode = ParseSimpleExpressionRecursion(lexer);
                childNodes = new[] { signNode, termNode, simpleExpressionRecursionNode };
                attributes = $"{termNode.Attribute} {simpleExpressionRecursionNode.Attribute} {signNode.Attribute}";
            }
            else
            {
                var term = ParseTerm(lexer);
                var simpleExpression = ParseSimpleExpressionRecursion(lexer);
                childNodes = new[] { term, simpleExpression };
                attributes = $"{term.Attribute} {simpleExpression.Attribute}";
            }
            return new TreeNode("<simple expression>", attributes, childNodes);
        }

        private bool IsSign(string text)
        {
            string[] signs = new[] { "+", "-" };

            return signs.Contains(text);
        }

        private TreeNode ParseSign(Lexer lexer)
        {
            var sign = lexer.NextToken;

            lexer.MoveNext();

            return new TreeNode("<Sign>", sign, new[] { new TreeNode(sign, sign) });
        }

        private TreeNode ParseTerm(Lexer lexer)
        {
            var factorNode = ParseFactor(lexer);
            var termRecurtionNode = ParseTermRecursion(lexer);

            return new TreeNode("<term>", $"{factorNode.Attribute} {termRecurtionNode.Attribute}", new[] { factorNode, termRecurtionNode });
        }

        private TreeNode ParseSimpleExpressionRecursion(Lexer lexer)
        {
            TreeNode[] childNodes;
            string attributes;

            if (IsSumOperation(lexer.NextToken))
            {
                var sumOperationNode = ParseSumOperation(lexer);
                var termNode = ParseTerm(lexer);
                var simpleExpressionRecursion = ParseSimpleExpressionRecursion(lexer);
                attributes = $"{termNode.Attribute} {simpleExpressionRecursion.Attribute} {sumOperationNode.Attribute}";
                childNodes = new[] { sumOperationNode, termNode, simpleExpressionRecursion };
            }
            else
            {
                childNodes = new[] { new TreeNode(_eps, "") };
                attributes = "";
            }

            return new TreeNode("<simple expression>", attributes, childNodes);
        }

        private bool IsSumOperation(string text)
        {
            string[] signs = new[] { "+", "-", "or" };

            return signs.Contains(text);
        }

        private TreeNode ParseSumOperation(Lexer lexer)
        {
            var sign = lexer.NextToken;

            lexer.MoveNext();

            return new TreeNode("<sum operation>", sign, new[] { new TreeNode(sign, sign) });
        }

        private TreeNode ParseFactor(Lexer lexer)
        {
            TreeNode[] childNodes;
            string attributes;
            if (IsIdentificator(lexer.NextToken) && IsNotReserved(lexer.NextToken))
            {
                var identificator = ParseIdentificator(lexer);
                childNodes = new[] { identificator };
                attributes = identificator.Attribute;
            }
            else if (IsConstant(lexer.NextToken))
            {
                var constant = ParseConstant(lexer);
                childNodes = new[] { constant };
                attributes = constant.Attribute;
            }
            else if (lexer.NextToken == "not")
            {
                var not = ParseNot(lexer);
                var factorRecursion = ParseFactor(lexer);
                childNodes = new[] {not, factorRecursion};
                attributes = $"{factorRecursion.Attribute} {not.Attribute}";
            }
            else
            {
                throw new ArgumentException($"Ошибка во время парсинга символа на позиции {lexer.NextTokenPosition}");
            }
            return new TreeNode("<factor>", attributes, childNodes);
        }

        private bool IsConstant(string text)
        {
            return _constantRegex.IsMatch(text);
        }

        private TreeNode ParseConstant(Lexer lexer)
        {
            var constant = lexer.NextToken;

            lexer.MoveNext();

            return new TreeNode("<const>", constant, new[] { new TreeNode(constant, constant) });
        }

        private TreeNode ParseNot(Lexer lexer)
        {
            var not = lexer.NextToken;

            lexer.MoveNext();

            return new TreeNode("<not>", not, new[] { new TreeNode(not, not) });
        }

        private TreeNode ParseTermRecursion(Lexer lexer)
        {
            TreeNode[] childNodes;
            string attributes; 

            if (IsMultiplyOperation(lexer.NextToken))
            {
                var multiplyOperationNode = ParseMultiplyOperation(lexer);
                var factorNode = ParseFactor(lexer);
                var dashTermNode = ParseTermRecursion(lexer);
                attributes = $"{factorNode.Attribute} {dashTermNode.Attribute} {multiplyOperationNode.Attribute}";
                childNodes = new[] { multiplyOperationNode, factorNode, dashTermNode };
            }
            else
            {
                childNodes = new[] { new TreeNode(_eps, "") };
                attributes = "";
            }

            return new TreeNode("<term>", attributes, childNodes);
        }

        private bool IsMultiplyOperation(string text)
        {
            string[] operations = new[] { "*", "/", "div", "mod", "and" };

            return operations.Contains(text);
        }
        private TreeNode ParseMultiplyOperation(Lexer lexer)
        {
            var sign = lexer.NextToken;

            lexer.MoveNext();

            return new TreeNode("<multiply operation>", sign, new[] { new TreeNode(sign, sign) });
        }

        private bool IsNotReserved(string text)
        {
            string[] operations = new[] { "not", "or", "div", "mod", "and" };

            return !operations.Contains(text);
        }
    }
}
