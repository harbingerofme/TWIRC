using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System;

namespace TWIRC
{
    class Calculator
    {
        public double LastValue;

        public Calculator()
        {
            LastValue = -1;
        }

        public Calculation Parse(string formula)
        {
            formula = formula.ToLower().Replace("sqrt", "√");
            formula = formula.Replace("pi", "π");
            formula = formula.Replace("arctan", "Ⓣ");
            formula = formula.Replace("arcsin", "Ⓢ");
            formula = formula.Replace("arccos", "Ⓒ");
            formula = formula.Replace("cos", "Ｃ");
            formula = formula.Replace("tan", "Ｔ");
            formula = formula.Replace("sin", "Ｓ");
            formula = formula.Replace(" ", "");

            List<string> stack = new List<string>();
            List<char> opstack = new List<char>();
            double answer = 0; bool error = false; string numberString = "";
            List<double> finalStack = new List<double>();
            double a; string end = "";
            foreach (char c in formula)
            {
                if (Regex.Match(c.ToString(), @"[0-9\.\,]").Success) { numberString += c; end += c; }
                else if (numberString != "" && double.TryParse(numberString, out a)) { stack.Add(numberString); numberString = ""; }
                if (c == 'π')
                {
                    stack.Add(Math.PI.ToString()); end += c;
                }
                if (Regex.Match(c.ToString(), @"[-+*/^√ⓉⓈⒸＣＳＴ]").Success)
                {
                    end += c;
                    if (opstack.Count != 0)
                    {
                        if ((getAssociative(c) == 1 && getPrecedence(c) < getPrecedence(opstack[0])) || (getAssociative(c) == -1 && getPrecedence(c) <= getPrecedence(opstack[0])))
                        {
                            stack.Add(opstack[0].ToString());
                            opstack.RemoveAt(0);
                        }
                    }
                    opstack.Insert(0, c);
                }
                if (c == '(')
                {
                    end += c;
                    opstack.Insert(0, c);
                }
                if (c == ')')
                {
                    end += c;
                    if (opstack.Count > 0)
                    {
                        while (opstack.Count > 0 && opstack[0] != '(')
                        {
                            stack.Add(opstack[0].ToString());
                            opstack.RemoveAt(0);
                        }
                        if (opstack.Count > 0)
                        {
                            opstack.RemoveAt(0);
                        }
                        else
                        {
                            error = true;
                        }
                    }
                    else
                    {
                        error = true;
                    }
                }
            }
            if (numberString != "")
            {
                stack.Add(numberString);
            }
            foreach (char c in opstack)
            {
                stack.Add(c.ToString());
            }

            try
            {
                foreach (string s in stack)
                {
                    if (double.TryParse(s, out a))
                    {
                        finalStack.Add(a);
                    }
                    else switch (s)
                        {
                            case "+": finalStack[finalStack.Count - 2] = finalStack[finalStack.Count - 1] + finalStack[finalStack.Count - 2]; finalStack.RemoveAt(finalStack.Count - 1); break;
                            case "-": finalStack[finalStack.Count - 2] = finalStack[finalStack.Count - 2] - finalStack[finalStack.Count - 1]; finalStack.RemoveAt(finalStack.Count - 1); break;
                            case "/": finalStack[finalStack.Count - 2] = finalStack[finalStack.Count - 2] / finalStack[finalStack.Count - 1]; finalStack.RemoveAt(finalStack.Count - 1); break;
                            case "*": finalStack[finalStack.Count - 2] = finalStack[finalStack.Count - 1] * finalStack[finalStack.Count - 2]; finalStack.RemoveAt(finalStack.Count - 1); break;
                            case "^": finalStack[finalStack.Count - 2] = Math.Pow(finalStack[finalStack.Count - 2], finalStack[finalStack.Count - 1]); finalStack.RemoveAt(finalStack.Count - 1); break;
                            case "√": finalStack[finalStack.Count - 1] = Math.Sqrt(finalStack[finalStack.Count - 1]); break;
                            case "Ⓣ": finalStack[finalStack.Count - 1] = Math.Atan(finalStack[finalStack.Count - 1]); break;
                            case "Ⓢ": finalStack[finalStack.Count - 1] = Math.Asin(finalStack[finalStack.Count - 1]); break;
                            case "Ⓒ": finalStack[finalStack.Count - 1] = Math.Acos(finalStack[finalStack.Count - 1]); break;
                            case "Ｔ": finalStack[finalStack.Count - 1] = Math.Tan(finalStack[finalStack.Count - 1]); break;
                            case "Ｓ": finalStack[finalStack.Count - 1] = Math.Sin(finalStack[finalStack.Count - 1]); break;
                            case "Ｃ": finalStack[finalStack.Count - 1] = Math.Cos(finalStack[finalStack.Count - 1]); break;
                    }
                    //Console.Write(s + ":");
                }
                if (finalStack.Count != 1)
                {
                    error = true;
                }
                else
                {
                    answer = finalStack[0];
                    LastValue = answer;
                }
            }
            catch
            {
                error = true;
            }

            return new Calculation(!error, answer, end);
        }
        private int getPrecedence(char i)//Yes I know the breaks are redundant, they are only still here to piss people off.
        {
            switch (i)
            {
                case '+': return 2; break;
                case '-': return 2; break;
                case '/': return 3; break;
                case '*': return 3; break;
                case '^': return 4; break;
                case '√': return 5; break;
                case 'Ⓣ': return 5; break;
                case 'Ⓢ': return 5; break;
                case 'Ⓒ': return 5; break;
                case 'Ｔ': return 5; break;
                case 'Ｓ': return 5; break;
                case 'Ｃ': return 5; break;
                default: return 0; break;//Wha?
            }
        }

        private int getAssociative(char i)//Screw everyone.
        {
            switch (i)
            {
                case '+': return -1; break;
                case '-': return -1; break;
                case '/': return -1; break;
                case '*': return -1; break;
                case '^': return 1; break;
                case '√': return 1; break; //correct?
                case 'Ⓣ': return 1; break;
                case 'Ⓢ': return 1; break;
                case 'Ⓒ': return 1;break;
                case 'Ｔ': return 1; break;
                case 'Ｓ': return 1; break;
                case 'Ｃ': return 1; break;
                default: return -1; break;//Wha?
            }
        }//YOLO
    }

    class Calculation
    {
        public bool Valid;
        public double Answer;
        public string Input;

        public Calculation(bool v, double a, string i)
        {
            Valid = v;
            Answer = a;
            Input = i;
        }
    }
}
