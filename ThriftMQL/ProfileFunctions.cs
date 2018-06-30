using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ThriftMQL
{
    public class ProfileFunctions
    {
        public const int CHAR_BUFF_SIZE = 512;

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringW", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileStringW(string lpApplicationName, string lpKeyName, string lpDefault,
                                           [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] char[] lpReturnedString, int nSize, string Filename);

        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Unicode)]
        public static extern int WritePrivateProfileStringW2(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);

        public static string GetPrivateProfileString(string fileName, string sectionName, string keyName)
        {
            char[] ret = new char[CHAR_BUFF_SIZE];

            while (true)
            {
                int length = GetPrivateProfileStringW(sectionName, keyName, null, ret, ret.Length, fileName);
                if (length == 0)
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                // This function behaves differently if both sectionName and keyName are null
                if (sectionName != null && keyName != null)
                {
                    if (length == ret.Length - 1)
                    {
                        // Double the buffer size and call again
                        ret = new char[ret.Length * 2];
                    }
                    else
                    {
                        // Return simple string
                        return new string(ret, 0, length);
                    }
                }
                else
                {
                    if (length == ret.Length - 2)
                    {
                        // Double the buffer size and call again
                        ret = new char[ret.Length * 2];
                    }
                    else
                    {
                        // Return multistring
                        return new string(ret, 0, length - 1);
                    }
                }
            }
        }

    }
}
