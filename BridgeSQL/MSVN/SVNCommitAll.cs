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
        private readonly ISsmsFunctionalityProvider6 plug;
        private ManaSQLCommand cmd;
        private List<SimpleOeMenuItemBase> menu;

        public SVNCommitAll(ISsmsFunctionalityProvider6 mPlug, ManaSQLCommand mCmd, List<SimpleOeMenuItemBase> mMenu)
        {
            plug = mPlug;
            cmd = mCmd;
            menu = mMenu;
        }

        public override bool AppliesTo(ObjectExplorerNodeDescriptorBase oeNode)
        {
            var theNode = (IOeNode)oeNode;
            return
                ManaSQLConfig.SvnCommit
                && ManaSQLConfig.ValidGenPaths
                && ManaSQLConfig.IsAllowedGroupNode(theNode);
        }
        public override string ItemText
        {
            get { return "[SVN] Commit ALL"; }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            if (ManaSQLConfig.IsWithExtract)
            {
                //quick hack: reference to extract menu
                var extractMenu = (ActionSimpleOeMenuItemBase)menu[0];
                extractMenu.OnAction(node);
            }

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
