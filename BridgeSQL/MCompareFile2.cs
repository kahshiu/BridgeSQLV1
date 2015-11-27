using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;

namespace BridgeSQL
{
    class MCompareFile2 : ActionSimpleOeMenuItemBase
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
            get { return "[Compare] File2"; }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            IOeNode theNode = (IOeNode)node;
            IDatabaseObjectInfo DBI;
            
            if (theNode.IsDatabaseObject && theNode.TryGetDatabaseObject(out DBI))
            {
                ManaSQLConfig.CompareFile2.ResetWhereSSP(false);
                ManaSQLConfig.CompareFile2.AppendWhereSSP(DBI.ObjectName);
            }
            ManaSQLConfig.CompareFile2.UpdateVariables(theNode);
        }
    }
}
