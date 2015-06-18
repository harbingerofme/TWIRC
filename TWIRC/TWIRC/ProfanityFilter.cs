using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SayingsBot
{
    /// <summary>
    /// Contains Methods and strings to filter profanity.
    /// </summary>
    class ProfanityFilter
    {
#region Profanity String
//Region so I can hide it from those walking by.
        /// <summary>
        /// Convinent string containing the prfanity
        /// </summary>
        string profanityA = "ass|bitch|ballocks|cock|damn|damnit|dike|dyke|fag|faggot|fuck|fuckers|fucking|hell|kanker|retard|shit|tard|wanker";
#endregion
        HarbBot hb = null;

        public ProfanityFilter(HarbBot hb)
        {
            this.hb = hb;
        }

        /// <summary>
        /// The method that filters the profanity.
        /// </summary>
        /// <param name="toFilter">The string to filter.</param>
        /// <returns>True if profanity, else false.</returns>
        public bool isProfanity(string toFilter)
        {
            if (hb.shouldRebuildProf)
            {
                foreach (string build in hb.swearList)
                {
                    profanityA += "|" + build;
                }
            }
            return Regex.Match(toFilter.ToLower(), @"((^(" + profanityA + ")$)|(^(" + profanityA + @")[ ?!\.,\-_])|( (" + profanityA + @")[ ?!\.,\-_])|( (" + profanityA + ")$))", RegexOptions.IgnoreCase).Success;
        }
    }
}
