using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriAxis.RunSharp;
using static GoParser;

namespace CC_Course.CodeGeneration
{
    public class ClassAndMethodImplementationVisitor : GoParserBaseVisitor<bool>
    {
        private const string PRINT = "Print";
        private const string READ = "Read";
        private const string INTPARSE = "IntParse";
        private readonly AssemblyGen _assemblyGen;
        private readonly Dictionary<string, TypeGen> _classes;
        private readonly Dictionary<string, Dictionary<string, MethodGen>> _classesMethods;
        private string _currentClassName = null;
        private string _currentMethodName = null;
        private string _currentTypeName = null;
        private CodeGen _currentCodeGen;
        private Operand _currentOperand;
        private Dictionary<string, Operand> _currentLocalVariables;
        private Dictionary<string,Dictionary<string, Operand>> _classFields;
        public ClassAndMethodImplementationVisitor(
            Dictionary<string, TypeGen> classes,
            Dictionary<string, Dictionary<string, MethodGen>> classesMethods,
            AssemblyGen assemblyGen)
        {
            _classes = classes;
            _classesMethods = classesMethods;
            _assemblyGen = assemblyGen;
        }
       
        public override bool VisitTypeDef([NotNull] TypeDefContext context)
        {
            _currentTypeName = context.IDENTIFIER().Symbol.Text;
            base.VisitTypeDef(context);
            _currentTypeName = null;
            return true;
        }

        public override bool VisitSourceFile([NotNull] SourceFileContext context)
        {
            _classFields = new Dictionary<string, Dictionary<string, Operand>>();
            _classFields["Program"] = new Dictionary<string, Operand>();
            return base.VisitSourceFile(context);
        }

        public override bool VisitStructType([NotNull] StructTypeContext context)
        {
            _currentClassName = _currentTypeName;
            _classFields[_currentClassName] = new Dictionary<string, Operand>();

            base.VisitStructType(context);

            _currentClassName = null;

            return true;
        }

        public override bool VisitFieldDecl([NotNull] FieldDeclContext context)
        {
            var currentClass = _classes[_currentClassName];

            var fieldName = context.identifierList().IDENTIFIER().First().Symbol.Text;
            var fieldType = GetTypeForField(context.type_());
            Operand def;
            if(fieldType == typeof(int))
            {
                def = 0;
            }
            else if(fieldType == typeof(string))
            {
                def = "";
            }
            else 
            {
                _classFields[_currentClassName][fieldName] = currentClass.Public.Field(fieldType, fieldName);

                return base.VisitFieldDecl(context);
            }

            _classFields[_currentClassName][fieldName] = currentClass.Public.Field(fieldType, fieldName, def);

            return base.VisitFieldDecl(context);
        }

        public override bool VisitMethodDecl([NotNull] MethodDeclContext context)
        {
            _currentClassName = context.receiver().parameters().parameterDecl().First().type_().typeLit().pointerType().type_().typeName().IDENTIFIER().Symbol.Text;
            _currentMethodName = context.IDENTIFIER().Symbol.Text;
            _currentCodeGen = _classesMethods[_currentClassName][_currentMethodName].GetCode();
            _currentLocalVariables = new Dictionary<string, Operand>();
            base.VisitMethodDecl(context);
            _currentClassName = null;
            _currentMethodName = null;
            _currentCodeGen = null;
            return true;
        }

        public override bool VisitFunctionDecl([NotNull] FunctionDeclContext context)
        {
            _currentClassName = "Program";
            _currentMethodName = context.IDENTIFIER().Symbol.Text;
            _currentCodeGen = _classesMethods[_currentClassName][_currentMethodName].GetCode();
            _currentLocalVariables = new Dictionary<string, Operand>();
            if (_currentMethodName == "Main")
            {
                var varName = "prog";
                var className = "Program";
                Type varType = _classes["Program"];
                _currentOperand = _assemblyGen.ExpressionFactory.New(_classes[className]);
                _currentLocalVariables[varName] = _currentCodeGen.Local(varType);
                _classFields[className][_currentMethodName] = _currentLocalVariables[varName];
                _currentCodeGen.Assign(_currentLocalVariables[varName], _currentOperand);
            }
            
            base.VisitFunctionDecl(context);
            _currentClassName = null;
            _currentMethodName = null;
            _currentCodeGen = null;
            return true;
        }


