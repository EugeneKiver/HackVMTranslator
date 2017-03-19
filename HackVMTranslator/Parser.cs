using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackVMTranslator
{
    enum Command
    {
        C_NONE,
        C_ARITHMETIC,
        C_ADD, C_SUB, C_NEG,
        C_EQ, C_GT, C_LT,
        C_AND, C_OR, C_NOT,
        C_PUSH, C_POP,
        C_LABEL, C_GOTO,
        C_IF, C_FUNCTION,
        C_RETURN, C_CALL
    };

    enum Destination
    {
        D_NONE,
        D_CONSTANT,
        D_LOCAL,
        D_ARGUMENT,
        D_THIS,
        D_THAT,
        D_TEMP,
        D_STATIC,
        D_POINTER
    };

    class Parser
    {
        string[] lines;
        List<string> code;
        int currentIndex;
        Command currentCommand;
        string currentDest;
        int currentValue;
        string currentLabel;

        /// <summary>
        /// Initializes all vars and opens a file.</summary>
        public Parser(string[] inputCode)
        {
            currentIndex = -1;
            currentDest = "";
            currentCommand = Command.C_NONE;
            currentValue = 0;
            lines = inputCode;
            code = new List<string>();
            RemoveComments();
        }

        /// <summary>
        /// Are there more commands in the input?</summary>
        public bool HasMoreCommands()
        {
            if ((code.Count - 1) - currentIndex > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reads the next command from the input and makes it the current command.</summary>
        /// <remarks>
        /// Should be called only if hasMoreCommands() is true. Initially there is no current command.</remarks>
        public void Advance()
        {
            if (HasMoreCommands())
            {
                currentIndex++;
                ParseCommand();
                /*if (currentCommand == Command.C_PUSH || currentCommand == Command.C_POP)
                    Console.WriteLine(currentIndex + ": " + currentCommand + " " + currentDest + " " + currentValue);
                else
                    Console.WriteLine(currentIndex + ": " + currentCommand);
                */
            }
        }

        public Command GetCurCommand()
        {
            return currentCommand;
        }

        public string GetCurDestination()
        {
            return currentDest;
        }

        public int GetCurValue()
        {
            return currentValue;
        }

        /// <summary>
        /// Creates custom label for construction of if-else for bitwise operations
        /// </summary>
        /// <returns></returns>
        public string GetCurLabel()
        {
            return currentLabel;
        }


        


        /// <summary>
        /// Removes comments and white spaces.</summary>
        private void RemoveComments()
        {
            //if (error > 0) { return; }
            foreach (string line in lines)
            {
                string curLine = line;
                if (curLine.StartsWith("//")) { continue; }
                int index = curLine.IndexOf("//");
                if (index > -1) { curLine = curLine.Remove(index); }
                curLine = curLine.Trim();
                if (curLine == "") { continue; }
                /*if (curLine.StartsWith("("))
                {
                    int pos = code.Count;
                    string[] parts = curLine.Split(new char[] { '(', ')' });
                    table.AddEntry(parts[1], pos);
                    Console.WriteLine("Label: " + parts[1] + " at " + pos);
                    continue;
                }*/
                code.Add(curLine);
            }
            //currentIndex = 0;
            //Console.WriteLine("code length is: " + code.Count);
            if (code.Count < 1)
            {
                //error = 1;
                Console.WriteLine("file doesn't contain any code");
            }
            /*for (int i = 0; i < code.Count; i++)
            {
                Console.WriteLine(i + ": " + code[i]);
            }*/
        }

        /*private Destination CheckKnownDestination(string strDest)
        {
            switch (strDest)
            {
                case "constant":
                    return Destination.D_CONSTANT;
                case "argument":
                    return Destination.D_ARGUMENT;
                case "local":
                    return Destination.D_LOCAL;
                case "this":
                    return Destination.D_THIS;
                case "that":
                    return Destination.D_THAT;
                case "temp":
                    return Destination.D_TEMP;
                case "static":
                    return Destination.D_STATIC;
                case "pointer":
                    return Destination.D_POINTER;

                default:
                    return Destination.D_NONE;
            }
        }*/
        
        /// <summary>
        /// Parses command and stores current context in global vars. 
        /// TODO fix arithmetic return</summary>
        private void ParseCommand()
        {
            string line = code[currentIndex];
            if (line.StartsWith("add"))
            {
                currentCommand = Command.C_ADD;
                return;
            }
            if (line.StartsWith("sub"))
            {
                currentCommand = Command.C_SUB;
                return;
            }
            if (line.StartsWith("neg"))
            {
                currentCommand = Command.C_NEG;
                return;
            }
            if (line.StartsWith("eq"))
            {
                currentCommand = Command.C_EQ;
                return;
            }
            if (line.StartsWith("gt"))
            {
                currentCommand = Command.C_GT;
                return;
            }
            if (line.StartsWith("lt"))
            {
                currentCommand = Command.C_LT;
                return;
            }
            if (line.StartsWith("and"))
            {
                currentCommand = Command.C_AND;
                return;
            }
            if (line.StartsWith("or"))
            {
                currentCommand = Command.C_OR;
                return;
            }
            if (line.StartsWith("not"))
            {
                currentCommand = Command.C_NOT;
                return;
            }
            if (line.StartsWith("push"))
            {
                string[] parts = line.Split(new char[] { ' ' });
                if (parts.Length > 0)
                {
                    currentCommand = Command.C_PUSH;
                    currentDest = parts[1];
                    currentValue = Int32.Parse(parts[2]);
                }
                return;
            }
            if (line.StartsWith("pop"))
            {
                string[] parts = line.Split(new char[] { ' ' });
                if (parts.Length > 0)
                {
                    currentCommand = Command.C_POP;
                    currentDest = parts[1];
                    currentValue = Int32.Parse(parts[2]);
                }
                return;
            }

            if (line.StartsWith("label"))
            {
                string[] parts = line.Split(new char[] { ' ' });
                if (parts.Length > 0)
                {
                    currentCommand = Command.C_LABEL;
                    currentDest = parts[1];
                }
                return;
            }
            if (line.StartsWith("if-goto"))
            {
                string[] parts = line.Split(new char[] { ' ' });
                if (parts.Length > 0)
                {
                    currentCommand = Command.C_IF;
                    currentDest = parts[1];
                }
                return;
            }
            if (line.StartsWith("goto"))
            {
                string[] parts = line.Split(new char[] { ' ' });
                if (parts.Length > 0)
                {
                    currentCommand = Command.C_GOTO;
                    currentDest = parts[1];
                }
                return;
            }
            return;
        }
    }
}
