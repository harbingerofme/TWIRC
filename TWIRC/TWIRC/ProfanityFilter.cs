using System;
using System.Collections.Generic;
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
        string profanityA = "ass|bitch|ballocks|cock|damnit|fag|faggot|fuck|fuckers|fucking|hell|kanker|retard|tard|wanker";
#endregion

        /// <summary>
        /// The method that filters the profanity.
        /// </summary>
        /// <param name="toFilter">The string to filter.</param>
        /// <returns>True if profanity, else false.</returns>
        public bool isProfanity(string toFilter)
        {
            return Regex.Match(toFilter.ToLower(), @"((^(" + profanityA + ")$)|(^(" + profanityA + @")[ ?!\.,\-_])|( (" + profanityA + @")[ ?!\.,\-_])|( (" + profanityA + ")$))", RegexOptions.IgnoreCase).Success;
        }
    }
}
