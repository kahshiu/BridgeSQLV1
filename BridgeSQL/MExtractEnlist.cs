using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;
using SqlMana;

namespace BridgeSQL
{
    class MExtractEnlist : ActionSimpleOeMenuItemBase
    {
        public override bool AppliesTo(ObjectExplorerNodeDescriptorBase oeNode)
        {
            var theNode = (IOeNode)oeNode;
            return 
                ManaSQLConfig.ValidGenPaths
                && ManaSQLConfig.Extract.ValidPaths
                && ManaSQLConfig.ShowExtract
                && theNode.Type == "StoredProcedure";
        }
        public override string ItemText
        {
            get { return "[Extract] Enlist ONLY"; }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            IOeNode theNode = (IOeNode)node;
            IDatabaseObjectInfo DBI;

            if (theNode.IsDatabaseObject && theNode.TryGetDatabaseObject(out DBI))
            {
                ManaSQLConfig.Extract.AppendWhereSSP(DBI.ObjectName);
            }
        }
    }
}
