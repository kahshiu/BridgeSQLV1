using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;
using RedGate.SIPFrameworkShared.ObjectExplorer;


namespace BridgeSQL
{
    public class MainExt : ISsmsAddin4
    {
        public string Version { get { return "1.0"; } }
        public string Description { get { return "Plugin: SSP to text version control"; } }
        public string Name { get { return "Merimen Custom Plugin"; } }
        public string Author { get { return "Kah Shiu"; } }
        public string Url { get { return @"https://github.com/red-gate/SampleSsmsEcosystemAddin"; } }

        private ISsmsFunctionalityProvider6 thePlug;
        private ManaSQLCommand manaSQLCommand;

        public void OnLoad(ISsmsExtendedFunctionalityProvider provider)
        {
            //MSettings.ReadSettings();
            ManaSQLConfig.Initialise();

            thePlug = (ISsmsFunctionalityProvider6)provider;
            manaSQLCommand = new ManaSQLCommand(thePlug);

            // adding UI to MSSQL
            thePlug.AddToolbarItem(manaSQLCommand);
            thePlug.AddTopLevelMenuItem(new MExtractAll());
            thePlug.AddTopLevelMenuItem(new MSVN.SVNCommitAll());
            thePlug.AddTopLevelMenuItem(new MSVN.SVNUpdateAll());

            thePlug.AddTopLevelMenuItem(new MExtract());
            thePlug.AddTopLevelMenuItem(new MExtractEnlist());
            thePlug.AddTopLevelMenuItem(new MCompareFile1());
            thePlug.AddTopLevelMenuItem(new MCompareFile2());

            // adding in SVN menu strip
            thePlug.AddTopLevelMenuItem(new MSVN.SVNRepoStatus());
            thePlug.AddTopLevelMenuItem(new MSVN.SVNCommit());
            thePlug.AddTopLevelMenuItem(new MSVN.SVNUpdate());
            thePlug.AddTopLevelMenuItem(new MSVN.SVNShowLog());
            thePlug.AddTopLevelMenuItem(new MSVN.SVNDiff());
            thePlug.AddTopLevelMenuItem(new MSVN.SVNMerge());
            thePlug.AddTopLevelMenuItem(new MSVN.SVNBlame());

            thePlug.ObjectExplorerWatcher.ConnectionsChanged += (args) => { OnConnectionsChanged(args); };
            thePlug.ObjectExplorerWatcher.SelectionChanged += (args) => { OnSelectionChanged(args); };
        }

        private void OnSelectionChanged(ISelectionChangedEventArgs args)
        {
            //standard
            ManaSQLConfig.Extract.UpdateVariables(args.Selection);
            ManaSQLConfig.Upload.UpdateVariables(args.Selection);
            ManaSQLConfig.CompareDir.UpdateVariables(args.Selection);
        }

        private void OnConnectionsChanged(IConnectionsChangedEventArgs args)
        {
            ManaSQLConfig.Extract.UpdateCON(args.Connections);
        }

        public void OnShutdown()
        {
        }

        //deprecated, but required to fulfill the implementation
        public void OnNodeChanged(ObjectExplorerNodeDescriptorBase node)
        {
        }
    }

}
