using Antlr4.Runtime;
using CC_Course;
using System;

namespace CC_Course
{
   public class Program
    {
        public static void Main(string[] args)
        {
            var miniJavaCompiler = new GoCompiler();
            var name = "StatLinkedList";
            var exec = true;
            
            miniJavaCompiler.Compile($@"C:\Users\timur\compilers_labs\bmstu_CC\CC_Course\CC_Course\GoCode\{name}.txt", $"{name}", $@"C:\Users\timur\compilers_labs\bmstu_CC\CC_Course\CC_Course\Results\{name}.exe");

            if (exec)
            {
                AppDomain.CurrentDomain.ExecuteAssembly($@"C:\Users\timur\compilers_labs\bmstu_CC\CC_Course\CC_Course\Results\{name}.exe");
            }
        }
    }
}

