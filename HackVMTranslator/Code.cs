using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackVMTranslator
{
    class Code
    {
        Dictionary<int, string> keywords;

        string pushRegister;
        string tempRegister;
        string staticRegister;
        bool writeComments;
        public Code()
        {
            keywords = new Dictionary<int, string>();
            keywords.Add((int)Destination.D_LOCAL, "LCL");
            keywords.Add((int)Destination.D_ARGUMENT, "ARG");
            keywords.Add((int)Destination.D_THIS, "THIS");
            keywords.Add((int)Destination.D_THAT, "THAT");
            keywords.Add((int)Destination.D_TEMP, "TEMP");
            pushRegister = "R13";
            tempRegister = "5";
            staticRegister = "16";
            writeComments = true;
        }



        public string CodeCommand(Command com, Destination dest, int val)
        {
            string command = "";
            string comment = "";
            if (writeComments) { comment = " // " + com.ToString() + " " + dest.ToString() + " " + val.ToString(); }
            switch (com)
            {
                case Command.C_ADD:
                    command = "@SP // ADD\nD=M\nM=D-1\nA=M\nD=M\nA=A-1\nM=M+D";
                    break;
                //break;
                case Command.C_AND:
                    return "// AND isn't implemented yet";
                case Command.C_SUB:
                    command = "@SP // SUB\nD=M\nM=D-1\nA=M\nD=M\nA=A-1\nM=M-D";
                    break;
                case Command.C_PUSH:
                    // constant is way simpler than other
                    if (dest == Destination.D_CONSTANT)
                    {
                        command = "@" + val + comment + "\n" + "D=A\n@SP\nA=M\nM=D\n@SP\nM=M+1";
                        break;
                    }

                    // For all the rest find dest address
                    if (dest == Destination.D_TEMP)
                    {
                        command = "@" + tempRegister + comment + "\nD=A\n";
                        
                    } else if (dest == Destination.D_STATIC)
                    {
                        command = "@" + staticRegister + comment + "\nD=A\n";
                    }
                    else // for all except temp or static
                    {
                        command = "@" + keywords[(int)dest] + comment + "\nD=M\n";
                    }
                    // Calc and store
                    command += "@" + val + "\nD=D+A\n";
                    command += "A=D\nD=M\n@SP\nA=M\nM=D\n@SP\nM=M+1";

                    break;
                case Command.C_POP:
                    if (dest == Destination.D_TEMP)
                    {
                        command = "@" + tempRegister + comment + "\nD=A\n";
                    }
                    else if (dest == Destination.D_STATIC)
                    {
                        command = "@" + staticRegister + comment + "\nD=A\n";
                    } else
                    {
                        command = "@" + keywords[(int)dest] + comment + "\nD=M\n";
                    }
                    command += "@" + val + "\nD=D+A\n";
                    command += "@" + pushRegister + "\nM=D\n@SP\nAM=M-1\nD=M\n";
                    command += "@" + pushRegister + "\nA=M\nM=D";
                    break;
                default:
                    return "// ? isn't implemented yet";

            }
            //if (writeComments) { return comment + command; }
            return command;
        }

    }
}
