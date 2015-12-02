using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;
using System.Text.RegularExpressions;

namespace BridgeSQL
{
    public class ManaSQLConfig
    {
        public static string Mode = "general";
        public static string Extension = "sql";
        public static string WinPath = Environment.GetFolderPath(Environment.SpecialFolder.System);

        private static string _RepoPath;
        private static string _ProgPath;
        private static string _TProcPath;

        private static bool _EnableLogging;
        private static bool _ShowExtract;
        private static bool _ShowCompareFile;
        private static bool _SvnRepoStatus;
        private static bool _SvnCommit;
        private static bool _SvnUpdate;
        private static bool _SvnShowLog;
        private static bool _SvnDiff;
        private static bool _SvnMerge;
        private static bool _SvnBlame;

        private static int _SelectedCustomPath = -1;
        private static string _customPathText;
        private static List<string> _customPaths = new List<string>();

        public static FixedSettings Extract = new FixedSettings("extract");
        public static FixedSettings Upload = new FixedSettings("upload");
        public static FixedSettings UploadFile1 = new FixedSettings("uploadFile1");
        public static FixedSettings UploadFile2 = new FixedSettings("uploadFile2");
        public static FixedSettings CompareFile1 = new FixedSettings("compareFile1");
        public static FixedSettings CompareFile2 = new FixedSettings("compareFile2");
        public static FixedSettings CompareDir = new FixedSettings("compareDir");

        public static event ListHandler UpdatedList;
        public delegate void ListHandler(string varname);

        public static event TextHandler UpdatedText;
        public delegate void TextHandler(string varname);

        public static event CheckBoxHandler UpdatedCheckBox;
        public delegate void CheckBoxHandler(string varname);

        public static event NonVisualDataHandler UpdatedNonVisualData;
        public delegate void NonVisualDataHandler(string varname);

        public static void Initialise()
        {
            MSettings.ReadSettings();

            EnableLogging = MSettings.IsLogEnable;
            ShowExtract = MSettings.IsExtract;
            ShowCompareFile = MSettings.IsCompareFile;
            SvnRepoStatus = MSettings.IsSVNRepo;
            SvnCommit = MSettings.IsSVNCommit;
            SvnUpdate = MSettings.IsSVNUpdate;
            SvnShowLog = MSettings.IsSVNShowLog;
            SvnDiff = MSettings.IsSVNDiff;
            SvnMerge = MSettings.IsSVNMerge;
            SvnBlame = MSettings.IsSVNBlame;
            RepoPath = MSettings.GenRepoPath;
            ProgPath = MSettings.GenManaPath;
            TProcPath = MSettings.GenTProcPath;

            ResetCustomPaths();
            AddRangeCustomPaths(MSettings.CustomPaths.ToArray());
            _customPathText = "";
        }

        // START: PROPERTIES OF CHECKBOXES
        public static bool ValidRepoPath { get { return Util.ValidatePath(RepoPath); } }
        public static bool ValidProgPath { get { return Util.ValidatePath(ProgPath, "file", "SqlMana.exe"); } }
        public static bool ValidTProcPath { get { return Util.ValidatePath(TProcPath, "file", "TortoiseProc.exe"); } }
        public static bool ValidGenPaths { get { return ValidRepoPath && ValidProgPath && ValidTProcPath; } }

        public static Boolean EnableLogging
        {
            get { return _EnableLogging; }
            set
            {
                _EnableLogging = value;
                if (UpdatedCheckBox != null)
                    UpdatedCheckBox(Mode + "-EnableLogging");
            }
        }

        public static bool ShowExtract
        {
            get { return _ShowExtract; }
            set
            {
                _ShowExtract = value;
                if (UpdatedCheckBox != null)
                    UpdatedCheckBox(Mode + "-ShowExtract");
            }
        }

        public static bool ShowCompareFile
        {
            get { return _ShowCompareFile; }
            set
            {
                _ShowCompareFile = value;
                if (UpdatedCheckBox != null)
                    UpdatedCheckBox(Mode + "-ShowCompareFile");
            }
        }

        public static bool SvnRepoStatus
        {
            get { return _SvnRepoStatus; }
            set
            {
                _SvnRepoStatus = value;
                if (UpdatedCheckBox != null)
                    UpdatedCheckBox(Mode + "-SvnRepoStatus");
            }
        }

