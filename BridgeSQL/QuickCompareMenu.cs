using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;

namespace BridgeSQL
{
    public class QuickCompareMenu
    {
        public string Server;
        public string DB;
        public IConnectionInfo2 Conn = null;

        public QuickCompareMenu(string mServer, string mDB)
        {
            Server = mServer;
            DB = mDB;
        }

    }
}
