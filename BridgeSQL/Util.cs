using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

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

        public static List<string> GetFilesWithExtension(string directory, string fileExtension, bool isFilenameOnly = true)
        {
            List<string> files = new List<string>();
            string regex1 = string.Format("*.{0}", fileExtension);
            string regex2 = string.Format(".{0}$", fileExtension);

            if (!Directory.Exists(directory)) return files;
            files = Directory.GetFiles(directory, regex1).ToList();

            for (int i = files.Count - 1; i > -1; i--)
            {
                Regex regex = new Regex(regex2);
                if (regex.Match(files[i]).Success)
                {
                    files[i] = Path.GetFileName(files[i]);
                }
                else
                {
                    files.RemoveAt(i);
                }
            }
            return files;
        }

        public static bool HasFile(string directory, string fileExtension)
        {
            bool flag = false;
            string[] files;
            string regex1 = string.Format("*.{0}", fileExtension);
            string regex2 = string.Format(".{0}$", fileExtension);

            if (!Directory.Exists(directory)) return false;
            files = Directory.GetFiles(directory, regex1);

            foreach (string file in files)
            {
                Regex regex = new Regex(regex2);
                flag = regex.Match(file).Success;
                if (flag) break;
            }
            return flag;
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
