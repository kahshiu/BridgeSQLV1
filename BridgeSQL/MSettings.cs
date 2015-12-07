using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;

namespace BridgeSQL
{
    public class MSettings
    {
        private static string SettingPath;
        private static string SettingFile = "BrideSQL.settings";

        public static bool IsInit = true;
        public static bool IsExtract = true;
        public static bool IsCompareFile = true;
        public static bool IsLogEnable = true;
        public static bool IsSVNRepo = true;
        public static bool IsSVNCommit = true;
        public static bool IsSVNUpdate = true;
        public static bool IsSVNShowLog = true;
        public static bool IsSVNDiff = true;
        public static bool IsSVNMerge = true;
        public static bool IsSVNBlame = true;
        public static string GenRepoPath = @"c:\repoSSP";
        public static string GenManaPath = @"C:\SSMS_plugins\SQLMana\SqlMana.exe";
        public static string GenTProcPath = @"c:\Program Files\TortoiseSVN\bin\TortoiseProc.exe";
        public static List<string> CustomPaths = new List<string>();
        public static int ServerNamingIndex = 1;
        public static int LogNamingIndex = 0;

        public static void ReadBridgeSQLRegKey()
        {
            string Win32 = @"SOFTWARE\Red Gate\SIPFramework\Plugins";
            string Win64 = @"\SOFTWARE\Wow6432Node\Red Gate\SIPFramework\Plugins";
            RegistryKey mainKey;
            mainKey = Registry.LocalMachine.OpenSubKey(Win64);
            if (mainKey == null) mainKey = Registry.LocalMachine.OpenSubKey(Win32);
            SettingPath = mainKey.GetValue("BridgeSQL") as string;
            SettingPath = Path.GetDirectoryName(SettingPath);
        }

        public static void ReadSettings()
        {
            ReadBridgeSQLRegKey();
            string filePath = string.Format(@"{0}\{1}", SettingPath, SettingFile);
            string fileLine = "";
            string originalValue = "";
            string[] pair;
            IsInit = !File.Exists(filePath);
            if (!IsInit)
            {
                using (StreamReader file = new StreamReader(filePath))
                {
                    while ((fileLine = file.ReadLine()) != null)
                    {
                        if (fileLine != "")
                        {
                            pair = fileLine.Split('|');
                            //if (pair.Length != 2) continue;

                            originalValue = pair[1].Trim();
                            pair[0] = pair[0].Trim().ToLower();
                            pair[1] = pair[1].Trim().ToLower();
                            if (pair[0] == "" || pair[1] == "") continue;

                            if (pair[0] == "extract") { Boolean.TryParse(pair[1], out IsExtract); }
                            else if (pair[0] == "comparefile") { Boolean.TryParse(pair[1], out IsCompareFile); }
                            else if (pair[0] == "logenable") { Boolean.TryParse(pair[1], out IsLogEnable); }
                            else if (pair[0] == "svnrepo") { Boolean.TryParse(pair[1], out IsSVNRepo); }
                            else if (pair[0] == "svncommit") { Boolean.TryParse(pair[1], out IsSVNCommit); }
                            else if (pair[0] == "svnupdate") { Boolean.TryParse(pair[1], out IsSVNUpdate); }
                            else if (pair[0] == "svnshowlog") { Boolean.TryParse(pair[1], out IsSVNUpdate); }
                            else if (pair[0] == "svndiff") { Boolean.TryParse(pair[1], out IsSVNShowLog); }
                            else if (pair[0] == "svnmerge") { Boolean.TryParse(pair[1], out IsSVNDiff); }
                            else if (pair[0] == "svnblame") { Boolean.TryParse(pair[1], out IsSVNMerge); }
                            else if (pair[0] == "repopath") { GenRepoPath = originalValue; }
                            else if (pair[0] == "manapath") { GenManaPath = originalValue; }
                            else if (pair[0] == "tprocpath") { GenTProcPath = originalValue; }
                            else if (pair[0] == "custompaths")
                            {
                                for (int i = 1; i < pair.Length; i++)
                                {
                                    if (Util.ValidatePath(pair[i]))
                                    {
                                        CustomPaths.Add(i == 1 ? originalValue : pair[i]);
                                    }
                                }
                            }
                            else if (pair[0] == "servernamingindex") {
                                bool isSuccessParse = Int32.TryParse(pair[1], out ServerNamingIndex);
                                if (!isSuccessParse) ServerNamingIndex = 0;
                            }
                            else if (pair[0] == "lognamingindex")
                            {
                                bool isSuccessParse = Int32.TryParse(pair[1], out LogNamingIndex);
                                if (!isSuccessParse) LogNamingIndex = 0;
                            }
                        }
                    }
                    file.Close();
                }
            }
        }