        public static Boolean SvnCommit
        {
            get { return _SvnCommit; }
            set
            {
                _SvnCommit = value;
                if (UpdatedCheckBox != null)
                    UpdatedCheckBox(Mode + "-SvnCommit");
            }
        }

        public static Boolean SvnUpdate
        {
            get { return _SvnUpdate; }
            set
            {
                _SvnUpdate = value;
                if (UpdatedCheckBox != null)
                    UpdatedCheckBox(Mode + "-SvnUpdate");
            }
        }

        public static Boolean SvnShowLog
        {
            get { return _SvnShowLog; }
            set
            {
                _SvnShowLog = value;
                if (UpdatedCheckBox != null)
                    UpdatedCheckBox(Mode + "-SvnShowLog");
            }
        }

        public static Boolean SvnDiff
        {
            get { return _SvnDiff; }
            set
            {
                _SvnDiff = value;
                if (UpdatedCheckBox != null)
                    UpdatedCheckBox(Mode + "-SvnDiff");
            }
        }

        public static Boolean SvnMerge
        {
            get { return _SvnMerge; }
            set
            {
                _SvnMerge = value;
                if (UpdatedCheckBox != null)
                    UpdatedCheckBox(Mode + "-SvnMerge");
            }
        }

        public static Boolean SvnBlame
        {
            get { return _SvnBlame; }
            set
            {
                _SvnBlame = value;
                if (UpdatedCheckBox != null)
                    UpdatedCheckBox(Mode + "-SvnBlame");
            }
        }

        // START: PROPERTIES OF CUSTOM PATH
        public static int SelectCustomPath
        {
            get { return _SelectedCustomPath; }
            set
            {
                _SelectedCustomPath = value;
                if (UpdatedNonVisualData != null)
                    UpdatedNonVisualData(Mode + "-SelectCustomPath");
            }
        }

        public static List<string> GetCustomPaths()
        {
            // check paths first as some might have already been deleted physically after added
            for (int i = 0; i < _customPaths.Count; i++)
            {
                if (!Util.ValidatePath(_customPaths[i]))
                {
                    RemoveCustomPaths(i);
                }
            }
            return _customPaths;
        }

        public static void AddCustomPath(string path, bool trigger = true)
        {
            if (Util.ValidatePath(path) && !Util.Contains(_customPaths.ToArray(), path))
            {
                _customPaths.Add(path);
            }
            if (trigger && UpdatedList != null)
                UpdatedList(Mode + "-CustomPaths");
        }

        public static void AddRangeCustomPaths(string[] paths, bool trigger = true)
        {
            foreach (string path in paths)
            {
                _customPaths.Add(path);
            }
            if (trigger && UpdatedList != null)
                UpdatedList(Mode + "-CustomPaths");
        }

        public static void ResetCustomPaths(bool trigger = true)
        {
            _customPaths.Clear();
            if (trigger && UpdatedList != null)
                UpdatedList(Mode + "-CustomPaths");
        }

        public static void RemoveCustomPaths(int index, bool trigger = true)
        {
            _customPaths.RemoveAt(index);
            if (trigger && UpdatedList != null)
                UpdatedList(Mode + "-CustomPaths");
        }

        public static string CustomPathText
        {
            get { return _customPathText; }
            set
            {
                _customPathText = value;
                if (UpdatedText != null)
                    UpdatedText(Mode + "-CustomPath");
            }
        }

        // START: PROPERTIES OF TEXTBOXES
        public static string RepoPath
        {
            get { return _RepoPath; }
            set
            {
                _RepoPath = value;
                if (UpdatedText != null)
                    UpdatedText(Mode + "-RepoPath");
            }
        }

        public static string ProgPath
        {
            get { return _ProgPath; }
            set
            {
                _ProgPath = value;
                if (UpdatedText != null)
                    UpdatedText(Mode + "-ProgPath");
            }
        }

        public static string TProcPath
        {
            get { return _TProcPath; }
            set
            {
                _TProcPath = value;
                if (UpdatedText != null)
                    UpdatedText(Mode + "-TProcPath");
            }
        }
    }

