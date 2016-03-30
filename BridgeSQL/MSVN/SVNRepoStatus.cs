using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;
using SqlMana;

namespace BridgeSQL.MSVN
{
    class SVNRepoStatus : ActionSimpleOeMenuItemBase
    {
        public override bool AppliesTo(ObjectExplorerNodeDescriptorBase oeNode)
        {
            var theNode = (IOeNode)oeNode;
            return
                ManaSQLConfig.SvnRepoStatus
                && ManaSQLConfig.ValidGenPaths
                && ManaSQLConfig.IsAllowedSingleNode(theNode)
                && ManaSQLConfig.IsAllowedGroupNode(theNode);
        }
        public override string ItemText
        {
            get { return "[SVN] Repo status"; }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            IOeNode theNode = (IOeNode)node;
            IDatabaseObjectInfo DBI;

            if (theNode.IsDatabaseObject && theNode.TryGetDatabaseObject(out DBI))
            {
                ManaSQLConfig.Extract.ResetWhereSSP(false);
                ManaSQLConfig.Extract.AppendWhereSSP(DBI.ObjectName);

                //add this command just in case SSP is new and not added
                ManaProcess.runExe(
                    ManaSQLConfig.TProcPath
                    , TProcCommands.RepoStatus(ManaSQLConfig.Extract.FormRepoPath())
                    , false
                    );
            }
        }
    }
}
