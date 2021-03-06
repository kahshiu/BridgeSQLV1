﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;

namespace BridgeSQL
{
    class MCompareFile2 : ActionSimpleOeMenuItemBase
    {
        private readonly ISsmsFunctionalityProvider6 plug;
        private ManaSQLCommand cmd;

        public MCompareFile2(ISsmsFunctionalityProvider6 mPlug, ManaSQLCommand mCmd)
        {
            plug = mPlug;
            cmd = mCmd;
        }

        public override bool AppliesTo(ObjectExplorerNodeDescriptorBase oeNode)
        {
            var theNode = (IOeNode)oeNode;
            return
                ManaSQLConfig.ValidGenPaths
                && ManaSQLConfig.ShowCompareFile
                && ManaSQLConfig.IsAllowedSingleNode(theNode);
        }
        public override string ItemText
        {
            get { return "[Compare] File2"; }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            IOeNode theNode = (IOeNode)node;
            IDatabaseObjectInfo DBI;

            ManaSQLConfig.UploadFile2.ResetMirrorVariables();
            ManaSQLConfig.CompareFile2.ResetMirrorVariables();

            cmd.Execute();
            ManaSQLConfig.PageIndex = 2;
            ManaSQLConfig.UploadFile2.UpdateVariables(theNode);
            if (theNode.IsDatabaseObject && theNode.TryGetDatabaseObject(out DBI))
            {
                ManaSQLConfig.CompareFile2.ResetWhereSSP(false);
                ManaSQLConfig.CompareFile2.AppendWhereSSP(DBI.ObjectName,false);
                ManaSQLConfig.UploadFile2.ResetWhereFiles(false);
                ManaSQLConfig.UploadFile2.AppendWhereFile(string.Format("{0}.{1}", DBI.ObjectName, ManaSQLConfig.Extension), false);
            }
            ManaSQLConfig.CompareFile2.UpdateVariables(theNode);
        }
    }
}