    public class FixedSettings
    {
        string[] SeqActions =
        {
            "db,file"
            ,"file,db"
            ,"file"
        };
        string[] DBActions =
        {
            "selectSSP"
            ,"updateSSP"
        };
        string[] FileActions =
        {
            "readSSP"
            ,"writeSSP"
            ,"compareSSP"
        };
        string[] Modes =
        {
            "extract"
            ,"upload"
            ,"compareDir"
        };

        //NODE
        //1. Connection string
        public IEnumerable<IConnectionInfo> CONS;
        public IOeNode NODE;
        public string SERVER = "";
        public string DB = "";
        public string OBJ = "";
        public string TYPE = "";

        public IOeNode NODE2;
        public string SERVER2 = "";
        public string DB2 = "";
        public string OBJ2 = "";
        public string TYPE2 = "";

        // 1.0 static items
        private string Mode = "";
        public bool IsActive = false;
        public string LogSuffix = "";
        public string SeqAction = "";
        public string DBAction = "";
        public string FileAction = "";

        // 2.1 variable items
        private Boolean _isStrict = true;
        private Boolean _UseTemp = false;
        private Boolean _IsDefault = true;
        private List<string> WhereSSPStore = new List<string>();
        private List<string> WhereFileStore = new List<string>();

        // 2.2 customisable: defaults
        private string _RepoPath = "";
        private string _RepoPath2 = "";
        private string _LogPath = "";
        private string _OutPath = "";

        public event VariablesHandler UpdatedVariables;
        public delegate void VariablesHandler(string varname);

        public event WhereHandler UpdatedWhere;
        public delegate void WhereHandler(string varname);

        public FixedSettings(string mode)
        {
            Mode = mode;
            UpdateStatics();
        }

        private void UpdateStatics()
        {
            LogSuffix = Mode;

            // actions + sequences
            if (Mode == "extract" || Mode == "compareFile1" || Mode == "compareFile2")
            {
                SeqAction = SeqActions[0];
                DBAction = DBActions[0];
                FileAction = FileActions[1];
            }
            else if (Mode == "upload" || Mode == "uploadFile1" || Mode == "uploadFile2")
            {
                SeqAction = SeqActions[1];
                DBAction = DBActions[1];
                FileAction = FileActions[0];
            }
            else if (Mode == "compareDir")
            {
                SeqAction = SeqActions[2];
                DBAction = "";
                FileAction = FileActions[2];
                _isStrict = false;
            }
        }

        //triggered when changing node selection
        //NODE reflect 
        //1. DatabaseObj (if true will show its details)
        //2. Connection details
        //3. Node name
        public void UpdateVariables(IOeNode newNode, int node = 1, bool trigger = true)
        {
            // setting variables of the new NODE first
            IOeNode currNode;
            string[] newDetails = GetNodeDetails(newNode);
            if (node == 2)
            {
                SERVER2 = newDetails[0];
                DB2 = newDetails[1];
                OBJ2 = newDetails[2];
                TYPE2 = newDetails[3];
                currNode = NODE2;
            }
            else
            {
                SERVER = newDetails[0];
                DB = newDetails[1];
                OBJ = newDetails[2];
                TYPE = newDetails[3];
                currNode = NODE;
            }
            string[] currDetails = GetNodeDetails(currNode);


            if ((Mode == "extract"
                || Mode == "upload"
                || Mode == "compareDir"
                )
                && currNode != null                 //ommit initiation, all items are empty
                && (newDetails[0] != currDetails[0] //changed details
                || newDetails[1] != currDetails[1]
                || newDetails[3] != currDetails[3]
                )
                )
            {
                ResetWhereSSP(false);
                ResetWhereFiles(false);
            }

            if (node == 2) { NODE2 = newNode; }
            else { NODE = newNode; }

            if (trigger && UpdatedVariables != null)
            {
                UpdatedVariables(Mode);
            }
        }

        public bool IsDefault
        {
            get { return _IsDefault; }
            set
            {
                _IsDefault = value;
                if (_IsDefault == true) { _UseTemp = false; }
                if (UpdatedVariables != null)
                    UpdatedVariables(Mode + "-IsDefault");
            }
        }

