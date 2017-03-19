using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackVMTranslator
{
    class Program
    {
        static void Main(string[] args)
        {
            bool setupMemoryMapping = true; // this is for testing purposes only, should be false for release
            string srcExt = ".vm";
            string destExt = ".asm";
            string file = args[0];
            string path = "";
            string[] input  ;
            List<string> output = new List<string>();
            //SymbolTable table = SymbolTable.Instance;

            try
            {
                path = Path.GetFullPath(file);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString() + "\n" + path);
                return;
            }

            if (path.EndsWith(srcExt, System.StringComparison.CurrentCultureIgnoreCase))
            {
                input = System.IO.File.ReadAllLines(path);
            }
            else
            {
                Console.WriteLine("error " + path);
                return;
            }

            Parser parser = new Parser(input);
            Code coder = new Code();

            int iteration = 0;
            if (setupMemoryMapping)
            {
                string memoryMapping = "";
                memoryMapping += "@317 // Setup Memory Mapping\nD=A\n@SP\nM=D\n"; // SP
                memoryMapping += "@317\nD=A\n@LCL\nM=D\n"; // LCL
                memoryMapping += "@310\nD=A\n@ARG\nM=D\n"; // ARG
                memoryMapping += "@3000\nD=A\n@THIS\nM=D\n"; // THIS
                memoryMapping += "@4000\nD=A\n@THAT\nM=D\n"; // THAT

                memoryMapping += "@1234\nD=A\n@310\nM=D\n"; // ARG
                memoryMapping += "@37\nD=A\n@311\nM=D\n"; // ARG
                memoryMapping += "@9\nD=A\n@312\nM=D\n"; // ARG
                memoryMapping += "@305\nD=A\n@313\nM=D\n"; // ARG
                memoryMapping += "@300\nD=A\n@314\nM=D\n"; // ARG
                memoryMapping += "@3010\nD=A\n@315\nM=D\n"; // ARG
                memoryMapping += "@4010\nD=A\n@136\nM=D"; // ARG
                output.Add(memoryMapping);
            }
            while (true)
            {
                iteration++;
                if (parser.HasMoreCommands())
                {
                    string code = "";
                    parser.Advance();
                    code = coder.CodeCommand(parser.GetCurCommand(), parser.GetCurDestination(), parser.GetCurValue(), iteration);
                    //Console.WriteLine("cur command:" + parser.GetCurCommand() + " dest:" + parser.GetCurDestination() + " val:" + parser.GetCurValue());
                    output.Add(code);
                }
                else break;
            }
            string[] codeArray = output.ToArray();
            path = path.Replace(srcExt, destExt);
            Console.WriteLine("dest path: " + path);
            System.IO.File.WriteAllLines(path, codeArray);

            Console.WriteLine("\nFinal code");
            foreach (string line in output)
            {
                Console.WriteLine(line);
            }
            //Console.ReadKey(); // Pause to show the results
        }
    }
}
