﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Christmas_bot
{
    internal static class UrlHandle
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

        //public static string FilterInput()
        //{

        //}
    }
}