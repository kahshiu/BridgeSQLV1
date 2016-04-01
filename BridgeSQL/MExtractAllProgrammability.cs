using System;
using System.Collections.Generic;
using System.Linq;
using RedGate.SIPFrameworkShared;
using SqlMana;

namespace BridgeSQL
{
    class MExtractAllProgrammability : ActionSimpleOeMenuItemBase
    {
        private readonly ISsmsFunctionalityProvider6 plug;
        private ManaSQLCommand cmd;
        private List<SimpleOeMenuItemBase> menu = new List<SimpleOeMenuItemBase>();

        public MExtractAllProgrammability(ISsmsFunctionalityProvider6 mPlug, ManaSQLCommand mCmd, List<SimpleOeMenuItemBase> mMenu)
        {
            plug = mPlug;
            cmd = mCmd;
            menu = mMenu;
        }

        public override bool AppliesTo(ObjectExplorerNodeDescriptorBase oeNode)
        {
            var theNode = (IOeNode)oeNode;
            return theNode.Type == "UserProgrammability";
        }
        public override string ItemText
        {
            get
            {
                return "[Extract] Programmability";
            }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            string args;
            ManaSQLConfig.PageIndex = 0;
            bool[] results = new bool[3]; // capture failed flags only

            // Extract SSP
            ManaSQLConfig.Extract.TYPE = "StoredProcedures";
            ManaSQLConfig.Extract.UpdateInteractionFlags();
            args = ManaSQLConfig.Extract.CompileArgs(1);
            args = "data " + args;
            ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);
            results[0] = ManaProcess.returnCode < 0;

            // Extract functiosn Scalar
            ManaSQLConfig.Extract.TYPE = "Functions_scalar_valued";
            ManaSQLConfig.Extract.UpdateInteractionFlags();
            args = ManaSQLConfig.Extract.CompileArgs(1);
            args = "data " + args;
            ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);
            results[1] = ManaProcess.returnCode < 0;

            // Extract functiosn table
            ManaSQLConfig.Extract.TYPE = "Functions_table_valued";
            ManaSQLConfig.Extract.UpdateInteractionFlags();
            args = ManaSQLConfig.Extract.CompileArgs(1);
            args = "data " + args;
            ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);
            results[2] = ManaProcess.returnCode < 0;

            if (results[0] || results[1] || results[2])
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

