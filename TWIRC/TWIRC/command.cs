using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWIRC
{
    public class com//allows us to check for coms, I guess. I could have prob made h
    {
        public string _keyword;
        public string[] _responses;
        public string[] _aliases;
        public int _lastTime = 0;
        public int _authlevel;
        public int _cooldown;

        public string keyword
        {
            get { return _keyword; }
            set { _keyword = value; }
        }
        public string[] responses
        {
            get { return _responses; }
            set { _responses = value; }
        }
        public string[] aliases
        {
            get { return _aliases; }
            set { _aliases = value; }
        }
        public int authlevel
        {
            get { return _authlevel; }
            set { _authlevel = value; }
        }
        public int cooldown
        {
            get { return _cooldown; }
            set { _cooldown = value; }
        }
        public int lastTime
        {
            get { return _lastTime; }
            set { _lastTime = value; }
        }
    }

    public class command : com
    {

        public command(string keyword,string response)
        {
            _cooldown = 20;
            _lastTime = 0;
            _keyword = keyword;
            string[] _responses ={response};
            _aliases = null;
            _authlevel = 0;
        }

        public command(string keyword, string response,string[] aliases)
        {
            _keyword = keyword;
            _responses[0] = response;
            _aliases = aliases;
            _authlevel = 0;
        }

        public command(string keyword, string[] responses)
        {
            _keyword = keyword;
            _responses = responses;
            _aliases = null;
            _authlevel = 0;
        }

        public command(string keyword, string[] responses,string[] aliases)
        {
            _keyword = keyword;
            _responses = responses;
            _aliases = aliases;
            _authlevel = 0;
        }
    }
    public class keycom : com
    {
        int _cooldown = 3;
        int _authlevel = 3;//I have yet to define 3, but let's say 3.

        public keycom(string keyword,int keycode,string response)
        {
            _keyword = keyword;
            _responses[0] = response;
            _aliases = null;
        }

        public keycom(string keyword, int keycode,string response,string[] aliases)
        {
            _keyword = keyword;
            _responses[0] = response;
            _aliases = aliases;
        }

        public keycom(string keyword, int keycode,string[] responses)
        {
            _keyword = keyword;
            _responses = responses;
            _aliases = null;
        }

        public keycom(string keyword, int keycode,string[] responses,string[] aliases)
        {
            _keyword = keyword;
            _responses = responses;
            _aliases = aliases;
        }
    }
}
