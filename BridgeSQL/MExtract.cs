using RedGate.SIPFrameworkShared;
using SqlMana;

namespace BridgeSQL
{
    class MExtract : ActionSimpleOeMenuItemBase
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
            get
            {
                return "[Extract] Extract and Commit";
            }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            IOeNode theNode = (IOeNode)node;
            IDatabaseObjectInfo DBI;

            if (theNode.IsDatabaseObject && theNode.TryGetDatabaseObject(out DBI))
            {
                ManaSQLConfig.PageIndex = 0;
                ManaSQLConfig.Extract.ResetWhereSSP(false);
                ManaSQLConfig.Extract.AppendWhereSSP(DBI.ObjectName);

                string args = ManaSQLConfig.Extract.CompileArgs();
                args = "data " + args;
                ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);

                //add this command just in case SSP is new and not added
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
