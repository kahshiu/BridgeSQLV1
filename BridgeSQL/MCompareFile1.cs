﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGate.SIPFrameworkShared;

namespace BridgeSQL
{
    class MCompareFile1 : ActionSimpleOeMenuItemBase
    {
        private readonly ISsmsFunctionalityProvider6 plug;
        private ManaSQLCommand cmd;

        public MCompareFile1(ISsmsFunctionalityProvider6 mPlug, ManaSQLCommand mCmd)
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
            get { return "[Compare] File1"; }
        }
        public override void OnAction(ObjectExplorerNodeDescriptorBase node)
        {
            IOeNode theNode = (IOeNode)node;
            IDatabaseObjectInfo DBI;

            cmd.Execute();
            ManaSQLConfig.PageIndex = 2;
            ManaSQLConfig.UploadFile1.UpdateVariables(theNode);
            if (theNode.IsDatabaseObject && theNode.TryGetDatabaseObject(out DBI))
            {
                ManaSQLConfig.CompareFile1.ResetWhereSSP(false);
                ManaSQLConfig.CompareFile1.AppendWhereSSP(DBI.ObjectName, false);
                ManaSQLConfig.UploadFile1.ResetWhereFiles(false);
                ManaSQLConfig.UploadFile1.AppendWhereFile(string.Format("{0}.{1}", DBI.ObjectName, ManaSQLConfig.Extension), false);
            }
            ManaSQLConfig.CompareFile1.UpdateVariables(theNode);
        }
    }
}
