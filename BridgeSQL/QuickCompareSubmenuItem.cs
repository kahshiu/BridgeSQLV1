using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;
using System.Windows.Forms;
using System.Data.SqlClient;
using SqlMana;

namespace BridgeSQL
{
    public class QuickCompareSubmenuItem : ActionSimpleOeMenuItemBase
    {
        private readonly ISsmsFunctionalityProvider6 plug;
        private ManaSQLCommand cmd;
        private QuickCompareMenu q;

        public QuickCompareSubmenuItem(ISsmsFunctionalityProvider6 mPlug, ManaSQLCommand mCmd, QuickCompareMenu mq)
        {
            q = mq;
            plug = mPlug;
            cmd = mCmd;
        }

        public override bool AppliesTo(ObjectExplorerNodeDescriptorBase oeNode)
        {
            IOeNode theNode = (IOeNode)oeNode;
            IDatabaseObjectInfo DBI;
            IConnectionInfo CON;
            bool hideFlag = true;
            bool validServer = false;
            if (theNode.IsDatabaseObject
                && theNode.TryGetDatabaseObject(out DBI)
                && theNode.TryGetConnection(out CON)
                )
            {
                validServer = q.Server == CON.Server
                    || Util.GetMachine(q.Server) == CON.Server
                    || Util.GetIP(q.Server) == CON.Server;
                hideFlag = (validServer && q.DB == DBI.DatabaseName) 
                    || q.Conn == null
                    || !ManaSQLConfig.IsAllowedSingleNode(theNode)
                    ;
            }
            return !hideFlag;
        }

        public override string ItemText
        {
            get { return q.DB; }
        }

        public override void OnAction(ObjectExplorerNodeDescriptorBase oeNode)
        {
            IOeNode theNode = (IOeNode)oeNode;
            IDatabaseObjectInfo DBI;
            IConnectionInfo CON;

            string tAuthString, tScript, wMessage = "", args, scriptBase = "";
            int tCount;

            if (theNode.IsDatabaseObject
                && theNode.TryGetDatabaseObject(out DBI)
                && theNode.TryGetConnection(out CON)
                )
            {
                tAuthString = Util.FormAuthString(q.Conn.ConnectionString, q.DB);
                if(theNode.Type == "StoredProcedure")
                {
                    scriptBase = SQLScripts.PeepSSP;
                }
                else if(theNode.Type == "UserDefinedFunction")
                {
                    scriptBase = SQLScripts.PeepFN;
                }
                else
                {
                    Popups.ResetVars();
                    Popups.message = "Unsupported object type compare";
                    Popups.Alert();
                }
                    
                tScript = string.Format(scriptBase, DBI.ObjectName);

                using (SqlConnection tConn = new SqlConnection(tAuthString))
                {
                    tConn.Open();
                    SqlCommand tCommand = new SqlCommand(tScript, tConn);
                    tCount = (int)tCommand.ExecuteScalar();
                }

                if (tCount == 0)
                {
                    wMessage = string.Format("{0}/{1} has NO {2}:{3}"
                        , q.Conn.Server
                        , q.DB
                        , DBI.Type
                        , DBI.ObjectName);

                    Popups.ResetVars();
                    Popups.message = "Missing Obj in DB";
                    Popups.Alert();
                }
                else
                {
                    // emulate click on plugin
                    cmd.Execute();

                    // emulate click on file1
                    ManaSQLConfig.fakeCompare2 = true;
                    ManaSQLConfig.PageIndex = 2;
                    ManaSQLConfig.UploadFile1.UpdateVariables(theNode);
                    if (theNode.IsDatabaseObject && theNode.TryGetDatabaseObject(out DBI))
                    {
                        ManaSQLConfig.CompareFile1.ResetWhereSSP(false);
                        ManaSQLConfig.CompareFile1.AppendWhereSSP(DBI.ObjectName, false);
                        ManaSQLConfig.UploadFile1.ResetWhereFiles(false);
                        ManaSQLConfig.UploadFile1.AppendWhereFile(string.Format("{0}.{1}", DBI.ObjectName, ManaSQLConfig.Extension), false);
                    }
                    ManaSQLConfig.CompareFile1.UpdateVariables(theNode);

                    // emulate click on file2

                    //TODO: fake obj explorer node
                    ManaSQLConfig.fakie.Server = q.Server;
                    ManaSQLConfig.fakie.DB = q.DB;
                    ManaSQLConfig.fakie.Obj = DBI.ObjectName;
                    ManaSQLConfig.fakie.Type = DBI.Type;
                    ManaSQLConfig.fakie.Path = theNode.Path;

                    ManaSQLConfig.UploadFile2.MirrorVariables(ManaSQLConfig.fakie, q.Conn);

                    ManaSQLConfig.CompareFile2.ResetWhereSSP(false);
                    ManaSQLConfig.CompareFile2.AppendWhereSSP(DBI.ObjectName, false);
                    ManaSQLConfig.UploadFile2.ResetWhereFiles(false);
                    ManaSQLConfig.UploadFile2.AppendWhereFile(string.Format("{0}.{1}", DBI.ObjectName, ManaSQLConfig.Extension), false);

                    ManaSQLConfig.CompareFile2.MirrorVariables(ManaSQLConfig.fakie, q.Conn);

                    cmd.manaSettingsForm.ActionCompareFileWrite();
                    cmd.manaSettingsForm.ActionCompareFileCompare();
                }
            }
        }
    }
}
