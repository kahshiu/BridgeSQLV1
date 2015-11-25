using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;

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
                && theNode.Type == "StoredProcedures"
                && theNode.Type == "StoredProcedure";
        }
        public override string ItemText
        {
            get { return "[SVN] Repo status"; }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            IOeNode theNode = (IOeNode)node;
            IDatabaseObjectInfo DBI;
            ManaSQLConfig.CompareFile1.UpdateVariables(theNode);
            if (theNode.IsDatabaseObject && theNode.TryGetDatabaseObject(out DBI))
            {
                ManaSQLConfig.CompareFile1.AppendWhereSSP(DBI.ObjectName);
            }
        }
    }
}
