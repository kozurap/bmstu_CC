using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriAxis.RunSharp;
using static GoParser;

namespace CC_Course.CodeGeneration
{
    public class ClassesMethodsDeclarationVisitor : GoParserBaseVisitor<bool>
    {
        private Dictionary<string, TypeGen> _classes { get; }
        private readonly AssemblyGen _assemblyGen;
        private string _currentClassName;
        public Dictionary<string, Dictionary<string, MethodGen>> ClassesMethods { get; }

        public ClassesMethodsDeclarationVisitor(AssemblyGen assemblyGen,
            Dictionary<string, TypeGen> classes)
        {
            ClassesMethods = new Dictionary<string, Dictionary<string, MethodGen>>();

            _assemblyGen = assemblyGen;
            _classes = classes;
        }

        public override bool VisitSourceFile(SourceFileContext context)
        {
            ClassesMethods["Program"] = new Dictionary<string, MethodGen>();
            return base.VisitSourceFile(context);
        }

        public override bool VisitTypeDef(TypeDefContext context)
        {
            _currentClassName = context.IDENTIFIER().Symbol.Text;
            return base.VisitTypeDef(context);
        }

        public override bool VisitStructType(StructTypeContext context)
        {
            ClassesMethods[_currentClassName] = new Dictionary<string, MethodGen>();

            return base.VisitStructType(context);
        }

        public override bool VisitMethodDecl([NotNull] MethodDeclContext context)
        {
            var methodName = context.IDENTIFIER().Symbol.Text;
            var className = context.receiver().parameters().parameterDecl().First().type_().typeLit().pointerType().type_().typeName().IDENTIFIER().Symbol.Text;
            var parameters = context.signature().parameters().parameterDecl();
            var returnType = typeof(void);
            if (context.signature().result() != null)
            {
                returnType = GetType(context.signature().result().parameters().parameterDecl().First().type_());
            }
            var currentClass = _classes[className];
            var method = currentClass.Public.Method(returnType, methodName);
            foreach(var parameter in parameters)
            {
                var type = GetType(parameter.type_());
                var parameterName = parameter.identifierList().IDENTIFIER().First().Symbol.Text;
                method.Parameter(type, parameterName);
            }
            ClassesMethods[className][methodName] = method;
            return base.VisitMethodDecl(context);
        }

        public override bool VisitFunctionDecl([NotNull] FunctionDeclContext context)
        {
            var methodName = context.IDENTIFIER().Symbol.Text;
            if (methodName == "Main")
            {
                var currentClass = _classes["Program"];

                var returnType = typeof(void);

                var method = currentClass.Public.Static.Method(returnType, methodName);
                method.Parameter(typeof(string[]), "args");

                ClassesMethods["Program"][methodName] = method;
            }
            else
            {
                var returnType = typeof(void);
                var className = "Program";
                var parameters = context.signature().parameters().parameterDecl();
                if(context.signature().result() != null)
                {
                    returnType = GetType(context.signature().result().parameters().parameterDecl().First().type_());
                }
                var currentClass = _classes[className];
                var method = currentClass.Public.Method(returnType, methodName);
                foreach (var parameter in parameters)
                {
                    var type = GetType(parameter.type_());
                    var parameterName = parameter.identifierList().IDENTIFIER().First().Symbol.Text;
                    method.Parameter(type, parameterName);
                }
                ClassesMethods[className][methodName] = method;
            }
            return base.VisitFunctionDecl(context);
        }

        public Type GetType(Type_Context context)
        {
            Type result = typeof(void);
            if (context.typeLit() == null)
            {
                switch (context.typeName()?.IDENTIFIER().Symbol.Text){
                    case "int": result = typeof(int); break;
                    case "string": result = typeof(string); break;
                    case "bool": result = typeof(bool); break;
                }
            }
            else if (context.typeLit()?.arrayType() != null)
            {
                switch (context.typeLit().arrayType().elementType().type_().typeName().IDENTIFIER().Symbol.Text){
                    case "int": result = typeof(int[]); break;
                    case "string": result = typeof(string[]); break;
                    case "bool": result = typeof(bool[]); break;
                }
            }
            else if (context.typeLit()?.structType() != null)
            {
                result = _classes[context.typeName().IDENTIFIER().Symbol.Text];
            }
            else if (context.typeLit().pointerType() != null)
            {
                if (context.typeLit()?.pointerType()?.type_()?.typeLit()?.structType() != null)
                {
                    result = _classes[context.typeName().IDENTIFIER().Symbol.Text];
                }
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
            return result;
        }
    }
}
