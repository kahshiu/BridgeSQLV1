using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SqlMana;
using System.IO;
using RedGate.SIPFrameworkShared;
using System.Text.RegularExpressions;
//using System.Runtime.InteropServices;

namespace BridgeSQL
{
    public partial class ManaSQLForm : UserControl
    {
        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool GetCaretPos(out Point lpPoint);

        public TextBox currentTextBox;
        public bool currentTextBoxIsValid = false;
        public string message = "";
        public ISsmsFunctionalityProvider6 mainPlug;

        public ManaSQLForm(ISsmsFunctionalityProvider6 thePlug)
        {
            InitializeComponent();
            mainPlug = thePlug;

            // checkboxes
            ManaSQLConfig.Extract.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshCheckBoxes);
            ManaSQLConfig.Upload.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshCheckBoxes);
            ManaSQLConfig.CompareFile1.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshCheckBoxes);
            ManaSQLConfig.CompareFile2.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshCheckBoxes);
            ManaSQLConfig.CompareDir.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshCheckBoxes);
            ManaSQLConfig.UpdatedCheckBox += new ManaSQLConfig.CheckBoxHandler(RefreshCheckBoxes);

            // textboxes
            ManaSQLConfig.Extract.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshTextBox);
            ManaSQLConfig.Upload.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshTextBox);
            ManaSQLConfig.CompareFile1.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshTextBox);
            ManaSQLConfig.CompareFile2.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshTextBox);
            ManaSQLConfig.CompareDir.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshTextBox);
            ManaSQLConfig.UpdatedText += new ManaSQLConfig.TextHandler(RefreshTextBox);

            //labels
            ManaSQLConfig.Extract.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshLabel);
            ManaSQLConfig.Upload.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshLabel);

            // listboxes
            ManaSQLConfig.Extract.UpdatedWhere += new FixedSettings.WhereHandler(RefreshListBoxItems);
            ManaSQLConfig.Upload.UpdatedWhere += new FixedSettings.WhereHandler(RefreshListBoxItems);
            ManaSQLConfig.Upload.UpdatedWhere += new FixedSettings.WhereHandler(RefreshSelectedListBoxItems);
            ManaSQLConfig.UpdatedList += new ManaSQLConfig.ListHandler(RefreshListBoxItems);
            ManaSQLConfig.UpdatedList += new ManaSQLConfig.ListHandler(RefreshSelectedListBoxItems);

            // force every program start to adopt default setting
            extractDDir.Checked = true;
            uploadDDir.Checked = true;
            compareFile1DDir.Checked = true;
            compareFile2DDir.Checked = true;
            compareDirDDir.Checked = true;

            RefreshCheckBoxes("all");
            RefreshTextBox("all");
            RefreshListBoxItems("all");

            //non visuals
            //ManaSQLConfig.UpdatedNonVisualData += new ManaSQLConfig.NonVisualDataHandler();

            //extractRepoWarning.Visible = false;
            //extractLogWarning.Visible = false;
            //uploadRepoWarning.Visible = false;
            //uploadLogWarning.Visible = false;
            //compareFile1RepoWarning.Visible = false;
            //compareFile2RepoWarning.Visible = false;
            //compareFile1LogWarning.Visible = false;
            //compareFile2LogWarning.Visible = false;
            //compareDirRepoWarning.Visible = false;
            //compareDirRepo2Warning.Visible = false;
            //compareDirLogWarning.Visible = false;
            //compareDirResultWarning.Visible = false;
            //generalRepoWarning.Visible = false;
            //generalSQLManaWarning.Visible = false;
            //generalTProcWarning.Visible = false;
            //customPathWarning.Visible = false;
        }

        private void Initialise()
        {

        }

        // START: CONTEXT MENU
        // Pop out context menu
        // handler to turn on context menu
        private void ShowTextContext(object sender, MouseEventArgs e)
        {
            var control = sender as TextBox;
            if (e.Button == MouseButtons.Left && (ModifierKeys & Keys.Control) == Keys.Control)
            {
                ShowQuickAccess(control, Cursor.Position);
            }
        }
        private void ShowQuickAccess(TextBox control, Point location)
        {
            textContextMenu.Show(location);
            exploreText.Enabled = currentTextBoxIsValid;
        }

        // display as context menu
        private void ShowFavPaths(TextBox control, Point location)
        {
            favPaths.Items.Clear();
            List<ToolStripItem> items = new List<ToolStripItem>();
            string[] paths = ManaSQLConfig.GetCustomPaths().ToArray();
            for (int i = 0; i < paths.Length; i++)
            {
                ToolStripMenuItem temp = new ToolStripMenuItem();
                temp.Name = "path" + i;
                temp.Text = paths[i];
                temp.Size = new System.Drawing.Size(144, 26);
                temp.Click += new System.EventHandler(this.FillTextBox);
                items.Add(temp);
            }
            favPaths.Items.AddRange(items.ToArray());
            favPaths.Show(location);
        }

        private void StartBrowse(object sender, EventArgs e)
        {
            string temp;
            if (currentTextBoxIsValid && currentTextBox != null)
            {
                temp = currentTextBox.Text;
            }
            else
            {
                temp = Path.GetPathRoot(ManaSQLConfig.WinPath);
            }
            dirBrowser.SelectedPath = temp;

            DialogResult result = dirBrowser.ShowDialog();
            if (result == DialogResult.OK)
            {
                currentTextBox.Text = dirBrowser.SelectedPath;
            }
        }

        private void StartExplore(object sender, EventArgs e)
        {
            ManaProcess.runExe("Explorer", currentTextBox.Text, false);
        }

        private void FillTextBox(object sender, EventArgs e)
        {
            var control = sender as ToolStripMenuItem;
            if (currentTextBox != null)
            {
                UpdateTextBox(currentTextBox.Name, control.Text);
            }
        }

        // START: ALL PATH TEXTBOXES
        private void StoreTextRef(object sender, EventArgs e)
        {
            var control = sender as TextBox;
            currentTextBox = control;
            currentTextBoxIsValid = Util.ValidatePath(control.Text);
        }
        private void UpdatePathWithLeave(object sender, EventArgs e)
        {
            var control = sender as TextBox;
            currentTextBox = null;
            currentTextBoxIsValid = false;
            UpdateTextBox(control.Name, control.Text);
        }

        private void UpdatePathWithKey(object sender, KeyEventArgs e)
        {
            var control = sender as TextBox;

            if (e.KeyCode == Keys.Enter)
            {
                UpdateTextBox(control.Name, control.Text);
            }
            else if (e.KeyCode == Keys.Down && (ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                //Point pos;
                //GetCaretPos(out pos);
                //StartPosition = FormStartPosition.CenterScreen;
                if (!control.ReadOnly)
                {
                    Point caret = control.GetPositionFromCharIndex(control.SelectionStart);
                    Point startPosition = control.PointToScreen(Point.Empty);
                    startPosition.X += caret.X;
                    startPosition.Y += (int)control.Font.GetHeight();
                    ShowFavPaths(control, startPosition);
                }
            }
        }

        // START: ALL CREATE BUTTONS
        private void CreateDir(object sender, EventArgs e)
        {
            var control = sender as Button;
            string path = "";
            string varname = "";

            if (control.Name == "extractRepoCreate")
            {
                path = ManaSQLConfig.Extract.GetRepoPath();
                varname = "extract-RepoPath";
            }
            else if (control.Name == "extractLogCreate")
            {
                path = ManaSQLConfig.Extract.GetLogPath();
                varname = "extract-LogPath";
            }
            else if (control.Name == "uploadRepoCreate")
            {
                path = ManaSQLConfig.Upload.GetRepoPath();
                varname = "upload-RepoPath";
            }
            else if (control.Name == "uploadLogCreate")
            {
                path = ManaSQLConfig.Upload.GetLogPath();
                varname = "upload-LogPath";
            }

            if (path == "" || varname == "")
            {
                message = string.Format(@"{0}{1}{2}", "Error: ", "Empty string in directory.", "");
                MessageBox.Show(message);
                return;
            }

            try
            {
                Directory.CreateDirectory(path);
                RefreshTextBox(varname);
            }
            catch (Exception except)
            {
                message = string.Format(@"{0}{1}{2}", "Error in creating directory: ", Environment.NewLine, path);
                MessageBox.Show(message);
            }
        }

        // START: ALL CHECKBOXES
        private void CheckBoxUI_Click(object sender, EventArgs e)
        {
            var control = sender as CheckBox;
            bool isChecked = control.CheckState == CheckState.Checked;
            UpdateCheckBoxes(control.Name, isChecked);
        }

        // START: PAGE EXTRACT
        // extract tab handlers
        private void ExtractActions(object sender, EventArgs e)
        {
            var control = sender as Button;

            if (control.Name == "extractClear")
            {
                ResetListBoxItems("extractSSP");
            }
            else if (control.Name == "extractList")
            {
                string args = ManaSQLConfig.Extract.CompileArgs();
                args = "data " + args;
                ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);

                // add file just in case its not already added
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

        // START: PAGE UPLOAD
        // upload tab handlers
        private void uploadSSP_SelectedIndexChanged(object sender, EventArgs e)
        {
            uploadCheckAll.Checked = false;
            UpdateSelectedListBoxItems("uploadSSP", uploadSSP.CheckedItems.Cast<string>().ToArray());
        }

        private void uploadSelectFromFile_Click(object sender, EventArgs e)
        {
            DialogResult result = fileSearch.ShowDialog();
            if (result == DialogResult.OK)
            {
                uploadCheckAll.Checked = false;
                UpdateSelectedListBoxItems("uploadSSP", new string[0]);

                string line = "";
                List<string> items = new List<string>();
                using (StreamReader file = new StreamReader(fileSearch.FileName))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        items.Add(line);
                    }
                    file.Close();
                }
                UpdateSelectedListBoxItems("uploadSSP", items.ToArray());
            }
        }

        private void uploadCheckAll_Click(object sender, EventArgs e)
        {
            var control = sender as CheckBox;
            bool isChecked = control.CheckState == CheckState.Checked;
            if (isChecked)
            {
                //check all
                UpdateSelectedListBoxItems("uploadSSP", uploadSSP.Items.Cast<string>().ToArray());
            }
            else
            {
                //remove all checked
                UpdateSelectedListBoxItems("uploadSSP", new string[0]);
            }
        }

        private void uploadList_Click(object sender, EventArgs e)
        {
            string args = ManaSQLConfig.Upload.CompileArgs();
            args = "data " + args;
            ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);
        }

        // START: PAGE COMPARE FILE
        // compare tab handlers
        private void compareFileAction1_Click(object sender, EventArgs e)
        {
            //string args;
            //args = ManaSQLConfig.CompareFile1.CompileArgs();
            //args = "data " + args;
            //ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);

            //args = ManaSQLConfig.CompareFile2.CompileArgs();
            //args = "data " + args;
            //ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);
        }

        private void compareFileTempPaths_CheckedChanged(object sender, EventArgs e)
        {
            //var control = sender as CheckBox;
            //bool isChecked = control.CheckState == CheckState.Checked;
            //ManaSQLConfig.CompareFile1.UseTemp = isChecked;
            //ManaSQLConfig.CompareFile2.UseTemp = isChecked;
        }

        private void compareDirCompare_Click(object sender, EventArgs e)
        {
            // get the arguments from here,
            // combine with those from repopath, repopath2
        }

        // START: DISPLAY SETTINGS
        // Handlers to manage context menu of explorer objects
        private void SaveSettings(object sender, EventArgs e)
        {
            MSettings.UpdateSettings();
            MSettings.SaveSettings();
        }

        private void SelectCustomPathForRemove(object sender, EventArgs e)
        {
            ManaSQLConfig.SelectCustomPath = customPathList.SelectedIndex;
        }

        private bool ConfirmBox(string path, string type = "directory")
        {
            string title = string.Format(@"This will create {0}: {1} {2} {3}", type, path, Environment.NewLine, "Continue?");
            string caption = string.Format(@"[Create {0}]", type);
            DialogResult confirm = MessageBox.Show(
                title
                , caption
                , MessageBoxButtons.YesNo
                , MessageBoxIcon.Question
                , MessageBoxDefaultButton.Button1
                );
            return confirm == DialogResult.Yes;
        }

        private void CustomPathActions(object sender, EventArgs e)
        {
            var control = sender as Button;
            if (control.Equals(customPathAdd))
            {
                if (control.Text == "Add")
                {
                    UpdateListBoxItems("customPathList", new string[1] { ManaSQLConfig.CustomPathText });
                    UpdateTextBox("customPath", "");
                }
                else if (control.Text == "Create" && ConfirmBox(ManaSQLConfig.CustomPathText))
                {
                    try
                    {
                        Directory.CreateDirectory(customPath.Text);
                        control.Text = "Add";
                    }
                    catch (Exception except)
                    {
                        message = string.Format(@"{0}{1}{2}", "Error in creating directory: ", Environment.NewLine, customPath.Text);
                        MessageBox.Show(message);
                    }
                }
            }
            else if (control.Equals(customPathRemove))
            {
                if (ManaSQLConfig.SelectCustomPath > -1)
                {
                    ManaSQLConfig.RemoveCustomPaths(ManaSQLConfig.SelectCustomPath);
                }
                ManaSQLConfig.SelectCustomPath = -1;
            }
        }

        // START: TEXTBOXES
        // Generic function, from DS
        // Based on datastore (config), update textboxes
        private void RefreshTextBox(string list)
        {
            string[] items = Util.SanitizeStringList(list);
            bool isValidPath = false;
            bool isValidFile = false;
            string repoText;
            string repo2Text;
            string logText;
            string resultText;
            string filename;

            if (Util.Contains(items, "all") || Util.Contains(items, "extract") || Util.Contains(items, "extract-RepoPath"))
            {
                repoText = ManaSQLConfig.Extract.GetRepoPath();
                extractRepo.Text = repoText;
                extractRepoWarning.Visible = !(ManaSQLConfig.Extract.ValidRepoPath || repoText == "");
                extractRepoCreate.Visible = ManaSQLConfig.Extract.ValidCreateRepoPath;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "extract") || Util.Contains(items, "extract-LogPath"))
            {
                logText = ManaSQLConfig.Extract.GetLogPath();
                extractLog.Text = logText;
                extractLogWarning.Visible = !(ManaSQLConfig.Extract.ValidLogPath || logText == "");
                extractLogCreate.Visible = ManaSQLConfig.Extract.ValidCreateLogPath;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "upload") || Util.Contains(items, "upload-RepoPath"))
            {
                repoText = ManaSQLConfig.Upload.GetRepoPath();
                uploadRepo.Text = repoText;
                uploadRepoWarning.Visible = !(ManaSQLConfig.Upload.ValidRepoPath || repoText == "");
                uploadRepoCreate.Visible = ManaSQLConfig.Upload.ValidCreateRepoPath;
                RefreshListBoxItems("upload");
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "upload") || Util.Contains(items, "upload-LogPath"))
            {
                logText = ManaSQLConfig.Upload.GetLogPath();
                uploadLog.Text = logText;
                uploadLogWarning.Visible = !(ManaSQLConfig.Upload.ValidLogPath || logText == "");
                uploadLogCreate.Visible = ManaSQLConfig.Upload.ValidCreateLogPath;
                RefreshListBoxItems("upload");
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareFile1") || Util.Contains(items, "compareFile1-RepoPath"))
            {
                //repoText = ManaSQLConfig.CompareFile1.GetRepoPath();
                //isValidPath = Directory.Exists(repoText);
                //filename = ManaSQLConfig.CompareFile1.WhereSSPString;

                //compareFile1Obj.Text = filename;
                //compareFile1Repo.Text = repoText;
                //compareFile1RepoWarning.Visible = 
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareFile1") || Util.Contains(items, "compareFile1-LogPath"))
            {
                //filename = ManaSQLConfig.CompareFile1.WhereSSPString;
                //logText = ManaSQLConfig.CompareFile1.GetLogPath();
                //compareFile1Log.Text = logText;
                //compareFile1LogWarning.Visible = logText == "";
            }

            if (Util.Contains(items, "all") || Util.Contains(items, "compareFile2") || Util.Contains(items, "compareFile2-RepoPath"))
            {
                //filename = ManaSQLConfig.CompareFile1.WhereSSPString;
                //repoText = ManaSQLConfig.CompareFile2.GetRepoPath();
                //isValidPath = Directory.Exists(repoText);
                //compareFile2Obj.Text = filename;
                //compareFile2Repo.Text = repoText;
                //compareFile2RepoWarning.Visible = isValidPath;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareFile2") || Util.Contains(items, "compareFile2-LogPath"))
            {
                //filename = ManaSQLConfig.CompareFile1.WhereSSPString;
                //logText = ManaSQLConfig.CompareFile2.GetLogPath();
                //isValidPath = Directory.Exists(logText);
                //compareFile2Log.Text = logText;
                //compareFile2LogWarning.Visible = isValidPath;
            }
            //if (varname.Contains("all") || varname.Contains("RepoPath"))
            //{
            //    repoText = ManaSQLConfig.CompareDir.GetRepoPath();
            //    compareDirRepo.Text = repoText;
            //    compareDirRepoWarning.Visible = repoText == "";
            //}
            //if (varname.Contains("all") || varname.Contains("RepoPath2"))
            //{
            //    repo2Text = ManaSQLConfig.CompareDir.GetRepoPath2();
            //    compareDirRepo2.Text = repo2Text;
            //    compareDirRepo2Warning.Visible = repo2Text == "";
            //}

            //if (Util.Contains(items, "all") || Util.Contains(items, "compareDir") || Util.Contains(items, "compareDir-LogPath"))
            //{
            //    logText = ManaSQLConfig.CompareDir.GetLogPath();
            //    isValidPath = Directory.Exists(logText);
            //    compareDirLog.Text = logText;
            //    compareDirLogWarning.Visible = isValidPath;
            //}
            //if (Util.Contains(items, "all") || Util.Contains(items, "compareDir") || Util.Contains(items, "compareDir-OutPath"))
            //{
            //    resultText = ManaSQLConfig.CompareDir.GetOutPath();
            //    isValidPath = Directory.Exists(resultText);
            //    compareDirResult.Text = resultText;
            //    compareDirResultWarning.Visible = isValidPath;
            //}

            if (Util.Contains(items, "all") || Util.Contains(items, "settings") || Util.Contains(items, "general-RepoPath"))
            {
                repoText = ManaSQLConfig.RepoPath;
                isValidPath = ManaSQLConfig.ValidRepoPath;
                generalRepo.Text = repoText;
                generalRepoWarning.Visible = !isValidPath;
                UpdateRepoDependents();
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "settings") || Util.Contains(items, "general-TProcPath"))
            {
                repoText = ManaSQLConfig.TProcPath;
                isValidPath = ManaSQLConfig.ValidTProcPath;
                generalTProc.Text = repoText;
                generalTProcWarning.Visible = !isValidPath;
                UpdateRepoDependents();
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "settings") || Util.Contains(items, "general-ProgPath"))
            {
                repoText = ManaSQLConfig.ProgPath;
                isValidPath = ManaSQLConfig.ValidProgPath;
                generalSQLMana.Text = repoText;
                generalSQLManaWarning.Visible = !isValidPath;
                UpdateRepoDependents();
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "general-CustomPath"))
            {
                repoText = ManaSQLConfig.CustomPathText;
                isValidPath = Util.ValidatePath(repoText) || repoText == "";
                customPath.Text = repoText;
                customPathWarning.Visible = !isValidPath;
                customPathAdd.Text = isValidPath ? "Add" : "Create";
            }
        }

        // Generic function, to DS
        // Updated on datastore (config) using textboxes
        private void UpdateTextBox(string list, string val)
        {
            string[] items = Util.SanitizeStringList(list);
            bool isValidPath = false;

            if (Util.Contains(items, "all") || Util.Contains(items, "extract") || Util.Contains(items, "extractRepo"))
            {
                ManaSQLConfig.Extract.RepoPath = val;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "extract") || Util.Contains(items, "extractLog"))
            {
                ManaSQLConfig.Extract.LogPath = val;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "upload") || Util.Contains(items, "uploadRepo"))
            {
                ManaSQLConfig.Upload.ResetWhereFiles(false);
                ManaSQLConfig.Upload.RepoPath = val;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "upload") || Util.Contains(items, "uploadLog"))
            {
                ManaSQLConfig.Upload.LogPath = val;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareFile2") || Util.Contains(items, "compareFile2Repo"))
            {
                ManaSQLConfig.CompareFile2.RepoPath = val;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareFile2") || Util.Contains(items, "compareFile2Log"))
            {
                ManaSQLConfig.CompareFile2.LogPath = val;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareFile1") || Util.Contains(items, "compareFile1Repo"))
            {
                ManaSQLConfig.CompareFile1.RepoPath = val;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareFile1") || Util.Contains(items, "compareFile1Log"))
            {
                ManaSQLConfig.CompareFile1.LogPath = val;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareDir") || Util.Contains(items, "compareDirRepo"))
            {
                ManaSQLConfig.CompareDir.RepoPath = val;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareDir") || Util.Contains(items, "compareDirRepo2"))
            {
                ManaSQLConfig.CompareDir.RepoPath2 = val;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareDir") || Util.Contains(items, "compareDirLog"))
            {
                ManaSQLConfig.CompareDir.LogPath = val;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareDir") || Util.Contains(items, "compareDirResult"))
            {
                ManaSQLConfig.CompareDir.OutPath = val;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "settings") || Util.Contains(items, "generalRepo"))
            {
                ManaSQLConfig.RepoPath = val;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "settings") || Util.Contains(items, "generalTProc"))
            {
                ManaSQLConfig.TProcPath = val;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "settings") || Util.Contains(items, "generalSQLMana"))
            {
                ManaSQLConfig.ProgPath = val;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "settings") || Util.Contains(items, "customPath"))
            {
                ManaSQLConfig.CustomPathText = val;
            }
        }

        // START: CHECKBOXES
        // Generic function, from DS
        // Based on datastore (config), update checkboxes
        private void RefreshCheckBoxes(string list)
        {
            string[] items = Util.SanitizeStringList(list);
            // checkbox for pages
            if (Util.Contains(items, "all") || Util.Contains(items, "ShowExtract"))
            {
                dspExtract.CheckState = ManaSQLConfig.ShowExtract ? CheckState.Checked : CheckState.Unchecked;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "ShowCompareFile"))
            {
                dspCompareFile.CheckState = ManaSQLConfig.ShowCompareFile ? CheckState.Checked : CheckState.Unchecked;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "EnableLogging"))
            {
                enableLogging.CheckState = ManaSQLConfig.EnableLogging ? CheckState.Checked : CheckState.Unchecked;
            }
            // checkbox for SVN
            if (Util.Contains(items, "all") || Util.Contains(items, "SvnRepoStatus"))
            {
                dspSVNRepoStatus.CheckState = ManaSQLConfig.SvnRepoStatus ? CheckState.Checked : CheckState.Unchecked;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "SvnCommit"))
            {
                dspSVNCommit.CheckState = ManaSQLConfig.SvnCommit ? CheckState.Checked : CheckState.Unchecked;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "SvnUpdate"))
            {
                dspSVNUpdate.CheckState = ManaSQLConfig.SvnUpdate ? CheckState.Checked : CheckState.Unchecked;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "SvnShowLog"))
            {
                dspSVNLog.CheckState = ManaSQLConfig.SvnShowLog ? CheckState.Checked : CheckState.Unchecked;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "SvnDiff"))
            {
                dspSVNDiff.CheckState = ManaSQLConfig.SvnDiff ? CheckState.Checked : CheckState.Unchecked;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "SvnMerge"))
            {
                dspSVNMerge.CheckState = ManaSQLConfig.SvnMerge ? CheckState.Checked : CheckState.Unchecked;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "SvnBlame"))
            {
                dspSVNBlame.CheckState = ManaSQLConfig.SvnBlame ? CheckState.Checked : CheckState.Unchecked;
            }

            // each page setting
            if (Util.Contains(items, "all") || Util.Contains(items, "extract-IsDefault"))
            {
                extractDDir.CheckState = ManaSQLConfig.Extract.IsDefault ? CheckState.Checked : CheckState.Unchecked;
                extractRepo.ReadOnly = ManaSQLConfig.Extract.IsDefault;
                extractLog.ReadOnly = ManaSQLConfig.Extract.IsDefault;
                RefreshTextBox("extract");
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "upload-IsDefault"))
            {
                uploadDDir.CheckState = ManaSQLConfig.Upload.IsDefault ? CheckState.Checked : CheckState.Unchecked;
                uploadRepo.ReadOnly = ManaSQLConfig.Upload.IsDefault;
                uploadLog.ReadOnly = ManaSQLConfig.Upload.IsDefault;
                uploadCheckAll.Checked = false;
                RefreshTextBox("upload");
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareFile1-IsDefault"))
            {
                compareFile1DDir.CheckState = ManaSQLConfig.CompareFile1.IsDefault ? CheckState.Checked : CheckState.Unchecked;
                compareFile1Repo.ReadOnly = ManaSQLConfig.CompareFile1.IsDefault;
                compareFile1Log.ReadOnly = ManaSQLConfig.CompareFile1.IsDefault;
                RefreshTextBox("compareFile1");
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareFile2-IsDefault"))
            {
                compareFile2DDir.CheckState = ManaSQLConfig.CompareFile2.IsDefault ? CheckState.Checked : CheckState.Unchecked;
                compareFile2Repo.ReadOnly = ManaSQLConfig.CompareFile2.IsDefault;
                compareFile2Log.ReadOnly = ManaSQLConfig.CompareFile2.IsDefault;
                RefreshTextBox("compareFile2");
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareDir-IsDefault"))
            {
                compareDirDDir.CheckState = ManaSQLConfig.CompareDir.IsDefault ? CheckState.Checked : CheckState.Unchecked;
                compareDirLog.ReadOnly = ManaSQLConfig.CompareDir.IsDefault;
                compareDirResult.ReadOnly = ManaSQLConfig.CompareDir.IsDefault;
                RefreshTextBox("compareDir");
            }

        }

        // Generic function, to DS
        // Updated on datastore (config) using checkboxes
        private void UpdateCheckBoxes(string list, bool flag)
        {
            string[] items = Util.SanitizeStringList(list);

            if (Util.Contains(items, "all") || Util.Contains(items, "dspExtract"))
            {
                ManaSQLConfig.ShowExtract = flag;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "dspCompareFile"))
            {
                ManaSQLConfig.ShowCompareFile = flag;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "enableLogging"))
            {
                ManaSQLConfig.EnableLogging = flag;
            }

            // SVN menu
            if (Util.Contains(items, "all") || Util.Contains(items, "dspSVNRepoStatus"))
            {
                ManaSQLConfig.SvnRepoStatus = flag;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "dspSVNCommit"))
            {
                ManaSQLConfig.SvnCommit = flag;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "dspSVNUpdate"))
            {
                ManaSQLConfig.SvnUpdate = flag;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "dspSVNLog"))
            {
                ManaSQLConfig.SvnShowLog = flag;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "dspSVNDiff"))
            {
                ManaSQLConfig.SvnDiff = flag;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "dspSVNMerge"))
            {
                ManaSQLConfig.SvnMerge = flag;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "dspSVNBlame"))
            {
                ManaSQLConfig.SvnBlame = flag;
            }

            // each page setting
            if (Util.Contains(items, "all") || Util.Contains(items, "extractDDir"))
            {
                ManaSQLConfig.Extract.IsDefault = flag;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "uploadDDir"))
            {
                ManaSQLConfig.Upload.ResetWhereFiles(false);
                ManaSQLConfig.Upload.IsDefault = flag;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareFile1DDir"))
            {
                ManaSQLConfig.CompareFile1.IsDefault = flag;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareFile2DDir"))
            {
                ManaSQLConfig.CompareFile2.IsDefault = flag;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareDirDDir"))
            {
                ManaSQLConfig.CompareDir.IsDefault = flag;
            }
        }

        // START: LABELS
        // Generic function, from DS
        // Based on datastore (config), update labels
        private void RefreshLabel(string list)
        {
            string[] items = Util.SanitizeStringList(list);

            if (Util.Contains(items, "all") || Util.Contains(items, "extract"))
            {
                extractServer.Text = ManaSQLConfig.Extract.SERVER;
                extractDB.Text = ManaSQLConfig.Extract.DB;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "upload"))
            {
                uploadServer.Text = ManaSQLConfig.Upload.SERVER;
                uploadDB.Text = ManaSQLConfig.Upload.DB;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareFile1"))
            {
                compareFile1Server.Text = ManaSQLConfig.CompareFile1.SERVER;
                compareFile1DB.Text = ManaSQLConfig.CompareFile1.DB;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "compareFile2"))
            {
                compareFile2Server.Text = ManaSQLConfig.CompareFile2.SERVER;
                compareFile2DB.Text = ManaSQLConfig.CompareFile2.DB;
            }
        }

        // START: LISTBOXES
        // Generic function, from DS
        // Based on datastore (config), update listboxes
        private void RefreshListBoxItems(string list)
        {
            string[] items = Util.SanitizeStringList(list);

            if (Util.Contains(items, "all") || Util.Contains(items, "extract") || Util.Contains(items, "extract-WhereSSPStore"))
            {
                extractSSP.Items.Clear();
                extractSSP.Items.AddRange(ManaSQLConfig.Extract.WhereSSPList.ToArray());
                bool isListed =
                    ManaSQLConfig.ValidGenPaths
                    && ManaSQLConfig.Extract.ValidPaths
                    && extractSSP.Items.Count > 0
                    ;
                extractList.Enabled = isListed;
                extractClear.Enabled = isListed;
                extractSSP.Enabled = isListed;
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "upload")) //|| Util.Contains(items, "upload-WhereFileStore"))
            {
                bool isActive =
                    ManaSQLConfig.ValidGenPaths
                    && ManaSQLConfig.Upload.ValidPaths
                    && ManaSQLConfig.Upload.ValidRepoPath
                    ;
                uploadSelectFromFile.Enabled = isActive;
                uploadCheckAll.Enabled = isActive;
                uploadSSP.Enabled = isActive;
                uploadList.Enabled = isActive && ManaSQLConfig.Upload.WhereFileList.Count > 0;

                if (isActive)
                {
                    string[] filenames = Directory.GetFiles(ManaSQLConfig.Upload.GetRepoPath(), "*.sql");
                    List<int> toRemove = new List<int>();

                    for (int i = 0; i < filenames.Length; i++)
                    {
                        filenames[i] = Path.GetFileName(filenames[i]);

                        // mark list as non .sql extension
                        Regex regex = new Regex(@".sql$");
                        if (!regex.Match(filenames[i]).Success)
                        {
                            toRemove.Add(i);
                        }
                    }

                    // purge list that are non .sql extension
                    if (toRemove.Count > 0)
                    {
                        List<string> temp = new List<string>(filenames);
                        foreach (int x in toRemove)
                        {
                            temp.RemoveAt(x);
                        }
                        filenames = temp.ToArray();
                    }

                    uploadSSP.Items.Clear();
                    uploadSSP.Items.AddRange(filenames);
                }
                else
                {
                    uploadSSP.Items.Clear();
                }
            }
            if (Util.Contains(items, "all") || Util.Contains(items, "general-CustomPaths"))
            {
                customPathList.Items.Clear();
                customPathList.Items.AddRange(ManaSQLConfig.GetCustomPaths().ToArray());
            }
        }

        private void RefreshSelectedListBoxItems(string list)
        {
            string[] items = Util.SanitizeStringList(list);

            if (Util.Contains(items, "all") || Util.Contains(items, "upload") || Util.Contains(items, "upload-WhereFileStore"))
            {
                bool isActive =
                    ManaSQLConfig.ValidGenPaths
                    && ManaSQLConfig.Upload.ValidPaths
                    && ManaSQLConfig.Upload.ValidRepoPath
                    ;
                uploadList.Enabled = isActive && ManaSQLConfig.Upload.WhereFileList.Count > 0;

                if (isActive)
                {
                    string[] selected = ManaSQLConfig.Upload.WhereFileList.ToArray();
                    for (int i = 0; i < uploadSSP.Items.Count; i++)
                    {
                        int hit = Util.IndexOf(selected, uploadSSP.Items[i].ToString(), "i");
                        uploadSSP.SetItemChecked(i, hit > -1);
                    }
                }
            }
        }

        // Generic function, to DS
        // Reset/ Update (config) with listBox value
        private void ResetListBoxItems(string list)
        {
            string[] items = Util.SanitizeStringList(list);
            if (Util.Contains(items, "all") || Util.Contains(items, "extractSSP"))
            {
                ManaSQLConfig.Extract.ResetWhereSSP();
            }
            // Data not stored in DS, refer straight in Visual
            //if (Util.Contains(items, "all") || Util.Contains(items, "uploadSSP"))
            //{
            //}
            if (Util.Contains(items, "all") || Util.Contains(items, "customPaths"))
            {
                ManaSQLConfig.ResetCustomPaths();
            }
        }

        private void UpdateListBoxItems(string list, string[] vals)
        {
            string[] items = Util.SanitizeStringList(list);
            if (Util.Contains(items, "all") || Util.Contains(items, "extractSSP"))
            {
                for (int i = 0; i < vals.Length; i++)
                {
                    if (Util.ValidatePath(vals[i]))
                    {
                        ManaSQLConfig.Extract.AppendWhereSSP(vals[i]);
                    }
                }
            }
            // Data not stored in DS, refer straight in Visual
            //if (Util.Contains(items, "all") || Util.Contains(items, "uploadSSP"))
            //{
            //}
            if (Util.Contains(items, "all") || Util.Contains(items, "customPathList"))
            {
                for (int i = 0; i < vals.Length; i++)
                {
                    if (Util.ValidatePath(vals[i]))
                    {
                        ManaSQLConfig.AddCustomPath(vals[i]);
                    }
                }
            }
        }

        // method overloading: for list with multiple string selections
        private void UpdateSelectedListBoxItems(string list, string[] vals)
        {
            string[] items = Util.SanitizeStringList(list);
            if (Util.Contains(items, "all") || Util.Contains(items, "uploadSSP"))
            {
                if (vals.Length > 0)
                {
                    ManaSQLConfig.Upload.ResetWhereFiles(false);
                    for (int i = 0; i < vals.Length; i++)
                    {
                        ManaSQLConfig.Upload.AppendWhereFile(vals[i], i == (vals.Length - 1));
                    }
                }
                else
                {
                    ManaSQLConfig.Upload.ResetWhereFiles();
                }
            }
        }

        // method overloading: for list with single int selection
        private void UpdateSelectedListBoxItems(string list, int val)
        {
            string[] items = Util.SanitizeStringList(list);
            if (Util.Contains(items, "all") || Util.Contains(items, "customPathList"))
            {
                ManaSQLConfig.SelectCustomPath = val;
            }
        }

        // START: HELPERS
        // helper functions to activate/ deactivate form controls
        private void UpdateRepoDependents()
        {
            bool RepoIsActive = ManaSQLConfig.ValidRepoPath && ManaSQLConfig.ValidProgPath && ManaSQLConfig.ValidTProcPath;

            ManaSQLConfig.Extract.IsActive = RepoIsActive;
            ManaSQLConfig.CompareFile1.IsActive = RepoIsActive;
            ManaSQLConfig.CompareFile2.IsActive = RepoIsActive;
            ManaSQLConfig.Upload.IsActive = RepoIsActive;
            ManaSQLConfig.CompareDir.IsActive = RepoIsActive;

            ActivateTab(extract, ManaSQLConfig.Extract.IsActive);
            ActivateTab(compareFile, ManaSQLConfig.CompareFile1.IsActive && ManaSQLConfig.CompareFile2.IsActive);
            ActivateTab(upload, ManaSQLConfig.Upload.IsActive);
            ActivateTab(compare2, ManaSQLConfig.CompareDir.IsActive);
        }

        private void ActivateTab(TabPage container, bool isActive)
        {
            foreach (var control in container.Controls)
            {
                if (control is TextBox) (control as TextBox).Enabled = isActive;
                else if (control is Button) (control as Button).Enabled = isActive;
                else if (control is CheckBox) (control as CheckBox).Enabled = isActive;
                else if (control is ListBox) (control as ListBox).Enabled = isActive;
            }
        }
    }
}
