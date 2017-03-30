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
        string currentFunction;
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
            currentFunction = "";
        }

        public string CodeBootstrap(string fileName)
        {
            string command = ("@256 // BOOTSTRAP\n" + "D=A\n" + "@SP\n" + "M=D\n");
            command += CodeCommand(Command.C_CALL, "Sys.init", 0, 0, fileName);
            return command;
        }

        public string CodeCommand(Command com, string dest, int val, int iteration, string fileName)
        {
            string command = "";
            string comment = "";
            string label = fileName + "." + com.ToString() + iteration.ToString();

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
                    command += "\n@EQ-EQUAL" + iteration + "\nD;JEQ\n@SP\nA=M-1\nM=0";
                    command += "\n@EQ-END" + iteration + "\n0;JMP"; // not then 0
                    command += "\n(EQ-EQUAL" + iteration + ")\n@SP\nA=M-1\nM=-1"; // true then -1
                    command += "\n(EQ-END" + iteration + ")";
                    break;
                case Command.C_GT:
                    command = "@SP // GT\nD=M\nM=D-1\nA=M\nD=M\nA=A-1\nD=M-D";
                    command += "\n@GT-GREATER" + iteration + "\nD;JGT\n@SP\nA=M-1\nM=0";
                    command += "\n@GT-END" + iteration + "\n0;JMP"; // not then 0
                    command += "\n(GT-GREATER" + iteration + ")\n@SP\nA=M-1\nM=-1"; // true then -1
                    command += "\n(GT-END" + iteration + ")";
                    break;
                case Command.C_LT:
                    command = "@SP // LT\nD=M\nM=D-1\nA=M\nD=M\nA=A-1\nD=M-D";
                    command += "\n@LT-GREATER" + iteration + "\nD;JLT\n@SP\nA=M-1\nM=0";
                    command += "\n@LT-END" + iteration + "\n0;JMP"; // not then 0
                    command += "\n(LT-GREATER" + iteration + ")\n@SP\nA=M-1\nM=-1"; // true then -1
                    command += "\n(LT-END" + iteration + ")";
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
                    command = "(" + currentFunction + "$" +  dest + ") // LABEL";
                    break;
                case Command.C_IF:
                    command = "@SP // IF-GOTO\nAM=M-1\nD=M\n@" + currentFunction + "$" + dest + "\nD;JNE";
                    break;
                case Command.C_GOTO:
                    command = "@" + currentFunction + "$" + dest + " // GOTO\n";
                    command += "0;JMP";
                    break;
                case Command.C_FUNCTION:
                    currentFunction = dest;
                    command = "(" + dest + ") // " + label;
                    for (int i = 0; i < val; i++)
                    {
                        command += "\n@SP\nA=M\nM=0\n@SP\nM=M+1";
                        
                    }
                    break;
                case Command.C_RETURN:
                    command =  "@LCL // RETURN START: *FRAME = *LCL\n" + "D=M\n" + "@FRAME\n" + "M=D\n"; //Is temp var equal to ex LCL address
                    //         A=LCL adr   D=*LCL   A=FRAME adr   *FR=*LCL    
                    //command += "@5\nD=D-A\nA=D\nD=M\n@RET\nM=D // ret = *(FRAME-5)\n";
                    command += "@SP // *ARG=*(*SP-1) RETURN value placed\n" + "M=M-1\n" + "A=M\n" + "D=M\n" + "@ARG\n" + "A=M\n" + "M=D\n";
                    //          A=SP ard  *SP=*SP-1   A=*SP-1  D=*(*SP-1) A=ARG ard  A=*ARG    *ARG=*(*SP-1) // *ARG=*RET_VAL
                    command += "@ARG // *SP=*ARG+1\n" + "D=M+1\n" + "@SP\n" + "M=D\n";
                    //          A=ARG adr  D=*ARG+1    A=SP adr  *SP=*ARG+1  // Set SP to ARG+1 now ret value is on top of stack and SP is above
                    command += "@FRAME // *THAT=*(FRAME-1)\n" + "D=M\n" + "@1\n" + "D=D-A\n" + "A=D\n" + "D=M\n" + "@THAT\n" + "M=D\n";
                    //          FR addr      D=FR      A=1      D=FR-1      A=FR-1   *(FR-1)    A=THAT     *THAT=*(FR-1)
                    command += "@FRAME // *THIS=*(FRAME-2)\nD=M\n@2\nD=D-A\nA=D\nD=M\n@THIS\nM=D\n";
                    command += "@FRAME // *ARG=*(FRAME-3)\nD=M\n@3\nD=D-A\nA=D\nD=M\n@ARG\nM=D\n";
                    command += "@FRAME // *LCL=*(FRAME-4)\nD=M\n@4\nD=D-A\nA=D\nD=M\n@LCL\nM=D\n";
                    //command += "@RET\nA=M\n0; JMP";
                    command += "@FRAME // return to *(FRAME-5)\n" + "D=M\n" + "@5\n" + "A=D-A\n" + "A=M\n" + "0; JMP";
                    //          FR addr      D=FR      A=5      A=FR-5     A=*(FR-5)  goto *(FR-5)
                    //currentFunction = "";
                    break;
                case Command.C_CALL:
                    command = "@RETURN-" + dest + iteration + comment + "\n";
                    command += "D=A // push return-address\n" + "@SP\n" + "A=M\n" + "M=D\n";
                    command += "@SP // push LCL\n" + "M=M+1\n" + "@LCL\n" + "D=M\n" + "@SP\n" + "A=M\n" + "M=D\n";
                    command += "@SP // push ARG\n" + "M=M+1\n" + "@ARG\n" + "D=M\n" + "@SP\n" + "A=M\n" + "M=D\n";
                    command += "@SP // push THIS\n" + "M=M+1\n" + "@THIS\n" + "D=M\n" + "@SP\n" + "A=M\n" + "M=D\n";
                    command += "@SP // push THAT\n" + "M=M+1\n" + "@THAT\n" + "D=M\n" + "@SP\n" + "A=M\n" + "M=D\n";
                    command += "@SP // ARG = SP-n-5\n" + "M=M+1\n" + "D=M\n" + "@1\n" + "D=D-A\n" + "@5\n" + "D=D-A\n" + "@ARG\n" + "M=D\n";
                    command += "@SP // LCL = SP\n" + "D=M\n" + "@LCL\n" + "M=D\n";
                    command += "@" + dest + " // end of call to " + dest + "\n" + "0; JMP\n" + "(" + "RETURN-" + dest + iteration + ")";
                    break;
                
                
                default:
                    return "// " + com + " isn't implemented yet! ";

            }
            //if (writeComments) { return comment + command; }
            return command;
        }

    }
}
