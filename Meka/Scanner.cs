using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meka
{
    public class Scanner
    {
        public string Source;
        public int Position;

        public bool EndOfString
        {
            get
            {
                return Position == Source.Length;
            }
        }

        public Scanner(string src)
        {
            Source = CleanFile(src);
            Position = -1;
        }

        static string CleanFile(string file)
        {
            //Remove bad characters
            //file = Replace(file, "\n", "\r");

            return file;
        }

        #region String extensions
        static string Replace(string obj, params string[] vals)
        {
            for (int i = 0; i < vals.Length; i++)
            {
                obj = obj.Replace(vals[i], string.Empty);
            }
            return obj;
        }
        #endregion

        public char? GetNextCharacter()
        {
            if (Position + 1 < Source.Length)
            {
                return Source[++Position];
            }
            return null;
        }

        public bool StringMatches(string str)
        {
            return Source.Substring(Position).StartsWith(str);
        }

        public bool StringMatchesWord(string str)
        {
            string[] subStr = Source.Substring(Position).Trim().Split(null);        //Trim the substring and then split via whitespace
            if (subStr.Length > 0)  //Make sure there is a first part to check and then check if it's the same as the expected string
            {
                return (subStr[0].Trim() == str);
            }
            return false;   //If nothing works, return false
        }

        public bool CurrentWordContains(string str)
        {
            string[] subStr = Source.Substring(Position).Trim().Split(null);        //Trim the substring and then split via whitespace
            if (subStr.Length > 0)  //Make sure there is a first part to check and then check if it contains the specified string
            {
                return (subStr[0].Trim().Contains(str));
            }
            return false;   //If nothing works, return false
        }

        public bool StringMatchesWordTerminator(string str, params char[] terminators)
        {
            //Check if the str matches
            int i = 0;
            for (i = Position; i < Position + str.Length; i++)
            {
                if (Source[i] != str[i - Position]) return false;
            }

            //Check if one of the expected terminators occurs next, ignoring all whitespace
            while (i < Source.Length)
            {
                if (!char.IsWhiteSpace(Source[i]))
                {
                    return terminators.Contains(Source[i]);
                }
                i++;
            }

            //Default to false
            return false;
        }

        public char? GetRelativeNthCharacter(int chars)
        {
            if (Position + chars < Source.Length)
            {
                Position += chars;
                return Source[Position];
            }
            return null;
        }

        public char? PeekNextCharacter()
        {
            return PeekRelativeNthCharacter(1);
        }

        public char? PeekRelativeNthCharacter(int chars)
        {
            if (Position + chars < Source.Length)
            {
                return Source[Position + chars];
            }
            return null;
        }

    }
}
