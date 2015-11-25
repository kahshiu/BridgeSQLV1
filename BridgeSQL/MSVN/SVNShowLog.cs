using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;

namespace BridgeSQL.MSVN
{
    class SVNShowLog : ActionSimpleOeMenuItemBase
    {
        public override bool AppliesTo(ObjectExplorerNodeDescriptorBase oeNode)
        {
            var theNode = (IOeNode)oeNode;
            return
                ManaSQLConfig.SvnShowLog
                && ManaSQLConfig.ValidGenPaths
                && theNode.Type == "StoredProcedure";
        }
        public override string ItemText
        {
            get { return "[SVN] Show Log"; }
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