        /*
    statement
    : declaration
    | simpleStmt
    | returnStmt
    | block
    | ifStmt
    | forStmt - forClause
    ;
        */
        public override bool VisitStatement([NotNull] StatementContext context)
        {
            //Console.WriteLine(context.GetText());
            if (context.forStmt() != null)
            {
                Visit(context.forStmt().expression());

                _currentCodeGen.While(_currentOperand);

                Visit(context.forStmt().block());

                _currentCodeGen.End();

                return true;
            }
            else if (context.returnStmt() != null)
            {
                Visit(context.returnStmt().expressionList().expression().First());

                _currentCodeGen.Return(_currentOperand);

                return true;
            }
            else
            {
                return base.VisitChildren(context);
            }
        }

        public override bool VisitIfStmt([NotNull] IfStmtContext context)
        {
            Visit(context.expression());

            _currentCodeGen.If(_currentOperand);

            Visit(context.block()[0]);
            if (context.ELSE() != null) {

                _currentCodeGen.Else();

                Visit(context.block()[1]);
            }

            _currentCodeGen.End();

            return true;
        }

        /*
    constDecl
    : CONST (constSpec | L_PAREN (constSpec eos)* R_PAREN)
    ;

    constSpec
    : identifierList (type_? ASSIGN expressionList)?
    ;
        */
        public override bool VisitConstDecl([NotNull] ConstDeclContext context)
        {
            var constName = context.constSpec().First().identifierList().IDENTIFIER().First().Symbol.Text;
            var constType = GetTypeForField(context.constSpec().First().type_());
            _currentLocalVariables[constName] = _currentCodeGen.Local(constType);
            return base.VisitConstDecl(context);
        }

        public override bool VisitVarDecl([NotNull] VarDeclContext context)
        {
            var varName = context.varSpec().First().identifierList().IDENTIFIER().First().Symbol.Text;
            var varType = GetTypeForField(context.varSpec().First().type_());
            if (varType.IsArray)
            {
                Type type;
                if (varType.IsClass && varType != typeof(int[]) && varType != typeof(string[]))
                {
                    type = _classes[varType.Name.Trim('[', ']').ToString()];
                }
                else
                {
                    type = typeof(int);
                }
                Visit(context.varSpec().First().type_().typeLit().arrayType().arrayLength().expression());

                var arraySize = _currentOperand;
                _currentOperand = _assemblyGen.ExpressionFactory.NewArray(type, arraySize);
                _currentLocalVariables[varName] = _currentCodeGen.Local(varType);
                _currentCodeGen.Assign(_currentLocalVariables[varName], _currentOperand);
                return true;
            }
            else if (varType.IsClass && varType != typeof(string))
            {
                var className = varType.Name.ToString();
                _currentOperand = _assemblyGen.ExpressionFactory.New(_classes[className]);
                _currentLocalVariables[varName] = _currentCodeGen.Local(varType);
                _currentCodeGen.Assign(_currentLocalVariables[varName], _currentOperand);
                return true;
            }
            _currentLocalVariables[varName] = _currentCodeGen.Local(varType);
            return base.VisitVarDecl(context);
        }

