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
            return count;
        }
        public string[] getResponse(string input, string user)
        {
            //System.Diagnostics.Debugger.Break();
            string reg1 = "("+ keyword + " )|(  )";//remove the keyword and double spaces
            string newPut = Regex.Replace(input,reg1,"");
            string[] pars = newPut.Split(' ');
            if(pars.Count()>10){
                for(int i = 10;i<=pars.Count();i++){
                    pars[9]+=" "+pars[i];//merge all results bigger than 10 in the last parameter
                }
            }
            foreach (string absd in pars)
            {
                Console.Write(absd + ";");
            }
            Console.Write("\n");
            string[] output = responses.ToArray();
            int a=0;
            //I actually should make a list of regexes to be cleaner, but oh well
            Regex reg2 = new Regex("@rand(\\d+)@");
            Regex reg3 = new Regex("@rand(\\d+)-(\\d+)@");
            string str2, str3,returnString;
            foreach( string str1 in output)
            {
                returnString = str1;
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
                foreach (Match match in Regex.Matches(returnString, "@rand(\\d+)@"))
                {
                    //http://puu.sh/dogkG/4b5408be02.png
                    Console.WriteLine("Match: {0}", match.Value);
                    for (int groupCtr = 0; groupCtr < match.Groups.Count; groupCtr++)
                    {
                        Group group = match.Groups[groupCtr];
                        Console.WriteLine("   Group {0}: {1}", groupCtr, group.Value);
                        for (int captureCtr = 0; captureCtr < group.Captures.Count; captureCtr++)
                            Console.WriteLine("      Capture {0}: {1}", captureCtr,
                                              group.Captures[captureCtr].Value);

                    }
                }
                    /*
                while (reg3.Match(returnString).Success)
                {
                    
                    Match mat = reg2.Match(returnString);
                    System.Diagnostics.Debugger.Break();
                    str2 = returnString.Substring(0, mat.Index);//expertly split the string (of course we could use a replace, but we want different random numbers, don't we?
                    Console.WriteLine("" + mat.Captures[0].Value + ";" + mat.Captures[1].Value);
                    
                    str3 = returnString.Substring(mat.Index + mat.Captures[0].Value.Length + mat.Captures[1].Value.Length + 7);
                    returnString = str2 + rnd.Next(int.Parse(mat.Captures[0].Value), int.Parse(mat.Captures[1].Value)) + str3;
                     
                }
                */
                output[a] = returnString;
                a++;

            }

            return output;

        }
    }

    public class command : com
    {
        public command() { throw new ArgumentException(); }//CAUSE I CAN
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
        { //order is as follows: index type access_level count keyword(s) [keycode] <message|keyword(s)>
            string result = "1 " + authLevel + " " + count + " " + keyword + " "+responses.Count+ " ";
            foreach (string response in responses)
            {
                result += responses + "}}}}||||>>>>"//that is some neato escape string right? Better not ever tell our users. I cannot predict what will happen if they make it this way.
;
            }
            return result;//caller should prepend a index number himself, and append a line ending
        }
    }
}
