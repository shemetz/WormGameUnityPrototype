using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Methods
{
    public class Methods
    {

    }

    public static partial class CoolExtensions
    {
        /// <summary>
        /// the JAVA version of substring. with start+end, instead of start+length!
        public static string substring(this string s, int start, int end)
        {
            return s.Substring(start, end - start);
        }
        /// <summary>
        /// Actually identical to string.substring(start) :P
        public static string substring(this string s, int start)
        {
            return s.Substring(start);
        }
    }
}