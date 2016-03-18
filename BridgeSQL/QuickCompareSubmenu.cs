using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RedGate.SIPFrameworkShared;

namespace BridgeSQL
{
    class QuickCompareSubmenu : SubmenuSimpleOeMenuItemBase
    {
        public QuickCompareSubmenu(SimpleOeMenuItemBase[] subMenus) : base(subMenus)
        {
        }

        public override string ItemText
        {
            get { return "Quick Compare"; }
        }

        public override bool AppliesTo(ObjectExplorerNodeDescriptorBase oeNode)
        {
            return GetApplicableChildren(oeNode).Length > 0;
        }
    }
}
