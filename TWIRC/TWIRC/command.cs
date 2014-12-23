using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TWIRC
{
    public class com//allows us to check for coms, I guess. I could have prob made h
    {
        protected string keyword{get;set;}
        protected List<string> responses { get; set; }
        protected int cooldown{get;set;}
        protected int lastTime { get; set; }
        protected int authLevel { get; set; }
        protected int count { get; set; }

        public bool doesMatch(string input)
        {
            string a =input.ToLower();
            string b = keyword.ToLower();
            if (a.StartsWith(keyword+ " ")||a==keyword) { return true; }
            return false;
        }

        public bool canTrigger()
        {
            if(lastTime+cooldown<getNow()){return true;}
            return false;
        }
        public void addResponse(string input)
        {
            responses.Add(input);
        }
        public string[] getResponses()
        {
            return responses.ToArray();
        }
        public void updateTime()
        {
            lastTime = getNow();
        }
        public int getNow()
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = DateTime.Now.ToUniversalTime() - origin;
            return (int)Math.Floor(diff.TotalSeconds);
        }
        public int setCount(int amount)
        {
            count = amount;
            return count;
        }
        public int getCount()
        {
            return count;
        }
        public int addCount(int amount)
        {
            count += amount;
            return count;
        }
        public int setCooldown(int amount)
        {
            if (amount > -1)
            {
                cooldown = amount;
                return amount;
            }
            else
            {
                return -1;
            }
        }


        public string[] getResponse(string input, string user)
        {
            //System.Diagnostics.Debugger.Break();
            string reg1 = "("+ keyword + "[ ]?)";//remove the keyword
            string newPut = Regex.Replace(input,reg1,"");
            string[] pars = {"","","","","","","","","",""};
                for(int i =0;i< newPut.Split(' ').Count()&&i<10;i++){
                    pars[i] = newPut.Split(' ')[i];
                }
            if( newPut.Split(' ').Count()>10){
                for (int i = 10; i <= newPut.Split(' ').Count(); i++)
                {
                    pars[9] += " " + newPut.Split(' ')[i];//merge all results bigger than 10 in the last parameter
                }
            }
            Console.Write("\n");
            string[] output = responses.ToArray();
            int a=0;
            string str2, str3,returnString;
            bool failure = false;
            foreach( string str1 in output)
            {
                returnString = str1;
                
                while(Regex.Match(returnString,@"#par([123456789])#",RegexOptions.IgnoreCase).Success && !failure){
                    Match match = Regex.Match(returnString, @"#par(\d)#", RegexOptions.IgnoreCase);
                        if (pars[int.Parse(match.Groups[1].Captures[0].Value) - 1].Length > 0)
                        {
                            str2 = returnString.Substring(0, match.Index);
                            str3 = returnString.Substring(match.Index + 6);
                            returnString = str2 + pars[int.Parse(match.Groups[1].Captures[0].Value) - 1] + str3;
                        }
                        else
                        {
                            failure = true;
                        }
                }

                for (int b = 1; b < pars.Count()+1; b++)
                {
                    returnString = returnString.Replace("@par" + b + "@", pars[b - 1]);
                }

                for (int b = 1; b < pars.Count()+1; b++)
                {
                    str2 = "";
                    for (int c = b; c < pars.Count()+1; c++) { str2 += pars[c-1]+ " "; }
                    returnString = returnString.Replace("@par" + b + "-@", str2);
                }
                returnString = returnString.Replace("@count@", count.ToString());
                returnString = returnString.Replace("@user@", user);
                Random rnd = new Random();
                while (Regex.Match(returnString, "@rand(\\d+)@").Success)
                {
                    Match match = Regex.Match(returnString, "@rand(\\d+)-(\\d+)@");
                    str2 = returnString.Substring(0, match.Index);
                    str3 = returnString.Substring(match.Groups[1].Captures[0].Value.Length + match.Index + 6);
                    returnString = str2 + rnd.Next(int.Parse(match.Groups[1].Captures[0].Value)) + str3;
                }
                    
                while(Regex.Match(returnString, "@rand(\\d+)-(\\d+)@").Success)
                {
                    Match match = Regex.Match(returnString, "@rand(\\d+)-(\\d+)@");
                    if (int.Parse(match.Groups[1].Captures[0].Value) > int.Parse(match.Groups[2].Captures[0].Value)) { break; }
                    str2 = returnString.Substring(0, match.Index);
                    str3 = returnString.Substring(match.Index + match.Value.Length);
                    returnString = str2 + rnd.Next(int.Parse(match.Groups[1].Captures[0].Value), int.Parse(match.Groups[2].Captures[0].Value)+1) + str3;
                     
                }
                
                output[a] = returnString;
                a++;

            }
            if (failure) { output= new string[]{""};}
            return output;

        }
    }

    public class command : com
    {
        public command() { throw new ArgumentException(); }//CAUSE I CAN
        public command(string fromString)
        {
            string[] a = fromString.Split(' ');
            string c="";
            for (int b = 1; b < a.Count(); b++)
            {
                switch (b)
                {
                    case 1: authLevel = int.Parse(a[b]); break;
                    case 2: count = int.Parse(a[b]); break;
                    case 3: keyword = a[b]; break;
                    default: c += a[b]; if (b + 1 < a.Count()) { c += " "; }; break;
                }

            }
            responses = c.Split(new string[] { "}}}}||||>>>>" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (responses.Count() == 0) { throw new MissingFieldException(); }//having none of that shit.
        }
        public command(string kw,string response)
        {
            responses = new List<string>();
            keyword = kw.ToLower();
            responses.Add(response);
            authLevel = 0;
            count = 0;
            responses = makeLowerCase(responses.ToArray()).ToList();
        }

        public command(string kw, string[] response)
        {
            responses = new List<string>();
            keyword = kw.ToLower();
            responses = response.ToList<string>();
            authLevel = 0;
            count = 0;
            responses = makeLowerCase(responses.ToArray()).ToList();
        }
        public command(string kw, string[] response, int auth)
        {
            responses = new List<string>();
            keyword = kw.ToLower();
            responses = response.ToList<string>();
            authLevel = auth;
            count = 0;
            responses = makeLowerCase(responses.ToArray()).ToList();
        }
        public string[] makeLowerCase(string[] respos){
            string[] ret = respos;
            String[] regs = new String[4];
            regs[0] = "@count@";
            regs[1] = @"@par([\d|(\d-)|(\d-\d)])@";
            regs[2] = @"@rand([\d|(\d-\d)])@";
            regs[3] = "@user@";

            for(int a =0; a< respos.Count();a++){
                respos[a] = Regex.Replace(respos[a],regs[0],"@count@");
                respos[a] = Regex.Replace(respos[a],regs[3], "@user@");
                respos[a] = Regex.Replace(respos[a], regs[1], @"@par$1@");
                respos[a] = Regex.Replace(respos[a],regs[2], @"@rand$1@");

            }
            return ret;
        }

        public override string ToString()
        { //order is as follows: index type access_level count keyword(s) amount of responses <message|keyword(s)>
            string result = "1 " + authLevel + " " + count + " " + keyword + " ";
            foreach (string response in responses)
            {
                result += response + "}}}}||||>>>>"//that is some neato escape string right? Better not ever tell our users. I cannot predict what will happen if they make it this way.
;
            }
            return result;//caller should prepend a index number himself, and append a line ending
        }
    }
}
