using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BridgeSQL
{
    class Util
    {
        public static string[] SanitizeStringList(string list, char delim = ',')
        {
            string[] fragments = list.Split(delim);
            for (int i = 0; i < fragments.Length; i++)
            {
                fragments[i] = fragments[i].Trim();
            }
            return fragments;
        }

        public static bool ValidateCreateDir(string path)
        {
            bool result = false;

            if (path == "") return result;
            if (Directory.Exists(path)) return result;

            // custom minor checking
            DirectoryInfo dInfo = new DirectoryInfo(path);
            result = dInfo.Parent != null;

            return result;
        }

        public static bool ValidatePath(string path, string type = "dir", string filename = "")
        {
            bool result = false;

            if (path == "") return result;

            if (type == "dir")
            {
                if (!Directory.Exists(path)) return result;

                // custom minor checking
                DirectoryInfo dInfo = new DirectoryInfo(path);
                result = dInfo.Parent != null;
            }
            else if (type == "file")
            {
                if (!File.Exists(path)) return result;

                // custom minor checking
                result = (filename == "" || filename == Path.GetFileName(path));
            }
            return result;
        }

        public static bool Contains(string[] array, string item, string options = "")
        {
            bool flag = false;
            foreach (string current in array)
            {
                if (current == item
                    || (options.Contains("i") && current.ToLower() == item.ToLower())
                    )
                {
                    flag = true;
                    break;
                }
            }

            return flag;
        }

        public static int IndexOf(string[] array, string item, string options = "")
        {
            int index = -1;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == item
                    || (options.Contains("i") && array[i].ToLower() == item.ToLower())
                    )
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
    }
}