        public bool UseTemp
        {
            get { return _UseTemp; }
            set
            {
                _UseTemp = value;
                if (_UseTemp == true) { _IsDefault = false; }
                if (UpdatedVariables != null)
                    UpdatedVariables(Mode + "-IsTemp");
            }
        }

        // read path, check if directory exists base on Mode
        public bool ValidRepoPath
        {
            get
            {
                bool flag = false;

                if (Mode == "compareDir"
                    || Mode == "upload"
                    || Mode == "uploadFile1"
                    || Mode == "uploadFile2"
                    )
                {
                    if (FormRepoPath() == "") return flag;
                    flag = Util.ValidatePath(FormRepoPath())
                          && Util.HasFile(FormRepoPath(), ManaSQLConfig.Extension);
                }
                else if (Mode == "compareFile1"
                    || Mode == "compareFile2"
                    || Mode == "extract")
                { flag = FormRepoPath() != ""; }
                return flag;
            }
        }

        // read path, check if directory exists
        public bool ValidRepoPath2
        {
            get
            {
                bool flag = false;

                if (Mode == "compareDir")
                {
                    if (FormRepoPath2() == "") return flag;
                    flag = Util.ValidatePath(FormRepoPath2())
                        && Util.HasFile(FormRepoPath2(), ManaSQLConfig.Extension);
                }
                else if (Mode == "compareFile1"
                    || Mode == "compareFile2"
                    || Mode == "uploadFile2"
                    || Mode == "uploadFile1"
                    || Mode == "extract"
                    || Mode == "upload")
                { flag = true; }
                return flag;
            }
        }

        // written path, no need to check if directory exists
        public bool ValidLogPath
        {
            get
            {
                bool flag = false;
                if (Mode == "compareDir"
                    || Mode == "compareFile1"
                    || Mode == "compareFile2"
                    || Mode == "uploadFile1"
                    || Mode == "uploadFile2"
                    || Mode == "extract"
                    || Mode == "upload")
                { flag = !ManaSQLConfig.EnableLogging || (ManaSQLConfig.EnableLogging && FormLogPath() != ""); }
                return flag;
            }
        }

        // written path, no need to check if directory exists
        public bool ValidOutPath
        {
            get
            {
                bool flag = false;
                if (Mode == "compareDir") { flag = FormOutPath() != ""; }
                else if (Mode == "compareFile1"
                    || Mode == "compareFile2"
                    || Mode == "uploadFile1"
                    || Mode == "uploadFile2"
                    || Mode == "extract"
                    || Mode == "upload")
                { flag = true; }
                return flag;
            }
        }

        public bool ValidPaths
        {
            get
            {
                bool flag = false;
                flag = ValidRepoPath && ValidRepoPath2 && ValidLogPath && ValidOutPath;
                return flag;
            }
        }

        // hide create directory buttons
        // decided there's no need for these, at the moment
        public bool ValidCreateRepoPath { get { return false; } }
        public bool ValidCreateRepoPath2 { get { return false; } }
        public bool ValidCreateLogPath { get { return false; } }
        public bool ValidCreateOutPath { get { return false; } }

        public string RepoPath
        {
            get { return _RepoPath; }
            set
            {
                _RepoPath = value;
                if (Mode == "compareFile1") ManaSQLConfig.UploadFile1.RepoPath = value;
                else if (Mode == "compareFile2") ManaSQLConfig.UploadFile2.RepoPath = value;

                if (UpdatedVariables != null)
                    UpdatedVariables(Mode + "-RepoPath");
            }
        }

        public string RepoPath2
        {
            get { return _RepoPath2; }
            set
            {
                _RepoPath2 = value;
                if (UpdatedVariables != null)
                    UpdatedVariables(Mode + "-RepoPath2");
            }
        }
        public string LogPath
        {
            get { return _LogPath; }
            set
            {
                _LogPath = value;
                if (Mode == "compareFile1") ManaSQLConfig.UploadFile1.LogPath = value;
                else if (Mode == "compareFile2") ManaSQLConfig.UploadFile2.LogPath = value;

                if (UpdatedVariables != null)
                    UpdatedVariables(Mode + "-LogPath");
            }
        }

