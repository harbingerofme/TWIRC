using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWIRC
{
    public class com//allows us to check for coms, I guess. I could have prob made h
    {
        protected string keyword{get;set;}
        protected List<string> aliases{get;set;}
        protected List<string> responses { get; set; }
        protected int cooldown{get;set;}
        protected int lastTime { get; set; }
        protected int authLevel { get; set; }

        public bool doesMatch(string input)
        {
            if (input.StartsWith(keyword)) { return true; }
            foreach (string a in aliases) { if(input.StartsWith(a)){return true;}}
            return false;
        }

        public bool canTrigger()
        {
            if(lastTime+cooldown<getNow()){return true;}
            return false;
        }

        public void addAlias(string input)
        {
            aliases.Add(input);
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
    }

    public class command : com
    {
        public command() { throw new ArgumentException(); }//CAUSE I CAN
        public command(string kw,string response)
        {
            aliases = new List<string>(); responses = new List<string>();
            keyword = kw;
            responses.Add(response);
            authLevel = 0;
        }

        public command(string kw, string[] response)
        {
            aliases = new List<string>(); responses = new List<string>();
            keyword = kw;
            responses = response.ToList<string>();
            authLevel = 0;
        }
        public command(string kw, string response, string[] aliasess)//not a typo, just to avoid collision
        {
            aliases = new List<string>(); responses = new List<string>();
            keyword = kw;
            responses.Add(response);
            aliases = aliasess.ToList<string>();
            authLevel = 0;
        }
        public command(string kw, string[] response, string[] aliasess)
        {
            aliases = new List<string>(); responses = new List<string>();
            keyword = kw;
            responses = response.ToList<string>();
            aliases = aliasess.ToList<string>();
            authLevel = 0;
        }
        public command(string kw, string[] response, string[] aliasess, int auth)
        {
            aliases = new List<string>(); responses = new List<string>();
            keyword = kw;
            responses = response.ToList<string>();
            aliases = aliasess.ToList<string>();
            authLevel = auth;
        }
    }
}
