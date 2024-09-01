using Antlr4.Runtime;
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
    public class StructTypeDeclarationVisitor : GoParserBaseVisitor<bool>
    {
        public Dictionary<string, TypeGen> Classes { get; }
        private readonly AssemblyGen _assemblyGen;
        private string _typeName;

        public StructTypeDeclarationVisitor(AssemblyGen assemblyGen)
        {
            Classes = new Dictionary<string, TypeGen>();
            _assemblyGen = assemblyGen;
        }

        public override bool VisitSourceFile([NotNull] SourceFileContext context)
        {
            var className = "Program";

            var classGen = _assemblyGen.Public.Class(className);

            Classes[className] = classGen;

            return base.VisitSourceFile(context);
        }

        public override bool VisitTypeDef(TypeDefContext context)
        {
            _typeName = context.IDENTIFIER().Symbol.Text;
            return base.VisitTypeDef(context);
        }

        public override bool VisitStructType(StructTypeContext context)
        {
            var className = _typeName;

            var classGen = _assemblyGen.Public.Class(className);

            Classes[className] = classGen;

            return base.VisitStructType(context);
        }
    }
}
