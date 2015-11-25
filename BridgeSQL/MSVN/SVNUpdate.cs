using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;

namespace BridgeSQL.MSVN
{
    class SVNUpdate : ActionSimpleOeMenuItemBase
    {
        public override bool AppliesTo(ObjectExplorerNodeDescriptorBase oeNode)
        {
            var theNode = (IOeNode)oeNode;
            return
                ManaSQLConfig.SvnUpdate
                && ManaSQLConfig.ValidGenPaths
                && theNode.Type == "StoredProcedure";
        }
        public override string ItemText
        {
            get { return "[SVN] Update"; }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            IOeNode theNode = (IOeNode)node;
            IDatabaseObjectInfo DBI;
        }
    }
}
