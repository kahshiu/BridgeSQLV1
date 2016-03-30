using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;
using SqlMana;

namespace BridgeSQL.MSVN
{
    class SVNUpdateAll : ActionSimpleOeMenuItemBase
    {
        public override bool AppliesTo(ObjectExplorerNodeDescriptorBase oeNode)
        {
            var theNode = (IOeNode)oeNode;
            return
                ManaSQLConfig.SvnUpdate
                && ManaSQLConfig.ValidGenPaths
                && ManaSQLConfig.IsAllowedGroupNode(theNode);
        }
        public override string ItemText
        {
            get { return "[SVN] Update ALL"; }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            ManaProcess.runExe(
                ManaSQLConfig.TProcPath
                , TProcCommands.Update(new string[] { ManaSQLConfig.Extract.FormRepoPath() })
                , false
                );

        }
    }
}
