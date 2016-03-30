using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BridgeSQL
{
    class Popups
    {
        public static DialogResult response = DialogResult.Ignore;
        public static string message = "";
        public static string title = "[BridgeSQL: ERROR]";
        public static MessageBoxButtons interact = MessageBoxButtons.OKCancel;
        public static MessageBoxIcon icon = MessageBoxIcon.Error;
        public static MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1;
        
        public static void ResetVars()
        {
            response = DialogResult.Ignore;
            message = "";
            title = "[BridgeSQL: ERROR]";
            interact = MessageBoxButtons.OKCancel;
            icon = MessageBoxIcon.Error;
            defaultButton = MessageBoxDefaultButton.Button1;
        }
        public static void Alert()
        {
            MessageBox.Show(message, title);
        }
        public static void Prompt()
        {
            response = MessageBox.Show(message, title, interact, icon, defaultButton);
        }
    }
}
