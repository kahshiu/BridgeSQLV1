using RedGate.SIPFrameworkShared;
using SqlMana;

namespace BridgeSQL
{
    class MExtract : ActionSimpleOeMenuItemBase
    {
        private readonly ISsmsFunctionalityProvider6 plug;
        private ManaSQLCommand cmd;

        public MExtract (ISsmsFunctionalityProvider6 mPlug, ManaSQLCommand mCmd)
        {
            plug = mPlug;
            cmd = mCmd;
        }

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
            get
            {
                return "[Extract] Enlist + Extract";
            }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            IOeNode theNode = (IOeNode)node;
            IDatabaseObjectInfo DBI;

            if (theNode.IsDatabaseObject && theNode.TryGetDatabaseObject(out DBI))
            {
                cmd.Execute();
                ManaSQLConfig.PageIndex = 0;
                ManaSQLConfig.Extract.ResetWhereSSP(false);
                ManaSQLConfig.Extract.AppendWhereSSP(DBI.ObjectName);

                string args = ManaSQLConfig.Extract.CompileArgs();
                args = "data " + args;
                ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);

                if (ManaProcess.returnCode < 0)
                {
                    Popups.ResetVars();
                    Popups.message = "Error writing SQL to file. View log?";
                    Popups.Prompt();
                    if(Popups.response == System.Windows.Forms.DialogResult.OK)
                    {
                        ManaProcess.runExe("Explorer", ManaSQLConfig.Extract.GetLogPath(), false);
                    }
                }
                //else
                //{
                //    //add this command just in case SSP is new and not added
                //    ManaProcess.runExe(
                //        ManaSQLConfig.TProcPath
                //        , TProcCommands.Add(ManaSQLConfig.Extract.FormSelectedSSPFilePaths().ToArray())
                //        , false
                //        );
                //    ManaProcess.runExe(
                //        ManaSQLConfig.TProcPath
                //        , TProcCommands.Commit(ManaSQLConfig.Extract.FormSelectedSSPFilePaths().ToArray())
                //        , false
                //        );
                //}
            }
        }
    }
}
