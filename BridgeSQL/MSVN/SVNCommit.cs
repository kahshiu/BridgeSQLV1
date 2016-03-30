using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;
using SqlMana;

namespace BridgeSQL.MSVN
{
    class SVNCommit : ActionSimpleOeMenuItemBase
    {
        private readonly ISsmsFunctionalityProvider6 plug;
        private ManaSQLCommand cmd;
        private List<SimpleOeMenuItemBase> menu;

        public SVNCommit(ISsmsFunctionalityProvider6 mPlug, ManaSQLCommand mCmd, List<SimpleOeMenuItemBase> mMenu)
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
                && theNode.Type == "StoredProcedure";
        }
        public override string ItemText
        {
            get
            {
                string actionb4 = "+Add";
                if (ManaSQLConfig.IsWithExtract) actionb4 = actionb4 + "+Extract";
                return string.Format("[SVN] Commit (After {0})",actionb4.Substring(1));
            }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            IOeNode theNode = (IOeNode)node;
            IDatabaseObjectInfo DBI;

            if (theNode.IsDatabaseObject && theNode.TryGetDatabaseObject(out DBI))
            {
                ManaSQLConfig.Extract.ResetWhereSSP(false);
                ManaSQLConfig.Extract.AppendWhereSSP(DBI.ObjectName);

                if (ManaSQLConfig.IsWithExtract)
                {
                    //quick hack: reference to extract menu
                    var extractMenu = (ActionSimpleOeMenuItemBase)menu[0];
                    extractMenu.OnAction(node);
                }

                ManaProcess.runExe(
                    ManaSQLConfig.TProcPath
                    , TProcCommands.Add(ManaSQLConfig.Extract.FormSelectedSSPFilePaths().ToArray())
                    , false
                    );
                ManaProcess.runExe(
                    ManaSQLConfig.TProcPath
                    , TProcCommands.Commit(ManaSQLConfig.Extract.FormSelectedSSPFilePaths().ToArray())
                    , false
                    );
            }
        }
    }
}
