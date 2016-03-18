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

        public ISsmsFunctionalityProvider6 mainPlug;
        public TextBox currentTextBox;
        public bool currentTextBoxIsValid = false;
        public string filterText = "";
        public bool filterTextCasing = false;
        public string message = "";

        private string errorMsg = string.Format(
            @"Error: Some paths do not exist or do not contain {0} files. {1} No action taken. Program will exit."
                , ManaSQLConfig.Extension
                , Environment.NewLine
            );

        public ManaSQLForm(ISsmsFunctionalityProvider6 thePlug)
        {
            InitializeComponent();
            mainPlug = thePlug;
            Initialise();

            // page:extract
            ManaSQLConfig.Extract.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshCheckBoxes);
            ManaSQLConfig.Extract.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshTextBox);
            ManaSQLConfig.Extract.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshLabel);
            ManaSQLConfig.Extract.UpdatedWhere += new FixedSettings.WhereHandler(RefreshListBoxItems);

            // page:update
            ManaSQLConfig.Upload.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshCheckBoxes);
            ManaSQLConfig.Upload.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshTextBox);
            ManaSQLConfig.Upload.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshLabel);
            ManaSQLConfig.Upload.UpdatedWhere += new FixedSettings.WhereHandler(RefreshListBoxItems);
            ManaSQLConfig.Upload.UpdatedWhere += new FixedSettings.WhereHandler(RefreshSelectedListBoxItems);
            ManaSQLConfig.Upload.UpdatedWhere += new FixedSettings.WhereHandler(RefreshLabel);

            // page:comparefile
            // -------------- file 1
            ManaSQLConfig.CompareFile1.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshCheckBoxes);
            ManaSQLConfig.CompareFile1.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshTextBox);
            ManaSQLConfig.CompareFile1.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshLabel);
            // -------------- file 2
            ManaSQLConfig.CompareFile2.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshCheckBoxes);
            ManaSQLConfig.CompareFile2.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshTextBox);
            ManaSQLConfig.CompareFile2.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshLabel);

            // page:comparedir
            ManaSQLConfig.CompareDir.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshCheckBoxes);
            ManaSQLConfig.CompareDir.UpdatedVariables += new FixedSettings.VariablesHandler(RefreshTextBox);

            // generic setting
            ManaSQLConfig.UpdatedCheckBox += new ManaSQLConfig.CheckBoxHandler(RefreshCheckBoxes);
            ManaSQLConfig.UpdatedText += new ManaSQLConfig.TextHandler(RefreshTextBox);
            ManaSQLConfig.UpdatedList += new ManaSQLConfig.ListHandler(RefreshListBoxItems);
            ManaSQLConfig.UpdatedList += new ManaSQLConfig.ListHandler(RefreshSelectedListBoxItems);
            ManaSQLConfig.UpdatedComboBox += new ManaSQLConfig.ComboBoxHandler(RefreshComboBox);

            ManaSQLConfig.UpdatedPage += new ManaSQLConfig.PageHandler(RefreshPage);
            //non visuals
            //ManaSQLConfig.UpdatedNonVisualData += new ManaSQLConfig.NonVisualDataHandler();         
        }

        private void Initialise()
        {
            extractDDir.Checked = true;
            uploadDDir.Checked = true;
            compareFile1DDir.Checked = true;
            compareFile2DDir.Checked = true;
            compareDirDDir.Checked = true;

            extractServer.Text = "SELECT NODE";
            extractDB.Text = "SELECT NODE";
            uploadServer.Text = "SELECT NODE";
            uploadDB.Text = "SELECT NODE";
            compareFile1Server.Text = "SELECT NODE";
            compareFile1DB.Text = "SELECT NODE";
            compareFile1Obj.Text = "SELECT NODE";
            compareFile2Server.Text = "SELECT NODE";
            compareFile2DB.Text = "SELECT NODE";
            compareFile2Obj.Text = "SELECT NODE";
            serverNaming.Items.AddRange(ManaSQLConfig.ServerNamings.ToArray());
            logNaming.Items.AddRange(ManaSQLConfig.LogNamings.ToArray());

            RefreshCheckBoxes("all");
            RefreshTextBox("all");
            RefreshListBoxItems("all");
            RefreshButton("all");
            RefreshComboBox("all");
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
        // handler for textbox onfocus
        private void StoreTextRef(object sender, EventArgs e)
        {
            var control = sender as TextBox;
            currentTextBoxIsValid = Util.ValidatePath(control.Text);
            currentTextBox = control;

        }
        // handler for textbox onblur
        private void UpdatePathWithLeave(object sender, EventArgs e)
        {
            var control = sender as TextBox;

            if (!control.ReadOnly)
            {
                currentTextBox = null;
                currentTextBoxIsValid = false;
                UpdateTextBox(control.Name, control.Text);
            }
        }
        // handler for textbox keyboard
        private void UpdatePathWithKey(object sender, KeyEventArgs e)
        {
            var control = sender as TextBox;

            if (e.KeyCode == Keys.Enter)
            {
                UpdateTextBox(control.Name, control.Text);
            }
            else if (e.KeyCode == Keys.Down && (ModifierKeys & Keys.Control) == Keys.Control)
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
                path = ManaSQLConfig.Extract.FormRepoPath();
                varname = "extract-RepoPath";
            }
            else if (control.Name == "extractLogCreate")
            {
                path = ManaSQLConfig.Extract.FormLogPath();
                varname = "extract-LogPath";
            }
            else if (control.Name == "uploadRepoCreate")
            {
                path = ManaSQLConfig.Upload.FormRepoPath();
                varname = "upload-RepoPath";
            }
            else if (control.Name == "uploadLogCreate")
            {
                path = ManaSQLConfig.Upload.FormLogPath();
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

        // START: ALL COMBOBOX
        private void SelectedComboUI(object sender, EventArgs e)
        {
            var control = sender as ComboBox;
            int index = control.SelectedIndex;
            UpdateComboBox(control.Name, index);
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
                if (ManaSQLConfig.Extract.ValidPaths)
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
                else
                {
                    MessageBox.Show(errorMsg);
                }
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
            if (ManaSQLConfig.Upload.ValidPaths)
            {
                string args = ManaSQLConfig.Upload.CompileArgs();
                args = "data " + args;
                ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);
            }
            else
            {
                MessageBox.Show(errorMsg);
            }
        }

        // START: PAGE COMPARE FILE
        // compare tab handlers
        private void compareFileAction1_Click(object sender, EventArgs e)
        {
            var control = sender as Button;
            string args;

            //TODO: implement defensive code here
            if (control.Equals(compareFileWrite))
            {
                ActionCompareFileWrite();
            }
            else if (control.Equals(compareFileCompare))
            {
                ActionCompareFileCompare();
            }
            else if (control.Equals(compareFileUpload1))
            {
                if (ManaSQLConfig.UploadFile1.ValidPaths)
                {
                    args = ManaSQLConfig.UploadFile1.CompileArgs();
                    args = string.Format(@"data {0}", args);
                    ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);
                }
                else
                {
                    MessageBox.Show(errorMsg);
                }
            }
            else if (control.Equals(compareFileUpload2))
            {
                args = ManaSQLConfig.UploadFile2.CompileArgs();

                if (ManaSQLConfig.UploadFile2.ValidPaths)
                {
                    args = ManaSQLConfig.UploadFile2.CompileArgs();
                    args = string.Format(@"data {0}", args);
                    ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);
                }
                else
                {
                    MessageBox.Show(errorMsg);
                }
            }
            // SVN merging
        }

        public void ActionCompareFileCompare()
        {
            if (!compareFileCompare.Enabled) return;

            string pathRight = ManaSQLConfig.CompareFile1.FormSelectedSSPFilePaths()[0];
            string pathLeft = ManaSQLConfig.CompareFile2.FormSelectedSSPFilePaths()[0];
            string indicatorRight = ManaSQLConfig.CompareFile1.FormTags();
            string indicatorLeft = ManaSQLConfig.CompareFile2.FormTags();

            ManaProcess.runExe(
                ManaSQLConfig.TProcPath
                , TProcCommands.Diff(
                    pathLeft
                    , pathRight
                    , indicatorLeft
                    , indicatorRight
                    , pathLeft
                    , pathRight
                    )
                , false
                );
        }

        public void ActionCompareFileWrite()
        {
            string args;
            if (ManaSQLConfig.CompareFile1.ValidPaths && ManaSQLConfig.CompareFile2.ValidPaths)
            {
                args = ManaSQLConfig.CompareFile1.CompileArgs();
                args = "data " + args;
                ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);

                args = ManaSQLConfig.CompareFile2.CompileArgs();
                args = "data " + args;
                ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);

                // TODO: addback the files so can upload later
                ManaSQLConfig.UploadFile1.AppendWhereFile(string.Format("{0}.{1}", ManaSQLConfig.CompareFile1.OBJ, ManaSQLConfig.Extension), false);
                ManaSQLConfig.UploadFile2.AppendWhereFile(string.Format("{0}.{1}", ManaSQLConfig.CompareFile2.OBJ, ManaSQLConfig.Extension), false);
                RefreshButton("compareFile1");
            }
            else
            {
                MessageBox.Show(errorMsg);
            }
        }

        private void compareDirCompare_Click(object sender, EventArgs e)
        {
            //TODO: implement defensive code here
            if (!ManaSQLConfig.CompareDir.ValidPaths)
            {
                MessageBox.Show(errorMsg);
                return;
            }

            ManaSQLConfig.CompareDir.GenAutomationDate();
            string logDir = ManaSQLConfig.CompareDir.GetLogPath();
            string outDir = Path.GetDirectoryName(ManaSQLConfig.CompareDir.GetOutPath());
            bool isValidDir = !Directory.Exists(logDir) || !Directory.Exists(outDir); // these two should point to the same dir

            string filename = "compare.log";
            string fileDir = Path.GetDirectoryName(logDir.Substring(0, logDir.Length - 1));
            string filePath = string.Format(@"{0}\{1}", fileDir, filename);
            string fileContent = "";

            if (ManaSQLConfig.CompareDir.IsDefault)
            {
                if (isValidDir)
                {
                    try
                    {
                        Directory.CreateDirectory(logDir);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format(@"Error: Cannot create directory: [{0}]", logDir));
                        return;
                    }
                }
            }
            else
            {
                if (isValidDir)
                {
                    MessageBox.Show("Error: Not specified paths not present: Log Path, Result Path");
                    return;
                }
            }

            string args = ManaSQLConfig.CompareDir.CompileArgs();
            args = "data " + args;
            ManaProcess.runExe(ManaSQLConfig.ProgPath, args, false);

            try
            {
                string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                string datetime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:fff");

                fileContent = string.Format(
@"---------------------------
[Compare Begin      ] {0} {1}
[Compare Written Dir] {2}
[Compare Base       ] {3}
[Compare Target     ] {4}
[Compare Base Path  ] {5}
[Compare Target Path] {6}
---------------------------
"
, datetime
, username
, ManaSQLConfig.CompareDir.AutomationDate
, ManaSQLConfig.CompareDir.FormTags()
, ManaSQLConfig.CompareDir.FormTags2()
, ManaSQLConfig.CompareDir.GetRepoPath()
, ManaSQLConfig.CompareDir.GetRepoPath2()
);
                File.AppendAllText(filePath, fileContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Cannot compare log: " + filename);
            }

            ManaSQLConfig.CompareDir.InvalidateAutomationDate();
        }

        // START: DISPLAY SETTINGS
        // Handlers to manage context menu of explorer objects
        private void SaveSettings(object sender, EventArgs e)
        {
            MSettings.UpdateSettings();
            MSettings.SaveSettings();
        }

        private void SelectCustomRepoPathForRemove(object sender, EventArgs e)
        {
            ManaSQLConfig.RPMan.SelectAt(customRepoPaths.SelectedIndex);
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

        private void CustomRepoPathActions(object sender, EventArgs e)
        {
            var control = sender as Button;
            if (control.Equals(customRepoPathAdd))
            {
                if (control.Text == "Add")
                {
                    UpdateListBoxItems("customRepoPathList", new string[1] { ManaSQLConfig.CustomRepoPathText });
                    UpdateTextBox("customRepoPath", "");
                }
                else if (control.Text == "Create" && ConfirmBox(ManaSQLConfig.CustomPathText))
                {
                    try
                    {
                        Directory.CreateDirectory(customRepoPath.Text);
                        control.Text = "Add";
                    }
                    catch (Exception except)
                    {
                        message = string.Format(@"{0}{1}{2}", "Error in creating directory: ", Environment.NewLine, customRepoPath.Text);
                        MessageBox.Show(message);
                    }
                }
            }
            else if (control.Equals(customRepoPathRemove))
            {
                ManaSQLConfig.RemoveCustomRepoPaths();
            }
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
            bool isSystemGen;
            bool isValidPath = false;
            string theText;

            string isRefreshButton = "";
            string isRefreshListBoxItems = "";
            string isUpdateRepoDependents = "";

            // textboxes in extract page
            if (Util.Contains(new string[] { "all", "extract", "extract-RepoPath" }, list))
            {
                theText = ManaSQLConfig.Extract.FormRepoPath();
                extractRepo.Text = theText;
                extractRepoWarning.Visible = !ManaSQLConfig.Extract.ValidRepoPath;
                extractRepoCreate.Visible = ManaSQLConfig.Extract.ValidCreateRepoPath;
                isRefreshButton = "extract";
            }
            if (Util.Contains(new string[] { "all", "extract", "extract-LogPath" }, list))
            {
                theText = ManaSQLConfig.Extract.FormLogPath();
                extractLog.Text = theText;
                extractLogWarning.Visible = !ManaSQLConfig.Extract.ValidLogPath;
                extractLogCreate.Visible = ManaSQLConfig.Extract.ValidCreateLogPath;
                isRefreshButton = "extract";
            }

            // attempt to optimise, only run once here to refresh
            if (isRefreshButton == "extract")
            {
                RefreshButton(isRefreshButton);
                isRefreshButton = "";
            }

            // textboxes in upload page
            if (Util.Contains(new string[] { "all", "upload", "upload-RepoPath" }, list))
            {
                theText = ManaSQLConfig.Upload.FormRepoPath();
                uploadRepo.Text = theText;
                uploadRepoWarning.Text = GetWarningText(theText);
                uploadRepoWarning.Visible = !ManaSQLConfig.Upload.ValidRepoPath;
                uploadRepoCreate.Visible = ManaSQLConfig.Upload.ValidCreateRepoPath;
                isRefreshListBoxItems = "upload";
            }
            if (Util.Contains(new string[] { "all", "upload", "upload-LogPath" }, list))
            {
                theText = ManaSQLConfig.Upload.FormLogPath();
                uploadLog.Text = theText;
                uploadLogWarning.Visible = !ManaSQLConfig.Upload.ValidLogPath;
                uploadLogCreate.Visible = ManaSQLConfig.Upload.ValidCreateLogPath;
                isRefreshListBoxItems = "upload";
            }

            // attempt to optimise, only run once here to refresh
            if (isRefreshListBoxItems == "upload")
            {
                RefreshListBoxItems(isRefreshListBoxItems);
                isRefreshListBoxItems = "";
            }

            // textboxes in compare file page
            if (Util.Contains(new string[] { "all", "compareFile1", "compareFile1-RepoPath" }, list))
            {
                theText = ManaSQLConfig.CompareFile1.FormRepoPath();
                compareFile1Repo.Text = theText;
                compareFile1RepoWarning.Visible = !ManaSQLConfig.CompareFile1.ValidRepoPath;
                compareFile1RepoCreate.Visible = ManaSQLConfig.CompareFile1.ValidCreateRepoPath;
                isRefreshButton = "compareFile1";
            }
            if (Util.Contains(new string[] { "all", "compareFile1", "compareFile1-LogPath" }, list))
            {
                theText = ManaSQLConfig.CompareFile1.FormLogPath();
                compareFile1Log.Text = theText;
                compareFile1LogWarning.Visible = !ManaSQLConfig.CompareFile1.ValidLogPath;
                compareFile1LogCreate.Visible = ManaSQLConfig.CompareFile1.ValidCreateLogPath;
                isRefreshButton = "compareFile1";
            }

            // attempt to optimise, only run once here to refresh
            if (isRefreshButton == "compareFile1")
            {
                RefreshButton(isRefreshButton);
                isRefreshButton = "";
            }

            if (Util.Contains(new string[] { "all", "compareFile2", "compareFile2-RepoPath" }, list))
            {
                theText = ManaSQLConfig.CompareFile2.FormRepoPath();
                compareFile2Repo.Text = theText;
                compareFile2RepoWarning.Visible = !ManaSQLConfig.CompareFile2.ValidRepoPath;
                compareFile2RepoCreate.Visible = ManaSQLConfig.CompareFile2.ValidCreateRepoPath;
                isRefreshButton = "compareFile2";
            }
            if (Util.Contains(new string[] { "all", "compareFile2", "compareFile2-LogPath" }, list))
            {
                theText = ManaSQLConfig.CompareFile2.FormLogPath();
                compareFile2Log.Text = theText;
                compareFile2LogWarning.Visible = !ManaSQLConfig.CompareFile2.ValidLogPath;
                compareFile2LogCreate.Visible = ManaSQLConfig.CompareFile2.ValidCreateLogPath;
                isRefreshButton = "compareFile2";
            }

            // attempt to optimise, only run once here to refresh
            if (isRefreshButton == "compareFile2")
            {
                RefreshButton(isRefreshButton);
                isRefreshButton = "";
            }

            // textboxes in compare dir page
            if (Util.Contains(new string[] { "all", "compareDir", "compareDir-RepoPath" }, list))
            {
                theText = ManaSQLConfig.CompareDir.FormRepoPath();
                compareDirRepo.Text = theText;
                compareDirRepoWarning.Text = GetWarningText(theText);
                compareDirRepoWarning.Visible = !ManaSQLConfig.CompareDir.ValidRepoPath;
                compareDirRepoCreate.Visible = ManaSQLConfig.CompareDir.ValidCreateRepoPath;
                isRefreshButton = "compareDir";
            }
            if (Util.Contains(new string[] { "all", "compareDir", "compareDir-RepoPath2" }, list))
            {
                theText = ManaSQLConfig.CompareDir.FormRepoPath2();
                compareDirRepo2.Text = theText;
                compareDirRepo2Warning.Text = GetWarningText(theText);
                compareDirRepo2Warning.Visible = !ManaSQLConfig.CompareDir.ValidRepoPath2;
                compareDirRepo2Create.Visible = ManaSQLConfig.CompareDir.ValidCreateRepoPath2;
                isRefreshButton = "compareDir";
            }
            if (Util.Contains(new string[] { "all", "compareDir", "compareDir-LogPath" }, list))
            {
                theText = ManaSQLConfig.CompareDir.FormLogPath();
                compareDirLog.Text = theText;
                compareDirLogWarning.Visible = !ManaSQLConfig.CompareDir.ValidLogPath;
                compareDirLogCreate.Visible = ManaSQLConfig.CompareDir.ValidCreateLogPath;
                isRefreshButton = "compareDir";
                RefreshLabel("compareDirLogLocationPath");
            }
            if (Util.Contains(new string[] { "all", "compareDir", "compareDir-OutPath" }, list))
            {
                theText = ManaSQLConfig.CompareDir.FormOutPath();
                compareDirResult.Text = theText;
                compareDirResultWarning.Visible = !ManaSQLConfig.CompareDir.ValidOutPath;
                compareDirResultCreate.Visible = ManaSQLConfig.CompareDir.ValidCreateOutPath;
                isRefreshButton = "compareDir";
                RefreshLabel("compareDirResultLocationPath");
            }

            if (isRefreshButton == "compareDir")
            {
                RefreshButton(isRefreshButton);
                isRefreshButton = "";
            }

            // textboxes in setting page
            if (Util.Contains(new string[] { "all", "settings", "general-RepoPath" }, list))
            {
                theText = ManaSQLConfig.RepoPath;
                isValidPath = ManaSQLConfig.ValidRepoPath;
                generalRepo.Text = theText;
                generalRepoWarning.Visible = !isValidPath;
                isUpdateRepoDependents = "repo";
            }
            if (Util.Contains(new string[] { "all", "settings", "general-TProcPath" }, list))
            {
                theText = ManaSQLConfig.TProcPath;
                isValidPath = ManaSQLConfig.ValidTProcPath;
                generalTProc.Text = theText;
                generalTProcWarning.Visible = !isValidPath;
                isUpdateRepoDependents = "repo";
            }
            if (Util.Contains(new string[] { "all", "settings", "general-ProgPath" }, list))
            {
                theText = ManaSQLConfig.ProgPath;
                isValidPath = ManaSQLConfig.ValidProgPath;
                generalSQLMana.Text = theText;
                generalSQLManaWarning.Visible = !isValidPath;
                isUpdateRepoDependents = "repo";
            }
            if (Util.Contains(new string[] { "all", "general-CustomPath" }, list))
            {
                theText = ManaSQLConfig.CustomPathText;
                isValidPath = Util.ValidatePath(theText) || theText == "";
                customPath.Text = theText;
                customPathWarning.Visible = !isValidPath;
                customPathAdd.Text = isValidPath ? "Add" : "Create";
            }

            if (Util.Contains(new string[] { "all", "general-CustomRepoPath" }, list))
            {
                theText = ManaSQLConfig.CustomRepoPathText;
                isValidPath = false;

                if (CustomRepoPath.ValidateFormat(theText))
                {
                    string[] temp = CustomRepoPath.DecompileFullString(theText);
                    if (!ManaSQLConfig.RPMan.IsDuplicate(temp[0], temp[1]))
                    {
                        isValidPath = true;
                    }
                }
                isValidPath = isValidPath || theText == "";

                customRepoPath.Text = isValidPath ? theText : "";
                customRepoPathWarning.Visible = !isValidPath;
                //customRepoPathAdd.Text = isValidPath ? "Add" : "Create";
            }

            if (isUpdateRepoDependents == "repo")
            {
                UpdateRepoDependents();
            }
        }

        // Generic function, to DS
        // Updated on datastore (config) using textboxes
        private void UpdateTextBox(string list, string val)
        {
            bool isValidPath = false;

            if (Util.Contains(new string[] { "all", "extract", "extractRepo" }, list))
            {
                ManaSQLConfig.Extract.RepoPath = val;
            }
            if (Util.Contains(new string[] { "all", "extract", "extractLog" }, list))
            {
                ManaSQLConfig.Extract.LogPath = val;
            }
            if (Util.Contains(new string[] { "all", "upload", "uploadRepo" }, list))
            {
                ManaSQLConfig.Upload.ResetWhereFiles(false);
                ManaSQLConfig.Upload.RepoPath = val;
            }
            if (Util.Contains(new string[] { "all", "upload", "uploadLog" }, list))
            {
                ManaSQLConfig.Upload.LogPath = val;
            }
            if (Util.Contains(new string[] { "all", "compareFile2", "compareFile2Repo" }, list))
            {
                ManaSQLConfig.CompareFile2.RepoPath = val;
            }
            if (Util.Contains(new string[] { "all", "compareFile2", "compareFile2Log" }, list))
            {
                ManaSQLConfig.CompareFile2.LogPath = val;
            }
            if (Util.Contains(new string[] { "all", "compareFile1", "compareFile1Repo" }, list))
            {
                ManaSQLConfig.CompareFile1.RepoPath = val;
            }
            if (Util.Contains(new string[] { "all", "compareFile1", "compareFile1Log" }, list))
            {
                ManaSQLConfig.CompareFile1.LogPath = val;
            }
            if (Util.Contains(new string[] { "all", "compareDir", "compareDirRepo" }, list))
            {
                ManaSQLConfig.CompareDir.RepoPath = val;
            }
            if (Util.Contains(new string[] { "all", "compareDir", "compareDirRepo2" }, list))
            {
                ManaSQLConfig.CompareDir.RepoPath2 = val;
            }
            if (Util.Contains(new string[] { "all", "compareDirLog" }, list))
            {
                ManaSQLConfig.CompareDir.LogPath = val;
            }
            if (Util.Contains(new string[] { "all", "compareDirResult" }, list))
            {
                ManaSQLConfig.CompareDir.OutPath = val;
            }
            if (Util.Contains(new string[] { "all", "settings", "generalRepo" }, list))
            {
                ManaSQLConfig.RepoPath = val;
            }
            if (Util.Contains(new string[] { "all", "settings", "generalTProc" }, list))
            {
                ManaSQLConfig.TProcPath = val;
            }
            if (Util.Contains(new string[] { "all", "settings", "generalSQLMana" }, list))
            {
                ManaSQLConfig.ProgPath = val;
            }
            if (Util.Contains(new string[] { "all", "settings", "customPath" }, list))
            {
                ManaSQLConfig.CustomPathText = val;
            }
            if (Util.Contains(new string[] { "all", "settings", "customRepoPath" }, list))
            {
                ManaSQLConfig.CustomRepoPathText = val;
            }
        }

        //helper functions for textbox
        private string GetWarningText(string repoPath)
        {
            string warningText = "Directory Not Exist";
            if (!Util.HasFile(repoPath, ManaSQLConfig.Extension))
            {
                warningText = string.Format(@"Directory contains no {0} file", ManaSQLConfig.Extension);
            }
            return warningText;
        }

        // START: PAGES
        private void RefreshPage(string list)
        {
            tab.SelectedIndex = ManaSQLConfig.PageIndex;
        }

        // START: CHECKBOXES
        // Generic function, from DS
        // Based on datastore (config), update checkboxes
        private void RefreshCheckBoxes(string list)
        {
            string isRefreshTextBox = "";

            // Setting: checkbox for pages
            if (Util.Contains(new string[] { "all", "general-ShowExtract" }, list))
            {
                dspExtract.CheckState = ManaSQLConfig.ShowExtract ? CheckState.Checked : CheckState.Unchecked;
            }
            if (Util.Contains(new string[] { "all", "general-ShowCompareFile" }, list))
            {
                dspCompareFile.CheckState = ManaSQLConfig.ShowCompareFile ? CheckState.Checked : CheckState.Unchecked;
            }
            if (Util.Contains(new string[] { "all", "general-EnableLogging" }, list))
            {
                enableLogging.CheckState = ManaSQLConfig.EnableLogging ? CheckState.Checked : CheckState.Unchecked;
                RefreshButton("all");
            }
            // Setting: checkbox for SVN
            if (Util.Contains(new string[] { "all", "general-SvnRepoStatus" }, list))
            {
                dspSVNRepoStatus.CheckState = ManaSQLConfig.SvnRepoStatus ? CheckState.Checked : CheckState.Unchecked;
            }
            if (Util.Contains(new string[] { "all", "general-SvnCommit" }, list))
            {
                dspSVNCommit.CheckState = ManaSQLConfig.SvnCommit ? CheckState.Checked : CheckState.Unchecked;
            }
            if (Util.Contains(new string[] { "all", "general-SvnUpdate" }, list))
            {
                dspSVNUpdate.CheckState = ManaSQLConfig.SvnUpdate ? CheckState.Checked : CheckState.Unchecked;
            }
            if (Util.Contains(new string[] { "all", "general-SvnShowLog" }, list))
            {
                dspSVNLog.CheckState = ManaSQLConfig.SvnShowLog ? CheckState.Checked : CheckState.Unchecked;
            }
            if (Util.Contains(new string[] { "all", "general-SvnDiff" }, list))
            {
                dspSVNDiff.CheckState = ManaSQLConfig.SvnDiff ? CheckState.Checked : CheckState.Unchecked;
            }
            if (Util.Contains(new string[] { "all", "general-SvnMerge" }, list))
            {
                dspSVNMerge.CheckState = ManaSQLConfig.SvnMerge ? CheckState.Checked : CheckState.Unchecked;
            }
            if (Util.Contains(new string[] { "all", "general-SvnBlame" }, list))
            {
                dspSVNBlame.CheckState = ManaSQLConfig.SvnBlame ? CheckState.Checked : CheckState.Unchecked;
            }

            // each page: extract
            if (Util.Contains(new string[] { "all", "extract-IsDefault" }, list))
            {
                extractDDir.CheckState = ManaSQLConfig.Extract.IsDefault ? CheckState.Checked : CheckState.Unchecked;
                extractRepo.ReadOnly = ManaSQLConfig.Extract.IsDefault;
                extractLog.ReadOnly = ManaSQLConfig.Extract.IsDefault;
                RefreshTextBox("extract");
            }

            // each page: upload
            if (Util.Contains(new string[] { "all", "upload-IsDefault" }, list))
            {
                uploadDDir.CheckState = ManaSQLConfig.Upload.IsDefault ? CheckState.Checked : CheckState.Unchecked;
                uploadRepo.ReadOnly = ManaSQLConfig.Upload.IsDefault;
                uploadLog.ReadOnly = ManaSQLConfig.Upload.IsDefault;
                uploadCheckAll.Checked = false;
                RefreshTextBox("upload");
            }

            // each page: comparefile
            if (Util.Contains(new string[] { "all", "compareFile1-IsDefault" }, list))
            {
                // refresh dependent UI
                compareFile1DDir.CheckState = ManaSQLConfig.CompareFile1.IsDefault ? CheckState.Checked : CheckState.Unchecked;
                compareFile1TempPath.CheckState = ManaSQLConfig.CompareFile1.UseTemp ? CheckState.Checked : CheckState.Unchecked;
                compareFile1Repo.ReadOnly = ManaSQLConfig.CompareFile1.IsDefault || ManaSQLConfig.CompareFile1.UseTemp;
                compareFile1Log.ReadOnly = ManaSQLConfig.CompareFile1.IsDefault || ManaSQLConfig.CompareFile1.UseTemp;
                isRefreshTextBox = "compareFile1";
            }
            if (Util.Contains(new string[] { "all", "compareFile1-IsTemp" }, list))
            {
                // refresh relevant checkbox
                compareFile1TempPath.CheckState = ManaSQLConfig.CompareFile1.UseTemp ? CheckState.Checked : CheckState.Unchecked;
                compareFile1DDir.CheckState = ManaSQLConfig.CompareFile1.IsDefault ? CheckState.Checked : CheckState.Unchecked;
                compareFile1Repo.ReadOnly = ManaSQLConfig.CompareFile1.IsDefault || ManaSQLConfig.CompareFile1.UseTemp;
                compareFile1Log.ReadOnly = ManaSQLConfig.CompareFile1.IsDefault || ManaSQLConfig.CompareFile1.UseTemp;
                isRefreshTextBox = "compareFile1";
            }

            //attempt to optimise paths
            if (isRefreshTextBox == "compareFile1")
            {
                RefreshTextBox(isRefreshTextBox);
                isRefreshTextBox = "";
            }

            if (Util.Contains(new string[] { "all", "compareFile2-IsDefault" }, list))
            {
                // refresh dependent UI
                compareFile2DDir.CheckState = ManaSQLConfig.CompareFile2.IsDefault ? CheckState.Checked : CheckState.Unchecked;
                compareFile2TempPath.CheckState = ManaSQLConfig.CompareFile2.UseTemp ? CheckState.Checked : CheckState.Unchecked;
                compareFile2Repo.ReadOnly = ManaSQLConfig.CompareFile2.IsDefault || ManaSQLConfig.CompareFile2.UseTemp;
                compareFile2Log.ReadOnly = ManaSQLConfig.CompareFile2.IsDefault || ManaSQLConfig.CompareFile2.UseTemp;
                isRefreshTextBox = "compareFile2";
            }

            if (Util.Contains(new string[] { "all", "compareFile2-IsTemp" }, list))
            {
                // refresh relevant checkbox
                compareFile2TempPath.CheckState = ManaSQLConfig.CompareFile2.UseTemp ? CheckState.Checked : CheckState.Unchecked;
                compareFile2DDir.CheckState = ManaSQLConfig.CompareFile2.IsDefault ? CheckState.Checked : CheckState.Unchecked;
                compareFile2Repo.ReadOnly = ManaSQLConfig.CompareFile2.IsDefault || ManaSQLConfig.CompareFile2.UseTemp;
                compareFile2Log.ReadOnly = ManaSQLConfig.CompareFile2.IsDefault || ManaSQLConfig.CompareFile2.UseTemp;
                isRefreshTextBox = "compareFile2";
            }

            //attempt to optimise paths
            if (isRefreshTextBox == "compareFile2")
            {
                RefreshTextBox(isRefreshTextBox);
                isRefreshTextBox = "";
            }

            // each page: compareDir
            if (Util.Contains(new string[] { "all", "compareDir-IsDefault" }, list))
            {
                compareDirDDir.CheckState = ManaSQLConfig.CompareDir.IsDefault ? CheckState.Checked : CheckState.Unchecked;
                compareDirRepo.ReadOnly = ManaSQLConfig.CompareDir.IsDefault;
                compareDirLog.ReadOnly = ManaSQLConfig.CompareDir.IsDefault;
                compareDirResult.ReadOnly = ManaSQLConfig.CompareDir.IsDefault;
                RefreshTextBox("compareDir");
            }

        }

        // Generic function, to DS
        // Updated on datastore (config) using checkboxes
        private void UpdateCheckBoxes(string list, bool flag)
        {
            if (Util.Contains(new string[] { "all", "dspExtract" }, list))
            {
                ManaSQLConfig.ShowExtract = flag;
            }
            if (Util.Contains(new string[] { "all", "dspCompareFile" }, list))
            {
                ManaSQLConfig.ShowCompareFile = flag;
            }
            if (Util.Contains(new string[] { "all", "enableLogging" }, list))
            {
                ManaSQLConfig.EnableLogging = flag;
            }

            // SVN menu
            if (Util.Contains(new string[] { "all", "dspSVNRepoStatus" }, list))
            {
                ManaSQLConfig.SvnRepoStatus = flag;
            }
            if (Util.Contains(new string[] { "all", "dspSVNCommit" }, list))
            {
                ManaSQLConfig.SvnCommit = flag;
            }
            if (Util.Contains(new string[] { "all", "dspSVNUpdate" }, list))
            {
                ManaSQLConfig.SvnUpdate = flag;
            }
            if (Util.Contains(new string[] { "all", "dspSVNLog" }, list))
            {
                ManaSQLConfig.SvnShowLog = flag;
            }
            if (Util.Contains(new string[] { "all", "dspSVNDiff" }, list))
            {
                ManaSQLConfig.SvnDiff = flag;
            }
            if (Util.Contains(new string[] { "all", "dspSVNMerge" }, list))
            {
                ManaSQLConfig.SvnMerge = flag;
            }
            if (Util.Contains(new string[] { "all", "dspSVNBlame" }, list))
            {
                ManaSQLConfig.SvnBlame = flag;
            }

            // each page: extract
            if (Util.Contains(new string[] { "all", "extractDDir" }, list))
            {
                ManaSQLConfig.Extract.IsDefault = flag;
            }

            // each page: upload
            if (Util.Contains(new string[] { "all", "uploadDDir" }, list))
            {
                ManaSQLConfig.Upload.ResetWhereFiles(false);
                ManaSQLConfig.Upload.IsDefault = flag;
            }

            // each page: compare file
            if (Util.Contains(new string[] { "all", "compareFile1DDir" }, list))
            {
                ManaSQLConfig.CompareFile1.IsDefault = flag;
            }
            if (Util.Contains(new string[] { "all", "compareFile2DDir" }, list))
            {
                ManaSQLConfig.CompareFile2.IsDefault = flag;
            }
            if (Util.Contains(new string[] { "all", "compareFile1TempPath" }, list))
            {
                ManaSQLConfig.CompareFile1.UseTemp = flag;
            }
            if (Util.Contains(new string[] { "all", "compareFile2TempPath" }, list))
            {
                ManaSQLConfig.CompareFile2.UseTemp = flag;
            }

            // each page: compare dir
            if (Util.Contains(new string[] { "all", "compareDirDDir" }, list))
            {
                ManaSQLConfig.CompareDir.IsDefault = flag;
            }
        }

        // START: COMBOBOXES
        // Generic function, from DS
        // Based on datastore (config), update buttons 
        public void RefreshComboBox(string list)
        {
            if (Util.Contains(new string[] { "all", "general-ServerNamingIndex" }, list))
            {
                serverNaming.SelectedIndex = ManaSQLConfig.ServerNamingIndex;
            }
            if (Util.Contains(new string[] { "all", "general-LogNamingIndex" }, list))
            {
                logNaming.SelectedIndex = ManaSQLConfig.LogNamingIndex;
                logNaming.Enabled = ManaSQLConfig.EnableLogging;
            }
        }
        public void UpdateComboBox(string list, int val)
        {
            if (Util.Contains(new string[] { "all", "serverNaming" }, list))
            {
                ManaSQLConfig.ServerNamingIndex = serverNaming.SelectedIndex;
            }
            if (Util.Contains(new string[] { "all", "logNaming" }, list))
            {
                ManaSQLConfig.LogNamingIndex = logNaming.SelectedIndex;
            }
        }

        // START: BUTTONS
        // Generic function, from DS
        // Based on datastore (config), update buttons 
        public void RefreshButton(string list)
        {
            bool isListed;

            //Note: 
            //1. SqlMana will automatically create directory to write files into 
            //2. Extract + compareFile doesn NOT require any directory checks
            //3. Upload + compareDir MUST check existance of directory

            if (Util.Contains(new string[] { "all", "extract" }, list))
            {

                isListed =
                   ManaSQLConfig.ValidGenPaths
                   && ManaSQLConfig.Extract.ValidPaths
                   && extractSSP.Items.Count > 0
                   ;
                extractList.Enabled = isListed;
                extractClear.Enabled = isListed;
                extractSSP.Enabled = isListed;
            }

            if (Util.Contains(new string[] { "all", "upload" }, list))
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
            }

            if (Util.Contains(new string[] { "all", "compareFile1", "compareFile2" }, list))
            {
                isListed =
                    ManaSQLConfig.ValidGenPaths
                    && ManaSQLConfig.CompareFile1.ValidPaths
                    && ManaSQLConfig.CompareFile2.ValidPaths
                    && ManaSQLConfig.CompareFile1.WhereSSPList.Count == 1
                    && ManaSQLConfig.CompareFile2.WhereSSPList.Count == 1
                    ;

                bool isFileExist1 = ManaSQLConfig.CompareFile1.ExistsSSPFilename(0);
                bool isFileExist2 = ManaSQLConfig.CompareFile2.ExistsSSPFilename(0);

                compareFileWrite.Enabled = isListed;
                compareFileCompare.Enabled = isListed && isFileExist1 && isFileExist2;
                compareFileUpload1.Enabled = isListed && isFileExist1;
                compareFileUpload2.Enabled = isListed && isFileExist2;
            }

            if (Util.Contains(new string[] { "all", "compareDir" }, list))
            {
                isListed =
                    ManaSQLConfig.ValidGenPaths
                    && ManaSQLConfig.CompareDir.ValidPaths
                    ;
                compareDirCompare.Enabled = isListed;
            }
        }

        // START: LABELS
        // Generic function, from DS
        // Based on datastore (config), update labels
        private void RefreshLabel(string list)
        {
            if (Util.Contains(new string[] { "all", "extract" }, list))
            {
                extractServer.Text = ManaSQLConfig.Extract.SERVER;
                extractDB.Text = ManaSQLConfig.Extract.DB;
            }
            if (Util.Contains(new string[] { "all", "upload" }, list))
            {
                uploadServer.Text = ManaSQLConfig.Upload.SERVER;
                uploadDB.Text = ManaSQLConfig.Upload.DB;
            }
            if (Util.Contains(new string[] { "all", "compareFile1" }, list))
            {
                compareFile1Server.Text = ManaSQLConfig.CompareFile1.SERVER;
                compareFile1DB.Text = ManaSQLConfig.CompareFile1.DB;
                compareFile1Obj.Text = string.Format(@"[{0}]", ManaSQLConfig.CompareFile1.OBJ);
            }
            if (Util.Contains(new string[] { "all", "compareFile2" }, list))
            {
                compareFile2Server.Text = ManaSQLConfig.CompareFile2.SERVER;
                compareFile2DB.Text = ManaSQLConfig.CompareFile2.DB;
                compareFile2Obj.Text = string.Format(@"[{0}]", ManaSQLConfig.CompareFile2.OBJ);
            }
            if (Util.Contains(new string[] { "all", "compareDirLogLocationPath" }, list))
            {
                compareDirLogLocationPath.Text = ManaSQLConfig.CompareDir.GetLogPath("[logFilename]");
            }
            if (Util.Contains(new string[] { "all", "compareDirResultLocationPath" }, list))
            {
                compareDirResultLocationPath.Text = ManaSQLConfig.CompareDir.GetOutPath();
            }

            if (Util.Contains(new string[] { "all", "upload-WhereFileStore" }, list))
            {
                uploadSSPSelected.Items.Clear();
                uploadSSPSelected.Items.AddRange(ManaSQLConfig.Upload.WhereFileList.ToArray());
            }
        }

        // START: LISTBOXES
        // Generic function, from DS
        // Based on datastore (config), update listboxes
        private void RefreshListBoxItems(string list)
        {
            if (Util.Contains(new string[] { "all", "extract", "extract-WhereSSPStore" }, list))
            {
                extractSSP.Items.Clear();
                extractSSP.Items.AddRange(ManaSQLConfig.Extract.WhereSSPList.ToArray());
                RefreshButton("extract");
            }
            if (Util.Contains(new string[] { "all", "upload" }, list))
            {
                bool isActive =
                    ManaSQLConfig.ValidGenPaths
                    && ManaSQLConfig.Upload.ValidPaths
                    && ManaSQLConfig.Upload.ValidRepoPath
                    ;

                if (isActive)
                {
                    FilterUploadList();
                }
                else
                {
                    uploadSSP.Items.Clear();
                }
                RefreshButton("upload");
            }
            if (Util.Contains(new string[] { "all", "general-CustomPaths" }, list))
            {
                customPathList.Items.Clear();
                customPathList.Items.AddRange(ManaSQLConfig.GetCustomPaths().ToArray());
            }
            if (Util.Contains(new string[] { "all", "general-CustomRepoPaths" }, list))
            {
                customRepoPaths.Items.Clear();
                customRepoPaths.Items.AddRange(ManaSQLConfig.RPMan.ArrayPaths());
            }
        }

        private void FilterUploadList()
        {
            List<string> filenames = Util.GetFilesWithExtension(ManaSQLConfig.Upload.FormRepoPath(), ManaSQLConfig.Extension);
            if (filterText != null && filterText != "")
            {
                for (int i = filenames.Count - 1; i > -1; i--)
                {
                    if (!Regex.IsMatch(
                        filenames[i]
                        , filterText
                        , filterTextCasing ? RegexOptions.IgnoreCase : RegexOptions.None)
                        )
                        filenames.RemoveAt(i);
                }
            }

            uploadSSP.Items.Clear();
            uploadSSP.Items.AddRange(filenames.ToArray());
        }

        private void RefreshSelectedListBoxItems(string list)
        {
            if (Util.Contains(new string[] { "all", "upload", "upload-WhereFileStore" }, list))
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
            if (Util.Contains(new string[] { "all", "extractSSP" }, list))
            {
                ManaSQLConfig.Extract.ResetWhereSSP();
            }
            // Data not stored in DS, refer straight in Visual
            //if (Util.Contains(items, "all") || Util.Contains(items, "uploadSSP"))
            //{
            //}
            if (Util.Contains(new string[] { "all", "customPaths" }, list))
            {
                ManaSQLConfig.ResetCustomPaths();
            }
        }

        private void UpdateListBoxItems(string list, string[] vals)
        {
            if (Util.Contains(new string[] { "all", "extractSSP" }, list))
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
            if (Util.Contains(new string[] { "all", "customPathList" }, list))
            {
                for (int i = 0; i < vals.Length; i++)
                {
                    if (Util.ValidatePath(vals[i]))
                    {
                        ManaSQLConfig.AddCustomPath(vals[i]);
                    }
                }
            }

            if (Util.Contains(new string[] { "all", "customRepoPathList" }, list))
            {
                for (int i = 0; i < vals.Length; i++)
                {
                    ManaSQLConfig.AddCustomRepoPath(vals[i]);
                }
            }
        }

        // method overloading: for list with multiple string selections
        private void UpdateSelectedListBoxItems(string list, string[] vals)
        {
            if (Util.Contains(new string[] { "all", "uploadSSP" }, list))
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
            if (Util.Contains(new string[] { "all", "customPathList" }, list))
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

        private void FilterUploadListLeave(object sender, EventArgs e)
        {
            var control = sender as TextBox;
            filterText = control.Text;
            FilterUploadList();
        }

        private void FilterUploadListKey(object sender, KeyEventArgs e)
        {
            var control = sender as TextBox;
            if (e.KeyCode == Keys.Enter)
            {
                filterText = control.Text;
                FilterUploadList();
            }
        }

        private void FilterUploadListCase(object sender, EventArgs e)
        {
            var control = sender as CheckBox;
            filterTextCasing = control.CheckState == CheckState.Checked;
            FilterUploadList();
        }
    }
}