        /*
    expression
    : primaryExpr
    | unary_op = (PLUS | MINUS | EXCLAMATION | CARET | STAR | AMPERSAND | RECEIVE) expression
    | expression mul_op = (STAR | DIV | MOD | LSHIFT | RSHIFT | AMPERSAND | BIT_CLEAR) expression
    | expression add_op = (PLUS | MINUS | OR | CARET) expression
    | expression rel_op = (
        EQUALS
        | NOT_EQUALS
        | LESS
        | LESS_OR_EQUALS
        | GREATER
        | GREATER_OR_EQUALS
    ) expression
    | expression LOGICAL_AND expression
    | expression LOGICAL_OR expression
    ;
        */
        public override bool VisitExpression([NotNull] ExpressionContext context)
        {
            if (context.primaryExpr() != null)
            {
                return base.VisitChildren(context);
            }
            if(context.add_op != null)
            {
                if (context.add_op.Text == "+")
                {
                    Visit(context.expression()[0]);
                    var firstExpressionOperand = _currentOperand;

                    Visit(context.expression()[1]);
                    var secondExpressionOperand = _currentOperand;

                    _currentOperand = firstExpressionOperand.Add(secondExpressionOperand);

                    return true;
                }
                if (context.add_op.Text == "-")
                {
                    Visit(context.expression()[0]);
                    var firstExpressionOperand = _currentOperand;

                    Visit(context.expression()[1]);
                    var secondExpressionOperand = _currentOperand;

                    _currentOperand = firstExpressionOperand.Subtract(secondExpressionOperand);

                    return true;
                }
            }
            if(context.mul_op != null)
            {
                if (context.mul_op.Text == "*")
                {
                    Visit(context.expression()[0]);
                    var firstExpressionOperand = _currentOperand;

                    Visit(context.expression()[1]);
                    var secondExpressionOperand = _currentOperand;

                    _currentOperand = firstExpressionOperand.Multiply(secondExpressionOperand);

                    return true;
                }
                if (context.mul_op.Text == "/")
                {
                    Visit(context.expression()[0]);
                    var firstExpressionOperand = _currentOperand;

                    Visit(context.expression()[1]);
                    var secondExpressionOperand = _currentOperand;

                    _currentOperand = firstExpressionOperand.Divide(secondExpressionOperand);

                    return true;
                }
            }
            if(context.rel_op != null)
            {
                if(context.rel_op.Text == "==")
                {
                    Visit(context.expression()[0]);
                    var firstExpressionOperand = _currentOperand;

                    Visit(context.expression()[1]);
                    var secondExpressionOperand = _currentOperand;

                    _currentOperand = firstExpressionOperand.Eq(secondExpressionOperand);

                    return true;
                }
                if (context.rel_op.Text == "!=")
                {
                    Visit(context.expression()[0]);
                    var firstExpressionOperand = _currentOperand;

                    Visit(context.expression()[1]);
                    var secondExpressionOperand = _currentOperand;

                    _currentOperand = firstExpressionOperand.Ne(secondExpressionOperand);

                    return true;
                }
                if (context.rel_op.Text == ">=")
                {
                    Visit(context.expression()[0]);
                    var firstExpressionOperand = _currentOperand;

                    Visit(context.expression()[1]);
                    var secondExpressionOperand = _currentOperand;

                    _currentOperand = firstExpressionOperand.Ge(secondExpressionOperand);

                    return true;
                }
                if (context.rel_op.Text == "<=")
                {
                    Visit(context.expression()[0]);
                    var firstExpressionOperand = _currentOperand;

                    Visit(context.expression()[1]);
                    var secondExpressionOperand = _currentOperand;

                    _currentOperand = firstExpressionOperand.Le(secondExpressionOperand);

                    return true;
                }
                if (context.rel_op.Text == ">")
                {
                    Visit(context.expression()[0]);
                    var firstExpressionOperand = _currentOperand;

                    Visit(context.expression()[1]);
                    var secondExpressionOperand = _currentOperand;

                    _currentOperand = firstExpressionOperand.Gt(secondExpressionOperand);

                    return true;
                }
                if (context.rel_op.Text == "<")
                {
                    Visit(context.expression()[0]);
                    var firstExpressionOperand = _currentOperand;

                    Visit(context.expression()[1]);
                    var secondExpressionOperand = _currentOperand;

                    _currentOperand = firstExpressionOperand.Lt(secondExpressionOperand);

                    return true;
                }
            }
            if(context.LOGICAL_AND() != null)
            {
                Visit(context.expression()[0]);
                var firstExpressionOperand = _currentOperand;

                Visit(context.expression()[1]);
                var secondExpressionOperand = _currentOperand;

                _currentOperand = firstExpressionOperand.LogicalAnd(secondExpressionOperand);

                return true;
            }
            if (context.LOGICAL_OR() != null)
            {
                Visit(context.expression()[0]);
                var firstExpressionOperand = _currentOperand;

                Visit(context.expression()[1]);
                var secondExpressionOperand = _currentOperand;

                _currentOperand = firstExpressionOperand.LogicalOr(secondExpressionOperand);

                return true;
            }
            return base.VisitChildren(context);
        }


        /*
    primaryExpr
    : operand+
    | primaryExpr ( DOT IDENTIFIER | index | slice_ | typeAssertion | arguments)+
    ;
        */

