using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackVMTranslator
{
    class Code
    {
        Dictionary<string, string> keywords;

        string pushRegister;
        string tempRegister;
        string staticRegister;
        string pointerRegister;
        bool writeComments;
        public Code()
        {
            keywords = new Dictionary<string, string>();
            keywords.Add("local", "LCL");
            keywords.Add("argument", "ARG");
            keywords.Add("this", "THIS");
            keywords.Add("that", "THAT");
            keywords.Add("temp", "TEMP");
            pushRegister = "R13";
            tempRegister = "5";
            staticRegister = "16";
            pointerRegister = "3";
            writeComments = true;
        }

        public string CodeCommand(Command com, string dest, int val, int iteration)
        {
            string command = "";
            string comment = "";
            string label = com.ToString() + iteration.ToString();

            if (writeComments) { comment = " // " + com.ToString() + " " + dest.ToString() + " " + val.ToString(); }
            switch (com)
            {
                case Command.C_ADD:
                    command = "@SP // ADD\nD=M\nM=D-1\nA=M\nD=M\nA=A-1\nM=M+D";
                    break;
                case Command.C_SUB:
                    command = "@SP // SUB\nD=M\nM=D-1\nA=M\nD=M\nA=A-1\nM=M-D";
                    break;
                case Command.C_NEG:
                    command = "@SP // NEG\nD=M-1\nA=D\nM=-M";
                    break;
                case Command.C_EQ:
                    command = "@SP // EQ\nD=M\nM=D-1\nA=M\nD=M\nA=A-1\nD=M-D";
                    command += "\n@" + label + "-EQUAL\nD;JEQ\n@SP\nA=M-1\nM=0";
                    command += "\n@" + label + "-END\n0;JMP"; // not then 0
                    command += "\n(" + label + "-EQUAL)\n@SP\nA=M-1\nM=-1"; // true then -1
                    command += "\n(" + label + "-END)";
                    break;
                case Command.C_GT:
                    command = "@SP // EQ\nD=M\nM=D-1\nA=M\nD=M\nA=A-1\nD=M-D";
                    command += "\n@" + label + "-GREATER\nD;JGT\n@SP\nA=M-1\nM=0";
                    command += "\n@" + label + "-END\n0;JMP"; // not then 0
                    command += "\n(" + label + "-GREATER)\n@SP\nA=M-1\nM=-1"; // true then -1
                    command += "\n(" + label + "-END)";
                    break;
                case Command.C_LT:
                    command = "@SP // EQ\nD=M\nM=D-1\nA=M\nD=M\nA=A-1\nD=M-D";
                    command += "\n@" + label + "-GREATER\nD;JLT\n@SP\nA=M-1\nM=0";
                    command += "\n@" + label + "-END\n0;JMP"; // not then 0
                    command += "\n(" + label + "-GREATER)\n@SP\nA=M-1\nM=-1"; // true then -1
                    command += "\n(" + label + "-END)";
                    break;
                case Command.C_AND:
                    command = "@SP // AND\nD=M\nM=D-1\nA=M\nD=M\nA=A-1\nM=D&M";
                    break;
                case Command.C_OR:
                    command = "@SP // OR\nD=M\nM=D-1\nA=M\nD=M\nA=A-1\nM=D|M";
                    break;
                case Command.C_NOT:
                    command = "@SP // NOT\nD=M-1\nA=D\nM=!M";
                    break;

                case Command.C_PUSH:
                    // constant is way simpler than other
                    if (dest == "constant")
                    {
                        command = "@" + val + comment + "\n" + "D=A\n@SP\nA=M\nM=D\n@SP\nM=M+1";
                        break;
                    }
                    if (dest == "temp")
                    {
                        command = "@" + tempRegister + comment + "\nD=A\n";
                        
                    } else if (dest == "static")
                    {
                        command = "@" + staticRegister + comment + "\nD=A\n";
                    } else if (dest == "pointer")
                    {
                        command = "@" + pointerRegister + comment + "\nD=A\n";
                    }
                    else // for all except temp or static pointer
                    {
                        command = "@" + keywords[dest] + comment + "\nD=M\n";
                    }
                    // Calc and store
                    command += "@" + val + "\nD=D+A\n";
                    command += "A=D\nD=M\n@SP\nA=M\nM=D\n@SP\nM=M+1";

                    break;
                case Command.C_POP:
                    if (dest == "temp")
                    {
                        command = "@" + tempRegister + comment + "\nD=A\n";
                    }
                    else if (dest == "static")
                    {
                        command = "@" + staticRegister + comment + "\nD=A\n";
                    }
                    else if (dest == "pointer")
                    {
                        command = "@" + pointerRegister + comment + "\nD=A\n";
                    }
                    else
                    {
                        command = "@" + keywords[dest] + comment + "\nD=M\n";
                    }
                    command += "@" + val + "\nD=D+A\n";
                    command += "@" + pushRegister + "\nM=D\n@SP\nAM=M-1\nD=M\n";
                    command += "@" + pushRegister + "\nA=M\nM=D";
                    break;
                case Command.C_LABEL:
                    command = "(" +  dest + ") // LABEL";
                    break;
                case Command.C_IF:
                    command = "@SP // IF-GOTO\nAM=M-1\nD=M\n@" + dest + "\nD;JNE";
                    break;
                case Command.C_GOTO:
                    command = "@" + dest + " // GOTO\n";
                    command += "0;JMP";
                    break;

                default:
                    return "// " + com + " isn't implemented yet! ";

            }
            //if (writeComments) { return comment + command; }
            return command;
        }

    }
}