        public string OutPath
        {
            get { return _OutPath; }
            set
            {
                _OutPath = value;
                if (UpdatedVariables != null)
                    UpdatedVariables(Mode + "-OutPath");
            }
        }

        private string[] GetNodeDetails(IOeNode theNode)
        {
            IConnectionInfo CON;
            IDatabaseObjectInfo DBI;

            string dbname = "";
            string servername = "";
            string objname = "";
            string objtype = "";
            bool validDBI = false;

            if (theNode != null)
            {
                if (theNode.HasConnection && theNode.TryGetConnection(out CON))
                {
                    servername = CON.Server;
                    validDBI = theNode.TryGetDatabaseObject(out DBI);

                    if (Mode == "extract" || Mode == "upload" || Mode == "compareDir")
                    {
                        if (theNode.Type == "StoredProcedures")
                        {
                            dbname = theNode.Name;
                            objtype = "StoredProcedures";
                        }
                        else if (theNode.Type == "StoredProcedure" && validDBI)
                        {
                            dbname = DBI.DatabaseName;
                            objtype = "StoredProcedures";
                        }
                    }
                    else if (Mode == "compareFile1" 
                        || Mode == "compareFile2" 
                        || Mode == "uploadFile1"
                        || Mode == "uploadFile2"
                        )
                    {
                        //if (validDBI)
                        if (theNode.Type == "StoredProcedure" && validDBI)
                        {
                            dbname = DBI.DatabaseName;
                            objname = DBI.ObjectName;
                            objtype = "StoredProcedures";
                        }
                    }
                }
            }
            return new string[4] { servername, dbname, objname, objtype };
        }

        //triggered when opening/ closing new connection
        //all available connections == theCONS
        public void UpdateCON(IEnumerable<IConnectionInfo2> theCONS)
        {
            CONS = theCONS;
        }

        public List<string> WhereSSPList
        {
            get { return WhereSSPStore; }
        }

        public string WhereSSPString
        {
            get
            {
                string temp = "";
                foreach (string ssp in WhereSSPStore)
                {
                    temp = temp + string.Format(@",{0}", ssp);
                }
                return temp.Length == 0 ? temp : temp.Substring(1);
            }
        }

        public List<string> WhereSSPToFilename
        {
            get
            {
                List<string> equivalentFilename = new List<string>();
                foreach (string ssp in WhereSSPStore)
                {
                    equivalentFilename.Add(string.Format(@"{0}.{1}", ssp, ManaSQLConfig.Extension));
                }
                return equivalentFilename;
            }
        }

        public bool ExistsSSPFilename(int index = -1)
        {
            bool isExist = false;
            List<string> temp = WhereSSPToFilename;

            // check index only
            if (index > -1)
            {
                if (temp.Count == 0)
                {
                    isExist = false;
                    return isExist;
                }
                isExist = Util.ValidatePath(string.Format(@"{0}\{1}", GetRepoPath(), temp[index]), "file");
            }

            // index -1 indicates all
            else
            {
                foreach (string currFilename in temp)
                {
                    isExist = Util.ValidatePath(string.Format(@"{0}\{1}", GetRepoPath(), temp[index]), "file");
                    if (!isExist) break;
                }
            }
            return isExist;
        }

        public List<string> WhereFileList
        {
            get { return WhereFileStore; }
        }

        public string WhereFileString
        {
            get
            {
                string temp = "";
                foreach (string ssp in WhereFileStore)
                {
                    temp = temp + string.Format(@",{0}", ssp);
                }
                return temp.Length == 0 ? temp : temp.Substring(1);
            }
        }

        public void AppendWhereSSP(string objname, bool trigger = true)
        {
            if (!Util.Contains(WhereSSPStore.ToArray(), objname))
                WhereSSPStore.Add(objname);
            if (trigger && UpdatedWhere != null)
                UpdatedWhere(Mode + "-WhereSSPStore");
        }

        public void ResetWhereSSP(bool trigger = true)
        {
            WhereSSPStore.Clear();
            if (trigger && UpdatedWhere != null)
                UpdatedWhere(Mode + "-WhereSSPStore");
        }

