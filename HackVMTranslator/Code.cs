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
        
        public Code()
        {
            keywords = new Dictionary<int, string>();
            keywords.Add((int)Destination.D_LOCAL, "LCL");
            keywords.Add((int)Destination.D_ARGUMENT, "ARG");
            keywords.Add((int)Destination.D_THIS, "THIS");
            keywords.Add((int)Destination.D_THAT, "THAT");
            keywords.Add((int)Destination.D_TEMP, "TEMP");
        }



        public string CodeCommand(Command com, Destination dest, int val)
        {
            string command = "";
            string comment = "";
            string pushRegister = "R13";
            string tempRegister = "5";
            bool writeComments = true;
            if (writeComments) { comment = " // " + com.ToString() + " " + dest.ToString() + " " + val.ToString(); }

            switch (com)
            {
                case Command.C_ADD:
                    command += "@SP // ADD\nD=M\nM=D-1\nA=M\nD=M\nA=A-1\nM=M+D";
                    break;
                //break;
                case Command.C_AND:
                    return "// AND isn't implemented yet";
                case Command.C_SUB:
                    command += "@SP // SUB\nD=M\nM=D-1\nA=M\nD=M\nA=A-1\nM=M-D";
                    break;
                case Command.C_PUSH:
                    if (dest == Destination.D_CONSTANT)
                    {
                        command = "@" + val + comment + "\n" + "D=A\n@SP\nA=M\nM=D\n@SP\nM=M+1";
                        break;
                    }
                    if (dest == Destination.D_TEMP)
                    {
                        command += "@" + tempRegister + comment + "\nD=A\n";
                        command += "@" + val + "\nD=D+A\n";
                    } else
                    {
                        command += "@" + keywords[(int)dest] + comment + "\nD=M\n";
                        command += "@" + val + "\nD=D+A\n";
                    }
                    
                    command += "@" + pushRegister + "\nM=D\n@SP\nA=M\nD=M\n";
                    command += "@" + pushRegister + "\nA=M\nM=D\n@SP\nM=M+1";
                    break;
                case Command.C_POP:
                    if (dest == Destination.D_TEMP)
                    {
                        command += "@" + tempRegister + comment + "\nD=A\n";
                        command += "@" + val + "\nD=D+A\n";
                    }
                    else
                    {
                        command += "@" + keywords[(int)dest] + comment + "\nD=M\n";
                        command += "@" + val + "\nD=D+A\n";
                    }
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