        public static void UpdateSettings()
        {
            IsExtract = ManaSQLConfig.ShowExtract;
            IsCompareFile = ManaSQLConfig.ShowCompareFile;
            IsLogEnable = ManaSQLConfig.EnableLogging;
            IsSVNRepo = ManaSQLConfig.SvnRepoStatus;
            IsSVNCommit = ManaSQLConfig.SvnCommit;
            IsSVNUpdate = ManaSQLConfig.SvnUpdate;
            IsSVNShowLog = ManaSQLConfig.SvnShowLog;
            IsSVNDiff = ManaSQLConfig.SvnDiff;
            IsSVNMerge = ManaSQLConfig.SvnMerge;
            IsSVNBlame = ManaSQLConfig.SvnBlame;
            GenRepoPath = ManaSQLConfig.RepoPath;
            GenManaPath = ManaSQLConfig.ProgPath;
            GenTProcPath = ManaSQLConfig.TProcPath;
            ServerNamingIndex = ManaSQLConfig.ServerNamingIndex;
            LogNamingIndex = ManaSQLConfig.LogNamingIndex;
            CustomPaths.Clear();
            CustomPaths.AddRange(ManaSQLConfig.GetCustomPaths());
        }

        public static void SaveSettings()
        {
            string filePath = string.Format(@"{0}\{1}", SettingPath, SettingFile);
            // form string 
            string total = "";
            total = total + FormString("extract", IsExtract ? "true" : "false");
            total = total + FormString("comparefile", IsCompareFile ? "true" : "false");
            total = total + FormString("logenable", IsLogEnable ? "true" : "false");
            total = total + FormString("svnrepo", IsSVNRepo ? "true" : "false");
            total = total + FormString("svncommit", IsSVNCommit ? "true" : "false");
            total = total + FormString("svnupdate", IsSVNUpdate ? "true" : "false");
            total = total + FormString("svnshowlog", IsSVNShowLog ? "true" : "false");
            total = total + FormString("svndiff", IsSVNDiff ? "true" : "false");
            total = total + FormString("svnmerge", IsSVNMerge ? "true" : "false");
            total = total + FormString("svnblame", IsSVNBlame ? "true" : "false");
            total = total + FormString("repopath", GenRepoPath);
            total = total + FormString("manapath", GenManaPath);
            total = total + FormString("tprocpath", GenTProcPath);
            total = total + FormString("custompaths", CustomPaths.ToArray());
            total = total + FormString("servernamingindex", ServerNamingIndex.ToString());
            total = total + FormString("lognamingindex", LogNamingIndex.ToString());

            File.WriteAllText(filePath, total);
        }

        private static string FormString(string name, string[] vals)
        {
            string temp = "";
            foreach (string val in vals)
            {
                temp = temp + "|" + val;
            }
            return string.Format(@"{0}|{1}{2}", name, temp.Length == 0 ? temp : temp.Substring(1), Environment.NewLine);
        }
        private static string FormString(string name, string val)
        {
            return string.Format(@"{0}|{1}{2}", name, val, Environment.NewLine);
        }
    }
}