        public override bool VisitOperandName([NotNull] OperandNameContext context)
        {
            var id = context.IDENTIFIER().Symbol.Text;
            _currentOperand = GetOperandByName(id);
            return true;
        }
        public override bool VisitPrimaryExpr([NotNull] PrimaryExprContext context)
        {
            if (context.operand() != null)
            {
                VisitOperand(context.operand());
                return true;
            }
            else if (context.arguments() != null && context.primaryExpr()?.primaryExpr() != null)
            {
                var methodName = context.primaryExpr().IDENTIFIER().Symbol.Text;

                base.Visit(context.primaryExpr().primaryExpr());

                var targetOperand = _currentOperand;

                var argumentOperands = new List<Operand>();

                if (context.arguments().expressionList() != null)
                {
                    foreach (var argumentExpression in context.arguments().expressionList()?.expression())
                    {
                        Visit(argumentExpression);
                        argumentOperands.Add(_currentOperand);

                    }
                }

                _currentOperand = targetOperand.Invoke(methodName, _assemblyGen.TypeMapper, argumentOperands.ToArray());
                return true;
            }
            else if (context.arguments() != null && context.primaryExpr()?.primaryExpr() == null)
            {
                var methodName = context.primaryExpr().operand().operandName().IDENTIFIER().Symbol.Text;
                var argumentOperands = new List<Operand>();
                if (context.arguments().expressionList() != null)
                {
                    foreach (var argumentExpression in context.arguments().expressionList()?.expression())
                    {
                        Visit(argumentExpression);
                        argumentOperands.Add(_currentOperand);

                    }
                }
                if (methodName == PRINT)
                {
                   _currentCodeGen.WriteLine(argumentOperands.ToArray());
                    return true;
                }
                if(methodName == READ)
                {
                    _currentCodeGen.Assign(argumentOperands.First(), Console.ReadLine());
                    return true;
                }
                if(methodName == INTPARSE)
                {
                    _currentCodeGen.Invoke(typeof(int), "Parse", argumentOperands.ToArray());
                    return true;
                }
                else
                {
                    if (_currentMethodName == "Main")
                    {
                        var p = GetOperandByName("prog");
                        _currentOperand = p.Invoke(methodName, _assemblyGen.TypeMapper, argumentOperands.ToArray());
                    }
                    else
                    {
                        _currentOperand = _currentCodeGen.This().Invoke(methodName, argumentOperands.ToArray());
                    }
                    return true;
                }
            }
            else if(context.IDENTIFIER() != null)
            {
                base.VisitChildren(context);
                var targetOperand = _currentOperand;
                var property = context.IDENTIFIER().Symbol.Text;
                _currentOperand = targetOperand.Field(property, _assemblyGen.TypeMapper);
                return true;
                
            }
            if(context.index() != null)
            {
                var targetOperand = GetOperandByName(context.primaryExpr().operand().operandName().IDENTIFIER().Symbol.Text);
                base.VisitChildren(context.index());
                var index = _currentOperand;
                _currentOperand = targetOperand[_currentCodeGen.TypeMapper, index];
                return true;
            }
            return base.VisitChildren(context);
        }
        /*
            operand
            : literal+
            | operandName typeArgs?+
            | L_PAREN expression R_PAREN?
            ;
        */
        public override bool VisitOperand([NotNull] OperandContext context)
        {
            if (context.operandName() != null && context.typeArgs() == null)
            {
                _currentOperand = GetOperandByName(context.operandName().IDENTIFIER().Symbol.Text);
            }
            else if(context.operandName() != null && context.typeArgs() != null)
            {
                var arrayName = GetOperandByName(context.operandName().IDENTIFIER().Symbol.Text);
                var isInt =int.TryParse(context.typeArgs().typeList().type_().First().typeName().IDENTIFIER().Symbol.Text, out var decIndex);
                if (isInt)
                {
                    _currentOperand = arrayName[_currentCodeGen.TypeMapper, decIndex];
                    return true;
                }
                else
                {
                    var index = GetOperandByName(context.typeArgs().typeList().type_().First().typeName().IDENTIFIER().Symbol.Text);
                    _currentOperand = arrayName[_currentCodeGen.TypeMapper, index];
                    return true;
                }
            }
            return base.VisitOperand(context);
        }


        /*
    integer
    : DECIMAL_LIT+
    ;
        */
        public override bool VisitInteger([NotNull] IntegerContext context)
        {
            _currentOperand = int.Parse(context.DECIMAL_LIT().Symbol.Text);
            return base.VisitInteger(context);
        }

