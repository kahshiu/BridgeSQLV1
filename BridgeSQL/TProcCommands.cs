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

        public static string Diff(string path, string path2="", string title="",string title2 = "", string pathToShow="", string pathToShow2="")
        {
            string total = "/command:diff ";
            string _temp = @"/path:""{0}"" ";
            string _temp2 = @"/path2:""{0}"" ";
            string _title = @"/lefttitle:""{0}"" ";
            string _title2 = @"/righttitle:""{0}"" ";
            string _pathToShow = @"/left:""{0}""";
            string _pathToShow2 = @"/right:""{0}""";

            total = total + string.Format(_temp, path);
            if (path2 != "") { total = total + string.Format(_temp2, path2); }
            if (title != "") { total = total + string.Format(_title, title); }
            if (title2 != "") { total = total + string.Format(_title2, title2); }
            if (pathToShow != "") { total = total + string.Format(_pathToShow, pathToShow); }
            if (pathToShow2 != "") { total = total + string.Format(_pathToShow2, pathToShow2); }

            return total;
        }

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

        public static string Blame(string path)
        {
            string total = "/command:blame ";
            string temp = @"/path:""{0}""";
            total = total + string.Format(temp, path);
            return total;
        }

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
