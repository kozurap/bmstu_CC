using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriAxis.RunSharp;

namespace CC_Course.CodeGeneration
{
    public class SharpVisitor : GoParserBaseVisitor<bool>
    {
        public Dictionary<string, TypeGen> Classes { get; }

        private readonly AssemblyGen _assemblyGen;
        public Dictionary<string, Dictionary<string, MethodGen>> ClassesMethods { get; }

        private const string RANDOM_FIELD_NAME = "random";

        private readonly Dictionary<string, TypeGen> _classes;
        private readonly Dictionary<string, Dictionary<string, MethodGen>> _classesMethods;

        private string _currentClassName;
        private string _currentMethodName;
        private CodeGen _currentCodeGen;
        private Operand _currentOperand;
        private Dictionary<string, Operand> _currentLocalVariables;
        private Dictionary<string, Operand> _currentClassFields;

        public SharpVisitor(AssemblyGen assemblyGen)
        {
            _assemblyGen = assemblyGen;
            Classes = new Dictionary<string, TypeGen>();
            ClassesMethods = new Dictionary<string, Dictionary<string, MethodGen>>();
            _currentClassName = null;
        }

        public override bool VisitConstDecl([NotNull] GoParser.ConstDeclContext context)
        {
            return base.VisitConstDecl(context);
        }
    }
}