        public override bool VisitString_([NotNull] String_Context context)
        {
            var x = context.INTERPRETED_STRING_LIT().Symbol.Text.Trim('\"');
            _currentOperand = context.INTERPRETED_STRING_LIT().Symbol.Text;
            return base.VisitString_(context);
        }


        /*
    basicLit
    : NIL_LIT-
    | integer+
    | string_+
    | FLOAT_LIT-
    ;
        */
        public override bool VisitBasicLit([NotNull] BasicLitContext context)
        {
            if(context.NIL_LIT() != null)
            {
                _currentOperand = Operand.FromObject(null);
                return true;
            }
            return base.VisitBasicLit(context);
        }

        /*
    literal
    : basicLit+
    ;
        */
        public override bool VisitLiteral([NotNull] LiteralContext context)
        {
            return base.VisitLiteral(context);
        }

        /*
    simpleStmt
    | assignment+
    | expressionStmt+
    | shortVarDecl-
    ;
        */
        public override bool VisitSimpleStmt([NotNull] SimpleStmtContext context)
        {
            return base.VisitSimpleStmt(context);
        }

        public override bool VisitChildren(IRuleNode node)
        {
            Console.WriteLine(node.GetText());
            return base.VisitChildren(node);
        }

        public override bool VisitAssignment([NotNull] AssignmentContext context)
        {

            Visit(context.expressionList()[0].expression().First());
            var target = _currentOperand;
            
           
            Visit(context.expressionList()[1].expression().First());

            _currentCodeGen.Assign(target, _currentOperand);

            return true;

        }



        public Type GetTypeForField(Type_Context context)
        {
            Type result = null;
            if (context.typeLit() == null)
            {
                switch (context.typeName().IDENTIFIER().Symbol.Text)
                {
                    case "int": result = typeof(int); break;
                    case "string": result = typeof(string); break;
                    case "bool": result = typeof(bool); break;
                }
            }
            else if (context.typeLit()?.arrayType() != null)
            {
                var n = context.typeLit().arrayType().elementType().type_().typeName().IDENTIFIER().Symbol.Text;
                if (_classes.ContainsKey(n))
                {
                    Type r = _classes[n];
                    result = r.MakeArrayType();
                }
                switch (context.typeLit().arrayType().elementType().type_().typeName().IDENTIFIER().Symbol.Text)
                {
                    case "int": result = typeof(int[]); break;
                    case "string": result = typeof(string[]); break;
                    case "bool": result = typeof(bool[]); break;
                }
            }
            else if (context.typeLit()?.structType() != null)
            {
                var typeDef = context.Parent as TypeDefContext;
                result = _classes[typeDef.IDENTIFIER().Symbol.Text];
            }
            else if (context.typeLit()?.pointerType() != null)
            {
                if (context.typeLit()?.pointerType()?.type_()?.typeLit()?.structType() != null)
                {
                    result = _classes[context.typeName().IDENTIFIER().Symbol.Text];
                }
                else
                {
                    var name = context.typeLit().pointerType().type_().typeName().IDENTIFIER().Symbol.Text;
                    if (_classes.ContainsKey(name))
                    {
                        result = _classes[name];
                    }
                    else
                    {
                        switch (context.typeLit().pointerType().type_().typeName().IDENTIFIER().Symbol.Text)
                        {
                            case "int": result = typeof(int[]); break;
                            case "string": result = typeof(string[]); break;
                            case "bool": result = typeof(bool[]); break;
                            case "*int": result = typeof(int[]); break;
                            case "*string": result = typeof(string[]); break;
                            case "*bool": result = typeof(bool[]); break;
                        }
                    }
                }
            }
            return result;
        }

        private Operand GetOperandByName(string name)
        {
            Operand result = null;

            if (_currentLocalVariables.TryGetValue(name, out var localVariable))
                result = localVariable;
            else if (_currentClassName != null && _currentMethodName != null &&
                     _classesMethods[_currentClassName][_currentMethodName].Parameters.Any(p => p.Name == name))
            {
                result = _currentCodeGen.Arg(name);
            }
            else
                try
                {
                    var c = _currentCodeGen.This();
                    result = _currentCodeGen.This().Field(name);
                }
                catch
                {
                    result = _classFields[_currentClassName][name];
                }

            return result;
        }
    }
}
