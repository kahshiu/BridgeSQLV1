using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BridgeSQL
{
    class CustomRepoPath
    {
        // Accuracy: Use server/ DB as key to custom path
        public string Server;
        public string DB;
        public string CustomPath;

        // string fails: component missing {server,db,custompath}
        public static bool ValidateFormat(string target)
        {
            bool failed = target.Length == 0 || !Regex.IsMatch(target, @"\w+\/\w+\>\w+", RegexOptions.IgnoreCase);
            if (failed) return false;

            string path = target.Substring(target.IndexOf('>')+1);
            failed = !Util.ValidatePath(path);
            if (failed) return false;

            return true;
        }

        public static string[] DecompileFullString(string target)
        {
            string[] temp = new string[3];
            int pos = 0;
            pos = target.IndexOf(">");
            if (pos > -1)
            {
                temp[2] = target.Substring(pos+1);
                target = target.Substring(0, pos);
            }
            pos = target.IndexOf("/");
            if (pos > -1)
            {
                temp[1] = target.Substring(pos+1);
                target = target.Substring(0, pos);
            }
            temp[0] = target;
            return temp;
        }

        public CustomRepoPath(string mServer, string mDB, string mCustomPath)
        {
            Server = mServer;
            DB = mDB;
            CustomPath = mCustomPath;
        }

        public CustomRepoPath(string[] decompiled)
        {
            Server = decompiled[0];
            DB = decompiled[1];
            CustomPath = decompiled[2];
        }

        public CustomRepoPath(string fullString)
        {
            string[] decompiled = CustomRepoPath.DecompileFullString(fullString);
            Server = decompiled[0];
            DB = decompiled[1];
            CustomPath = decompiled[2];
        }

        public string FormFullString()
        {
            return string.Format(@"{0}/{1}>{2}", Server, DB, CustomPath);
        }

        // convention: {{server}}/{{db}}>{{custompath}}
        // string chopping from back to front

    }
}
