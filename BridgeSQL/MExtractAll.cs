using RedGate.SIPFrameworkShared;
using SqlMana;

namespace BridgeSQL
{
    class MExtractAll : ActionSimpleOeMenuItemBase
    {
        public override bool AppliesTo(ObjectExplorerNodeDescriptorBase oeNode)
        {
            var theNode = (IOeNode)oeNode;
            return
                ManaSQLConfig.ValidGenPaths
                && ManaSQLConfig.Extract.ValidPaths
                && ManaSQLConfig.ShowExtract
                && theNode.Type == "StoredProcedures";
        }
        public override string ItemText
        {
            get { return "[Extract] All SSPs"; }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            //throw new NotImplementedException();
            string args = ManaSQLConfig.Extract.CompileArgs(1);
            args = "data " + args;
            ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);
        }
    }
}
