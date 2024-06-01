using CC_Lab3.LexerDir;
using CC_Lab3.ParserDir;
using CC_Lab3.Visualization;

class Program
{
    static void Main(string[] args)
    {
        var fileName = "4";

        var file = $"C:/Users/timur/compilers_labs/bmstu_CC/Lab3/CC_Lab3/CC_Lab3/Texts/{fileName}.txt";


        var exampleText = File.ReadAllText(file);

        Console.WriteLine(exampleText);

        Lexer lexer = new Lexer(exampleText);
        Parser parser = new Parser();

        var tree = parser.Parse(lexer);

        var resultFile = $"C:/Users/timur/compilers_labs/bmstu_CC/Lab3/CC_Lab3/CC_Lab3/Trees/{fileName}";

        Visualisator.SaveTreeToFile(tree, resultFile);
    }
}
