using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeSQL
{
    public class CustomRepoPathManager
    {
        private List<CustomRepoPath> _customRepoPaths;
        private int _indexSelected;

        public CustomRepoPathManager()
        {
            _customRepoPaths = new List<CustomRepoPath>();
            _indexSelected = -1;
        }

        public bool IsDuplicate(string server, string db)
        {
            bool duped = false;
            foreach (CustomRepoPath temp in _customRepoPaths)
            {
                duped = (temp.Server == server) && (temp.DB == db);
                if (duped) break;
            }
            return duped;
        }

        public int AddPath(string fullString)
        {
            int index = -1;
            if (CustomRepoPath.ValidateFormat(fullString))
            {
                string[] temp = CustomRepoPath.DecompileFullString(fullString);
                if (!IsDuplicate(temp[0], temp[1]))
                {
                    _customRepoPaths.Add(new CustomRepoPath(fullString));
                    index = _customRepoPaths.Count - 1;
                }
            }
            return index;
        }

        public int AddPaths(string bigString, char delim = '|')
        {
            string[] mediumString = bigString.Split('|');
            foreach (string med in mediumString)
            {
                AddPath(med);
            }
            return _customRepoPaths.Count;
        }

        public string SerializePaths()
        {
            string total = "";
            foreach (CustomRepoPath temp in _customRepoPaths)
            {
                total = total + temp.FormFullString();
            }
            return total;
        }

        public List<string> ListPaths()
        {
            List<string> total = new List<string>();
            foreach (CustomRepoPath temp in _customRepoPaths)
            {
                total.Add(temp.FormFullString());
            }
            return total;
        }

        public string[] ArrayPaths()
        {
            string[] total = new string[_customRepoPaths.Count];
            for (int i = 0; i < _customRepoPaths.Count; i++)
            {
                total[i] = _customRepoPaths[i].FormFullString();
            }
            return total;
        }

        public void Clear()
        {
            _customRepoPaths.Clear();
        }

        public void SelectAt (int index)
        {
            _indexSelected = index;
        }

        public void KillSelected()
        {
            if (_indexSelected > -1) _customRepoPaths.RemoveAt(_indexSelected);
            _indexSelected = -1;
        }

        public string SearchBy(string server, string db)
        {
            string path = null, translatedServer;

            foreach (CustomRepoPath temp in _customRepoPaths)
            {
                if (ManaSQLConfig.ServerNamingIndex == 1) { translatedServer = Util.GetMachine(temp.Server); }
                else if (ManaSQLConfig.ServerNamingIndex == 2) { translatedServer = Util.GetIP(temp.Server); }
                else translatedServer = temp.Server;

                if (translatedServer == server && temp.DB == db)
                {
                    path = temp.CustomPath;
                    break;
                }
            }

            return path;
        }

        //public void 
    }
}
