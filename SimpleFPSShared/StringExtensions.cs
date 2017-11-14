using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleFPSShared
{
    public static class StringExtensions
    {
        public static byte[] To20Char(this string text)
        {
            var chars = text.ToCharArray();
            byte[] bytes = new byte[20];
            for (int i = 0; i < chars.Length; i++)
            {
                bytes[i] = (byte)chars[i];
            }
            return bytes;
        }

        public static string From20Char(this byte[] bytes, int offset)
        {
            char[] chars = new char[20];

            int i = offset;
            for (; i < offset + 20 && i < bytes.Length; i++)
            {
                if (bytes[i] == 0x00)
                {
                    break;
                }

                chars[i - offset] = (char)bytes[i];
            }

            return new string(chars, 0, i - offset);
        }
    }
}