        public void AppendWhereFile(string objname, bool trigger = true)
        {
            if (!Util.Contains(WhereFileStore.ToArray(), objname, "i")
                && Util.ValidatePath(
                    string.Format(@"{0}\{1}", GetRepoPath(), objname)
                    , "file")
                )
                WhereFileStore.Add(objname);
            if (trigger && UpdatedWhere != null)
                UpdatedWhere(Mode + "-WhereFileStore");
        }

        public void ResetWhereFiles(bool trigger = true)
        {
            WhereFileStore.Clear();
            if (trigger && UpdatedWhere != null)
                UpdatedWhere(Mode + "-WhereFileStore");
        }

        public void RemoveWhereFiles(string objname, bool trigger = true)
        {
            foreach (string temp in WhereFileStore)
            {
                if (temp == objname)
                {
                    WhereFileStore.Remove(temp);
                    break;
                }
            }
            if (trigger && UpdatedWhere != null)
                UpdatedWhere(Mode + "-WhereFileStore");
        }

        public string FormTags()
        {
            List<string> paths = new List<string>();
            if (SERVER != "") paths.Add(SERVER);
            if (DB != "") paths.Add(DB);
            if (OBJ != "") paths.Add(OBJ);
            return string.Join("_", paths);
        }

        public string FormTags2()
        {
            List<string> paths = new List<string>();
            if (SERVER2 != "") paths.Add(SERVER2);
            if (DB2 != "") paths.Add(DB2);
            if (OBJ2 != "") paths.Add(OBJ2);
            return string.Join("_", paths);
        }

        public string FormRepoPath()
        {
            string path = "";
            string subdir = "";
            bool isStrict = (IsDefault || UseTemp) ? true : _isStrict;
            bool hasDB = (SERVER != "" && DB != "");

            if (TYPE == "StoredProcedures")
                subdir = TYPE;
            else if (TYPE == "UserDefinedFunctions")
                subdir = TYPE;

            if ((isStrict && hasDB) || !isStrict)
            {
                if (IsDefault)
                {
                    path = string.Format(@"{0}\{1}\{2}", ManaSQLConfig.RepoPath, SERVER, DB);
                    if (subdir != "") path = string.Format(@"{0}\{1}", path, subdir);
                }
                else if (UseTemp)
                {
                    path = string.Format(@"{0}\{1}\{2}", ManaSQLConfig.RepoPath, "tempServer", DB);
                    if (subdir != "") path = string.Format(@"{0}\{1}", path, subdir);
                }
                else
                {
                    path = _RepoPath;
                }
            }

            return path;
        }

        // currently only compareDir uses this
        public string FormRepoPath2()
        {
            string path = "";
            path = _RepoPath2;
            return path;
        }

        public string FormLogPath()
        {
            string path = "";
            bool isStrict = (IsDefault || UseTemp) ? true : _isStrict;
            bool hasDB = (SERVER != "" && DB != "");

            if ((isStrict && hasDB) || !isStrict)
            {
                if (Mode == "compareDir")
                {
                    if (IsDefault)
                    {
                        path = ManaSQLConfig.RepoPath;
                    }
                    else
                    {
                        path = _LogPath;
                    }
                }
                else
                {
                    if (IsDefault)
                    {
                        path = string.Format(@"{0}\{1}\{2}", ManaSQLConfig.RepoPath, SERVER, "log");
                    }
                    else if (UseTemp)
                    {
                        path = string.Format(@"{0}\{1}\{2}", ManaSQLConfig.RepoPath, "tempServer", "log");
                    }
                    else
                    {
                        path = _LogPath;
                    }
                }
            }
            return path;
        }

        // currently only compareDir uses this
        public string FormOutPath()
        {
            string path = "";
            if (IsDefault)
            {
                path = ManaSQLConfig.RepoPath;
            }
            else
            {
                path = _OutPath;
            }
            return path;
        }

        public List<string> FormSelectedSSPFilePaths()
        {
            List<string> total = new List<string>();
            foreach (string ssp in WhereSSPStore)
            {
                total.Add(GetRepoPath(string.Format(@"{0}.{1}", ssp, ManaSQLConfig.Extension)));
            }
            return total;
        }

        public string GetRepoPath(string filename = "")
        {
            string path = FormRepoPath();
            if (filename != "") path = string.Format(@"{0}\{1}", path, filename);
            return path;
        }

