using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeSQL
{
    class TProcCommands
    {
        public static string Add(string[] paths)
        {
            string total = "/command:add ";
            string temp = @"/path:""{0}""";
            total = total + string.Format(temp, CompileString(paths));
            return total;
        }

        public static string RepoStatus(string path)
        {
            string total = "/command:repostatus ";
            string temp = @"/path:""{0}""";
            total = total + string.Format(temp, path);
            return total;
        }

        public static string Commit(string[] paths, string logMessage = "")
        {
            string total = "/command:commit ";
            string temp = @"/path:""{0}""";
            string temp2 = @"/logmsg:""{0}""";
            total = total + string.Format(temp, CompileString(paths));
            
            if (logMessage != "")
            {
                total = total + string.Format(temp2, logMessage);
            }
            return total;
        }

        //public static string Diff()
        //{

        //}

        //public static string Merge()
        //{

        //}

        public static string ShowLog(string path)
        {
            //additional options will be added as needed
            string total = "/command:log ";
            string temp = @"/path:""{0}""";
            total = total + string.Format(temp, path);
            return total;
        }
        //public static string ShowCompare()
        //{

        //}

        //public static string Blame()
        //{

        //}

        public static string Update(string[] paths, string revision = "")
        {
            string total = "/command:update ";
            string temp = @"/path:""{0}""";
            string temp2 = @"/rev:{0}";
            total = total + string.Format(temp, CompileString(paths));
            if (revision != "")
            {
                total = total + string.Format(temp2, revision);
            }
            return total;
        }

        public static string CompileString(string[] paths, char delimiter = '*')
        {
            string frag = "";
            foreach (string path in paths)
            {
                frag = frag + "*" + path;
            }
            return frag.Length == 0 ? "" : frag.Substring(1);
        }
    }
}
