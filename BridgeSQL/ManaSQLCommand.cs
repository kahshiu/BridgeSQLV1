using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;

namespace BridgeSQL
{
    class ManaSQLCommand : ISharedCommand
    {
        public string Name { get { return "ManaSQL"; } }
        public string Caption { get { return "SQL to text"; } }
        public string Tooltip { get { return "ManaSQL Config"; } }

        private readonly ICommandImage commandImage = new CommandImageNone();
        public ICommandImage Icon { get { return commandImage; } }
        public string[] DefaultBindings { get { return new string[] { }; } }
        public bool Visible { get { return true; } }
        public bool Enabled { get { return true; } }

        private ISsmsFunctionalityProvider6 mainPlug;
        private IToolWindow manaSettingWindow;

        // private Guid formGuid = new Guid("579fa20c-38cb-47776-9f57-6751d10e31d0");
        // request form guid from Redgate to avoid collision
        private Guid formGuid = Guid.NewGuid();
        private ManaSQLForm manaSettingsForm;
        private IOeNode mainNode;

        public ManaSQLCommand(ISsmsFunctionalityProvider6 thePlug)
        {
            mainPlug = thePlug;
        }

        public void UpdateNode(ObjectExplorerNodeDescriptorBase theSelectedNode)
        {
            mainNode = theSelectedNode as IOeNode;
            UpdateIndicator();
        }

        public void UpdateIndicator()
        {
            IConnectionInfo mainConn;
            if (mainNode != null
                && mainNode.HasConnection
                && mainNode.TryGetConnection(out mainConn)
                && manaSettingsForm != null
               )
            {
                //manaSettingsForm.UpdateIndicator(mainConn.ConnectionString);
            }
        }

        public void Execute()
        {
            if (manaSettingWindow == null)
            {
                manaSettingsForm = new ManaSQLForm(mainPlug);
                manaSettingWindow = mainPlug.ToolWindow.Create(manaSettingsForm, Caption, formGuid, true);
                manaSettingWindow.Window.IsFloating = false; // can't be docked
                manaSettingWindow.Window.Linkable = false;
            }
            manaSettingWindow.Activate(true);
        }
    }
}
