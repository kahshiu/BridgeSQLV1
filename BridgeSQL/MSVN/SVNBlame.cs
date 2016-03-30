using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;
using SqlMana;

namespace BridgeSQL.MSVN
{
    class SVNBlame : ActionSimpleOeMenuItemBase
    {
        public override bool AppliesTo(ObjectExplorerNodeDescriptorBase oeNode)
        {
            var theNode = (IOeNode)oeNode;
            return
                ManaSQLConfig.SvnBlame
                && ManaSQLConfig.ValidGenPaths
                && ManaSQLConfig.IsAllowedSingleNode(theNode);
                ;
        }
        public override string ItemText
        {
            get { return "[SVN] Blame"; }
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
                    , TProcCommands.Blame(ManaSQLConfig.Extract.FormSelectedSSPFilePaths()[0])
                    , false
                    );
            }
        }
    }
}