        // currently only compareDir uses this
        public string GetRepoPath2(string filename = "")
        {
            string path = FormRepoPath2();
            if (filename != "") path = string.Format(@"{0}\{1}", path, filename);
            return path;
        }

        public string GetLogPath(string filename = "")
        {
            string path = FormLogPath();
            if (path == "") return path;
            if (Mode == "compareDir")
            {
                path = string.Format(@"{0}\{1}\{2}", path, "compared", AutomationDate);
            }
            if (filename != "") path = string.Format(@"{0}\{1}", path, filename);
            return path;
        }

        // currently only compareDir uses this
        public string GetOutPath(string filename = "", string subdir = "")
        {
            string path = FormOutPath();
            if (path == "") return path;

            path = string.Format(@"{0}\{1}\{2}", path, "compared", AutomationDate);
            if (filename == "") filename = "result.txt";
            if (filename != "") path = string.Format(@"{0}\{1}", path, filename);
            return path;
        }

        public string AutomationDate = "[Automation]";
        public string GenAutomationDate()
        {
            AutomationDate = DateTime.Now.ToOADate().ToString();
            return AutomationDate;
        }
        public void InvalidateAutomationDate()
        {
            AutomationDate = "[Automation]";
        }
        // Unsuccessful: force string to return ""
        // Successful: return appropriate string
        public string CompileArgs(int variant = 0, string template = "\"{0}|{1}\" ")
        {
            Boolean success = false;
            string compiled = "";
            IConnectionInfo CON;

            if (NODE.HasConnection && NODE.TryGetConnection(out CON))
            {
                //Generic settings
                Regex reg = new Regex("\"");

                compiled = compiled + string.Format(template, "SeqAction", reg.Replace(SeqAction, "\\\""));
                compiled = compiled + string.Format(template, "DBAction", reg.Replace(DBAction, "\\\""));
                compiled = compiled + string.Format(template, "FileAction", reg.Replace(FileAction, "\\\""));

                compiled = compiled + string.Format(template, "LogPath", reg.Replace(GetLogPath(), "\\\""));
                compiled = compiled + string.Format(template, "LogSuffix", reg.Replace(LogSuffix, "\\\""));
                compiled = compiled + string.Format(template, "LogKill", ManaSQLConfig.EnableLogging ? "0" : "1");

                if (Mode == "compareDir")
                {
                    string subdir = string.Format(
                        @"{0}_{1}_{2}"
                        , FormTags()
                        , FormTags2()
                        , AutomationDate
                    );
                    compiled = compiled + string.Format(template, "RepoPath", reg.Replace(GetRepoPath(), "\\\""));
                    compiled = compiled + string.Format(template, "Repo2Path", reg.Replace(GetRepoPath2(), "\\\""));
                    compiled = compiled + string.Format(template, "LogPath", reg.Replace(GetLogPath(), "\\\""));
                    compiled = compiled + string.Format(template, "OutPath", reg.Replace(GetOutPath(), "\\\""));
                    success = true;
                }
                else
                {
                    compiled = compiled + string.Format(template, "AuthString", reg.Replace(CON.ConnectionString, "\\\""));
                    compiled = compiled + string.Format(template, "RepoPath", reg.Replace(GetRepoPath(), "\\\""));

                    if (Mode == "extract" || Mode == "compareFile1" || Mode == "compareFile2")
                    {
                        // extract selected SSPs 
                        if (variant == 0)
                        {
                            compiled = compiled + string.Format(template, "SqlWhere", WhereSSPString);
                            success = true;
                        }
                        // extract all SSPs
                        else if (variant == 1)
                        {
                            success = true;
                        }
                        else
                        {
                            success = false;
                        }
                    }
                    else if (Mode == "upload" || Mode == "uploadFile1" || Mode == "uploadFile2")
                    {
                        string temp = "";
                        foreach (string filename in WhereFileStore)
                        {
                            temp = temp + "," + filename;
                        }
                        if (temp.Length > 1)
                        {
                            compiled = compiled + string.Format(template, "InData", temp.Length == 0 ? "" : temp.Substring(1));
                            success = true;
                        }
                    }
                }
            }

            if (!success) compiled = "";

            return compiled;
        }
    }
}
