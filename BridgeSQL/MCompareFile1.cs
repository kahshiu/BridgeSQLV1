using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;

namespace BridgeSQL
{
    class MCompareFile1 : ActionSimpleOeMenuItemBase
    {
        public override bool AppliesTo(ObjectExplorerNodeDescriptorBase oeNode)
        {
            var theNode = (IOeNode)oeNode;
            return 
                ManaSQLConfig.ValidGenPaths
                && ManaSQLConfig.ShowCompareFile
                && theNode.Type == "StoredProcedure";
        }
        public override string ItemText
        {
            get { return "[Compare] File1"; }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            IOeNode theNode = (IOeNode)node;
            IDatabaseObjectInfo DBI;

            if (theNode.IsDatabaseObject && theNode.TryGetDatabaseObject(out DBI))
            {
                ManaSQLConfig.CompareFile1.ResetWhereSSP(false);
                ManaSQLConfig.CompareFile1.AppendWhereSSP(DBI.ObjectName,false);
            }
            ManaSQLConfig.CompareFile1.UpdateVariables(theNode);
        }
    }
}
