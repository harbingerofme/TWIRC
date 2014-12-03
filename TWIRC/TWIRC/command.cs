using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWIRC
{
    class com//allows us to check for coms, I guess. I could have prob made h
    {
    }

    class command : com
    {
                string _keyword;
        string[] _responses;
        string[] _aliases;
        int _cooldown = 20;
        int _lastTime = 0;
        int _authlevel;

        public command(string keyword,string response)
        {
            _cooldown = 20;
            _lastTime = 0;
            _keyword = keyword;
            _responses[0] = response;
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
    class keycom : com
    {
        string _keyword;
        string[] _responses;
        string[] _aliases;
        int _cooldown = 3;
        int _lastTime = 0;
        int _keycode;
        int _authlevel = 3;//I have yet to define 3, but let's say 3.

        public keycom(string keyword,int keycode,string response)
        {
            _cooldown = 20;
            _lastTime = 0;
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
