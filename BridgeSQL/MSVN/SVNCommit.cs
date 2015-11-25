using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;
using SqlMana;

namespace BridgeSQL.MSVN
{
    class SVNCommit : ActionSimpleOeMenuItemBase
    {
        public override bool AppliesTo(ObjectExplorerNodeDescriptorBase oeNode)
        {
            var theNode = (IOeNode)oeNode;
            return
                ManaSQLConfig.SvnCommit
                && ManaSQLConfig.ValidGenPaths
                && theNode.Type == "StoredProcedure";
        }
        public override string ItemText
        {
            get { return "[SVN] Commit"; }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            IOeNode theNode = (IOeNode)node;
            IDatabaseObjectInfo DBI;

            if (theNode.IsDatabaseObject && theNode.TryGetDatabaseObject(out DBI))
            {
                ManaSQLConfig.Extract.ResetWhereSSP(false);
                ManaSQLConfig.Extract.AppendWhereSSP(DBI.ObjectName);

                ManaProcess.runExe(
                    ManaSQLConfig.TProcPath
                    , TProcCommands.Add(ManaSQLConfig.Extract.FormSelectedSSPFilePaths().ToArray())
                    , false
                    );
                ManaProcess.runExe(
                    ManaSQLConfig.TProcPath
                    , TProcCommands.Commit(ManaSQLConfig.Extract.FormSelectedSSPFilePaths().ToArray())
                    , false
                    );
            }
        }
    }
}
