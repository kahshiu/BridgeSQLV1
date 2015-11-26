using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;
using SqlMana;

namespace BridgeSQL.MSVN
{
    class SVNCommitAll : ActionSimpleOeMenuItemBase
    {
        public override bool AppliesTo(ObjectExplorerNodeDescriptorBase oeNode)
        {
            var theNode = (IOeNode)oeNode;
            return
                ManaSQLConfig.SvnCommit
                && ManaSQLConfig.ValidGenPaths
                && theNode.Type == "StoredProcedures";
        }
        public override string ItemText
        {
            get { return "[SVN] Commit ALL"; }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            ManaProcess.runExe(
                ManaSQLConfig.TProcPath
                , TProcCommands.Add(new string[] { ManaSQLConfig.Extract.FormRepoPath() })
                , false
                );
            ManaProcess.runExe(
                ManaSQLConfig.TProcPath
                , TProcCommands.Commit(new string[] { ManaSQLConfig.Extract.FormRepoPath() })
                , false
                );
        }
    }
}
