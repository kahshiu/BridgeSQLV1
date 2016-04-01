using RedGate.SIPFrameworkShared;
using SqlMana;

namespace BridgeSQL
{
    class MExtractAll : ActionSimpleOeMenuItemBase
    {
        private readonly ISsmsFunctionalityProvider6 plug;
        private ManaSQLCommand cmd;

        public MExtractAll (ISsmsFunctionalityProvider6 mPlug, ManaSQLCommand mCmd)
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
                && ManaSQLConfig.IsAllowedGroupNode(theNode)
                && theNode.Type != "UserProgrammability"
                ;
        }
        public override string ItemText
        {
            get {
                return "[Extract] ALL children";
            }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            ManaSQLConfig.PageIndex = 0;
            string args = ManaSQLConfig.Extract.CompileArgs(1);
            args = "data " + args;
            ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);

            if (ManaProcess.returnCode < 0)
            {
                Popups.ResetVars();
                Popups.message = "Error writing SQL(s) to file(s). View log?";
                Popups.Prompt();
                if (Popups.response == System.Windows.Forms.DialogResult.OK)
                {
                    ManaProcess.runExe("Explorer", ManaSQLConfig.Extract.GetLogPath(), false);
                }
            }
        }
    }
}
