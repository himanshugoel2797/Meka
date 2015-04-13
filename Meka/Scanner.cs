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
