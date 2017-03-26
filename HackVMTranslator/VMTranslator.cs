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
            FileHandler fileHandler = new FileHandler(args);
            if (!fileHandler.IsValid()) { return; }

            Code coder = new Code();
            fileHandler.AddLine(coder.CodeBootstrap(fileHandler.GetCurFileName()));

            while (fileHandler.HasMoreFiles())
            { 
                Parser parser = new Parser(fileHandler.Advance());
                int iteration = 0;
                while (true)
                {
                    iteration++;
                    if (parser.HasMoreCommands())
                    {
                        string code = "";
                        parser.Advance();
                        code = coder.CodeCommand(parser.GetCurCommand(), parser.GetCurDestination(), parser.GetCurValue(), iteration, fileHandler.GetCurFileName());
                        //Console.WriteLine("cur command:" + parser.GetCurCommand() + " dest:" + parser.GetCurDestination() + " val:" + parser.GetCurValue());
                        fileHandler.AddLine(code);

                    }
                    else break;
                }

            }
            fileHandler.SaveFile();
            //Console.ReadKey();
            return;
            
        }
    }
}
