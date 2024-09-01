using Antlr4.Runtime;
using CC_Course.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriAxis.RunSharp;

namespace CC_Course
{
    public class GoCompiler
    {
        public void Compile(string sourceFile, string assemblyName, string outputFile)
        {
            var input = new AntlrFileStream(sourceFile);
            var lexer = new GoLexer(input);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new GoParser(tokenStream);

            /*var printNodesVisitor = new PrintNodesVisitor();
            printNodesVisitor.Visit(parser.goal());
            parser.Reset();*/

            var assemblyGenerator = new AssemblyGen(assemblyName, new CompilerOptions()
            {
                OutputPath = outputFile
            });

            var classesDeclarationsVisitor = new StructTypeDeclarationVisitor(assemblyGenerator);
            classesDeclarationsVisitor.Visit(parser.sourceFile());

            var classes = classesDeclarationsVisitor.Classes;

            var classesMethodsDeclarationsVisitor = new ClassesMethodsDeclarationVisitor(assemblyGenerator, classes);
            parser.Reset();
            classesMethodsDeclarationsVisitor.Visit(parser.sourceFile());

            var classesMethods = classesMethodsDeclarationsVisitor.ClassesMethods;

            var methodsImplementationVisitor = new ClassAndMethodImplementationVisitor(
                classes,
                classesMethods,
                assemblyGenerator);

            parser.Reset();
            methodsImplementationVisitor.Visit(parser.sourceFile());

            assemblyGenerator.Save();
        }
    }
}
