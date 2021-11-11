using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Christmas_bot
{
    internal static class TextHandle
    {
        public static bool isEmbedableImage(string url)
        {
            return 
                url.EndsWith(".png") ||
                url.EndsWith(".jpg") ||
                url.EndsWith(".jpeg") ||
                url.EndsWith(".JPG") ||
                url.EndsWith(".JPEG") ||
                url.EndsWith(".gif") ||
                url.EndsWith(".gifv");
        }

        public static string CleanText(string input)
        {
            Regex expression = new Regex("[\b\f\n\r\t\"\\ ]");
            string output = expression.Replace(input, " ");
            
            return new string(output.Take(240).ToArray());
        }
            
    }
}
