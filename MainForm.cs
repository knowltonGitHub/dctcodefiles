using DCT;
using DCT.Classes;
using DCT.Properties;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DCT
{
    public enum MouseActionEnum
    {
        Enter,
        Leave
    }

    public enum EditItemState
    {
        NewTag,
        NewItem,
        ExistingItem,
        ExistingTag,
        ExistingValue,
        None
    }

    public partial class FormMain : Form, ISupportsEvents
    { 
        public NVM _nvm = new NVM();

        private EditItemState _eis;
        UDPSocket _s = new UDPSocket();
        UDPSocket _c = new UDPSocket();
        private CustomEvent cue = new CustomEvent();
        public CustomEvents ce { get { return _c.ce; } set { _c.ce = value; } }

        //public NVM NVMCont
        //{
        //    get { return _nvm; }
        //}

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Helper.L(textBoxLog, "hi");

            UserLogin ul = new UserLogin();
            // ul.ShowDialog();
            _nvm = ul.NVMCont;
            _eis = EditItemState.None;
            AdjustMainFormControls();
            LoadInitialData();

            _c.ce.ApplicationEvent += this.Ce_ApplicationEvent;

            ce.ApplicationEvent += this.Ce_ApplicationEvent1;

            ce.AppEvent(new CustomEventArgs("_nvm online", EventType.tagclick));

            this.Text = this.Text + " - " + _nvm.UserName;

            ChangeStatusTextAndColor("ready", Color.Green);


            //dataGridViewAllData.DataSource = _nvm.TIVManager._tivList.ToArray();
            //dataGridViewAllData.Visible = true;
        }

        private void FillTagsListFromDatabase()
        {
            listBoxTags.Items.Clear();

            _nvm.FillTagItemValuesFromDatabase();

            foreach(var a in _nvm.TIVManager._tivList)
            {
                Helper.L(textBoxLog, a._value);
            }

            //Helper.CreateCopyOfCurrentDB(_nvm.TIVManager.GetTagItemValueCOUNT, "APP_OPEN_FIRST_MEMORY_FILL");

            FillTagsListBoxFromInMemoryTagsList();
        }

        private void FillTagsListBoxFromInMemoryTagsList()
        {
            listBoxTags.Items.Clear();
            listBoxTags.Items.AddRange(_nvm.TIVManager.GetTags(_nvm.UserID).ToArray());
        }

        private void FillItemsListBasedOnTagSelected(string lastTagSelected)
        {
            EnableControl(groupBoxItems, true);

            listBoxItems.Items.Clear();

            listBoxItems.Items.AddRange(_nvm.TIVManager.GetItems(listBoxTags.Text, _nvm.UserID).ToArray());

            textBoxItemValue.Text = _nvm.EMPTYSTRING;
            EnableControl(groupBoxValue, false);
            EnableControl(buttonAddNewItem, true);
        }

        private void Ce_ApplicationEvent1(object sender, CustomEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.message.ToString() + System.Environment.NewLine);
        }
        private void AdjustMainFormControls()
        {
            //Resize Tag Controls
            groupBoxTags.Top = 10;
            groupBoxTags.Left = 10;
            groupBoxTags.Height = this.Height - 100;
            groupBoxItems.Top = groupBoxTags.Top;
            groupBoxValue.Top = groupBoxItems.Top;
            groupBoxItems.Height = groupBoxTags.Height;
            groupBoxValue.Height = groupBoxItems.Height;

            listBoxTags.Top = groupBoxTags.Top + 10;
            listBoxTags.Left = groupBoxTags.Left + 5;
            listBoxTags.Height = groupBoxTags.Height - 100;

            listBoxItems.Top = listBoxTags.Top;
            textBoxItemValue.Top = listBoxItems.Top;
            listBoxItems.Height = listBoxTags.Height;

            textBoxItemValue.Height = listBoxItems.Height;

            buttonAddNewTag.Top = listBoxTags.Bottom - 100;
            buttonAddNewTag.Left = listBoxTags.Left + listBoxTags.Width + 2;
            buttonDeleteSelectedTag.Top = groupBoxTags.Top + listBoxTags.Top + listBoxTags.Height - buttonDeleteSelectedTag.Height - 8;
            buttonDeleteSelectedTag.Left = listBoxTags.Left + listBoxTags.Width + 2;

            buttonAddNewItem.Top = buttonAddNewTag.Top;
            buttonAddNewItem.Left = listBoxItems.Left + listBoxItems.Width + 2;
            buttonDeleteSelectedItem.Top = buttonDeleteSelectedTag.Top;
            buttonDeleteSelectedItem.Left = buttonAddNewItem.Left;
            buttonOpenWithChrome.Top = textBoxItemValue.Bottom + 10;
            buttonEmail.Top = textBoxItemValue.Bottom + 10;
            buttonExportFile.Top = textBoxItemValue.Bottom + 10;
        }
        private void FormMain_SizeChanged(object sender, EventArgs e)
        {
            AdjustMainFormControls();
        }
        private void LoadInitialData()
        {
            FillTagsListFromDatabase();
            EnableControl(buttonAddNewTag, true);
            EnableControl(buttonDeleteSelectedTag, false);
            EnableControl(groupBoxItems, false);
            EnableControl(groupBoxValue, false);
        }
        private void buttonAddNewTag_Click(object sender, EventArgs e)
        {
            panelEnterOrEditText.Visible = true;
            panelEnterOrEditText.Left = groupBoxTags.Left + listBoxTags.Left;
            panelEnterOrEditText.Top = listBoxTags.Top + 100;
            textBoxNewTagOrItem.Text = _nvm.EMPTYSTRING;
            textBoxNewItemValueCandidate.Visible = false;
            panelEnterOrEditText.Height = 40;

            EnableControl(buttonAddNewTag, false);
            EnableControl(buttonDeleteSelectedTag, false);
            EnableControl(groupBoxItems, false);
            EnableControl(groupBoxValue, false);

            _eis = EditItemState.NewTag;
        }
        private void buttonDeleteSelectedTag_Click(object sender, EventArgs e)
        {
            //how many items associated with the tag being deleted?  If > 0 cannot deleted the tag
            int itemCount = _nvm.TIVManager.GetItems(listBoxTags.Text, _nvm.UserID).Count;
           
            bool removed = false;

            //If item count > 0 cannot deleted the parent tag
            if (itemCount == 0)
            {
                removed = _nvm.TIVManager.RemoveTag(listBoxTags.Text, _nvm.UserID);

                if(removed)
                {
                    ChangeStatusTextAndColor(listBoxTags.Text + " was removed.  Tags list has been refreshed", Color.Green);
                }
            }
            else
            {
                ChangeStatusTextAndColor("Before deleting a tag, all associated items must be deleted first", Color.Red);
            }
                        
            FillTagsListBoxFromInMemoryTagsList();
            EnableControl(buttonAddNewTag, true);
            EnableControl(buttonDeleteSelectedTag, false);
        }

        private void ChangeStatusTextAndColor(string message, Color c)
        {
            toolStripStatusLabelDCT.Text = message;
            toolStripStatusLabelDCT.ForeColor = c;
        }

        private void listBoxTags_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ListBox)sender).SelectedIndex > -1)
            {
                EnableControl(buttonDeleteSelectedTag, true);
                EnableControl(buttonDeleteSelectedItem, false);
                FillItemsListBasedOnTagSelected(((ListBox)sender).Text.Trim());
                ChangeStatusTextAndColor(listBoxTags.Text + " was selected.  It has " + listBoxItems.Items.Count + " item(s) associated with it", Color.Green);
            }
            else
            {
                EnableControl(buttonDeleteSelectedTag, false);        
            }
        }
        private void textBoxNewTag_TextChanged(object sender, EventArgs e)
        {

        }

        private void EnableControls(List<Control> cnts, bool enable)
        {
            foreach (Control c in cnts)
            {
                c.Enabled = enable;
            }
        }
        private void EnableControl(Control c, bool enable)
        {
            c.Enabled = enable;
        }
        private void listBoxItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowItemValueForItemSelected();           
        }

        private void ShowItemValueForItemSelected()
        {
            EnableControl(groupBoxItems, true);
            EnableControl(listBoxItems, true);
            EnableControl(buttonDeleteSelectedItem, true);

            if (listBoxTags.Text != "" && listBoxItems.Text != "")
            {

                textBoxItemValue.Text = _nvm.TIVManager.GetItemValue(listBoxTags.Text,
                    listBoxItems.Text,
                    _nvm.UserID);
            }

            if (textBoxItemValue.Text != "")
            {
                Clipboard.SetText(textBoxItemValue.Text);
                EnableControl(groupBoxValue, true);
                textBoxItemValue.ReadOnly = true;
                EnableControl(buttonEmail, true);
                EnableControl(buttonOpenWithChrome, false);

                if (_nvm.ItemValueIsAWebURL(textBoxItemValue.Text))
                {
                    EnableControl(buttonOpenWithChrome, true);
                }
            }
        }

        private void ShowMessage(string tempmessage, string tempcaption)
        {
            MessageBox.Show(this, tempmessage, tempcaption);
        }
        private void EnterIntoEditMode()
        {
            //textBox1.Text += "entering edit mode..." + System.Environment.NewLine;

            //_eis = EditItemState.ExistingItem;
            //EnableControl(textBoxItemValue, true);
            //Tag = listBoxTags.Text;
            //ItemName = listBoxItems.Text;
            //EnableControl(listBoxItems, false);
            //EnableControl(buttonDeleteSelectedItem, false);
            //EnableControl(listBoxTags, false);
            //EnableControl(buttonAddNewTag, false);
            //EnableControl(buttonDeleteSelectedTag, false);
            //EnableControl(buttonAddNewItem, true);
            //EnableControl(buttonOpenWithChrome, false);
            //EnableControl(buttonEmail, false);         
        }
        private void buttonEditSelectedItem_Click(object sender, EventArgs e)
        {
            //EnterIntoEditMode();            
        }
        private void buttonDeleteSelectedItem_Click(object sender, EventArgs e)
        {
            if (_nvm.SelectedIndexIsValid(listBoxItems.SelectedIndex))
            {
                string lbtag = listBoxTags.Text;
                TagItemValue tiv = new TagItemValue(lbtag, listBoxItems.Text, textBoxItemValue.Text, _nvm.UserID);
                bool removed = _nvm.TIVManager.RemoveItem(lbtag, listBoxItems.Text, _nvm.UserID);
                EnableControl(textBoxItemValue, true);
                textBoxItemValue.Text = _nvm.EMPTYSTRING;
                FillItemsListBasedOnTagSelected(lbtag);
            }
        }
        private void textBoxNewItem_TextChanged(object sender, EventArgs e)
        {

        }
        private void EnterIntoNewItemMode()
        {

        }
        #region Mouse Hover Handling
        private System.Drawing.Bitmap GetMouseOverBitmap(string buttonName, MouseActionEnum mouseAction)
        {
            System.Drawing.Bitmap tempBitmap = null;


            switch (buttonName)
            {
                case "buttonAddNewTag":
                case "buttonAddNewItem":
                    if (mouseAction == MouseActionEnum.Enter)
                    {
                        tempBitmap = Properties.Resources.item_add_hover;
                    }
                    else
                    {
                        tempBitmap = Resources.item_add;
                    }
                    break;
                case "buttonDeleteSelectedTag":
                case "buttonDeleteSelectedItem":

                    if (mouseAction == MouseActionEnum.Enter)
                    {
                        tempBitmap = Resources.item_delete_hover;
                    }
                    else
                    {
                        tempBitmap = Resources.item_delete;
                    }
                    break;
                case "buttonEditSelectedItem":
                    if (mouseAction == MouseActionEnum.Enter)
                    {
                        tempBitmap = Resources.item_edit_hover;
                    }
                    else
                    {
                        tempBitmap = Resources.item_edit;
                    }
                    break;
                case "buttonOpenWithChrome":
                    if (mouseAction == MouseActionEnum.Enter)
                    {
                        tempBitmap = Resources.chrome_hover;
                    }
                    else
                    {
                        tempBitmap = Resources.chrome_icon;
                    }
                    break;
                case "buttonEmail":
                    if (mouseAction == MouseActionEnum.Enter)
                    {
                        tempBitmap = Resources.envelope_green;
                    }
                    else
                    {
                        tempBitmap = Resources.envelope;
                    }
                    break;
                case "buttonExportFile":
                    if (mouseAction == MouseActionEnum.Enter)
                    {
                        tempBitmap = Resources.fileexporthover;
                    }
                    else
                    {
                        tempBitmap = Resources.fileexport;
                    }
                    break;
            }

            return tempBitmap;
        }
        private void ButtonMouseEnter(object sender, EventArgs e)
        {
            _nvm.ChangeButtonImageResource((System.Windows.Forms.Button)sender, GetMouseOverBitmap(((System.Windows.Forms.Button)sender).Name, MouseActionEnum.Enter));
        }
        private void ButtonMouseLeave(object sender, EventArgs e)
        {
            _nvm.ChangeButtonImageResource((System.Windows.Forms.Button)sender, GetMouseOverBitmap(((System.Windows.Forms.Button)sender).Name, MouseActionEnum.Leave));
        }

        #endregion

        private void buttonAddNewItem_Click(object sender, EventArgs e)
        {
            bool tagHasValue = listBoxTags.Text.Length > 0 ? true : false;

            if (tagHasValue)
            {
                Tag = listBoxTags.Text;

                panelEnterOrEditText.Left = groupBoxItems.Left + listBoxItems.Left;
                panelEnterOrEditText.Top = listBoxItems.Top + 100;
                textBoxNewTagOrItem.Text = _nvm.EMPTYSTRING;
                panelEnterOrEditText.Height = 200;
                textBoxNewItemValueCandidate.Top = textBoxNewTagOrItem.Top + textBoxNewTagOrItem.Height + 5;
                textBoxNewItemValueCandidate.Height = panelEnterOrEditText.Height - 50;
                textBoxNewItemValueCandidate.Left = textBoxNewTagOrItem.Left;
                textBoxNewItemValueCandidate.Width = textBoxNewTagOrItem.Width;
                panelEnterOrEditText.Visible = true;
                textBoxNewItemValueCandidate.Visible = true;

                EnableControl(buttonAddNewTag, false);
                EnableControl(buttonDeleteSelectedTag, false);
                EnableControl(groupBoxItems, true);
                EnableControl(groupBoxValue, false);

                _eis = EditItemState.NewItem;
            }
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm af = new AboutForm();
            af.ShowDialog();
        }
        private void textBoxNewItem_Enter(object sender, EventArgs e)
        {
            EnterIntoNewItemMode();
        }
        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Preferences pForm = new Preferences();
            pForm.NVMCont = _nvm;

            pForm.ShowDialog();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void buttonEmail_Click(object sender, EventArgs e)
        {
            GetGMailToAddress gmail = new GetGMailToAddress();
            gmail.ShowDialog();
        }
        private void textBoxSendMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SendUDPClientMessage(textBoxSendMessage.Text);
            }
        }
        

        private void Ce_ApplicationEvent(object sender, CustomEventArgs e)
        {
            // textBoxChatMessages.Text += e.message.ToString() + System.Environment.NewLine;
        }
        private void SendUDPClientMessage(string message)
        {
            //_c.Client("127.0.0.1", 27000);
            //_c.Send(message);
            string[] a = new string[] { "affasd" };
            SendNewMessage(a);
        }
        private void RunUDPServer()
        {
            // _s.Server("127.0.0.1", 27000);

            StartListener();
        }
        private void StartListener()
        {
            using (var server = new ResponseSocket())
            {
                server.Bind("tcp://*:5556");
                string msg = server.ReceiveFrameString();
                Console.WriteLine("From Client: {0}", msg);
                server.SendFrame("World");
            }
        }
        private void SendNewMessage(string[] args)
        {
            using (var client = new RequestSocket())
            {
                client.Connect("tcp://127.0.0.1:5556");
                client.SendFrame("Hello");
                var msg = client.ReceiveFrameString();
                Console.WriteLine("From Server: {0}", msg);
            }
        }
        private void trymq()
        {
            using (var responseSocket = new ResponseSocket("@tcp://*:5555"))
            using (var requestSocket = new RequestSocket(">tcp://localhost:5555"))
            {
                Console.WriteLine("requestSocket : Sending 'Hello'");
                requestSocket.SendFrame("Hello");
                var message = responseSocket.ReceiveFrameString();
                Console.WriteLine("responseSocket : Server Received '{0}'", message);
                Console.WriteLine("responseSocket Sending 'World'");
                responseSocket.SendFrame("World");
                message = requestSocket.ReceiveFrameString();
                Console.WriteLine("requestSocket : Received '{0}'", message);
                Console.ReadLine();
            }
        }
        private void buttonServerMode_Click(object sender, EventArgs e)
        {
            //RunUDPServer();

            trymq();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            textBoxLog.Text = "";
        }
        private void textBoxValue_DoubleClick(object sender, EventArgs e)
        {
            panelEnterOrEditText.Visible = true;
            textBoxNewItemValueCandidate.Visible = true;
            panelEnterOrEditText.Height = 40;
            panelEnterOrEditText.Left = groupBoxItems.Left + listBoxItems.Left;
            panelEnterOrEditText.Top = listBoxItems.Top + 100;
            textBoxNewItemValueCandidate.Text = textBoxItemValue.Text;
            textBoxNewTagOrItem.Text = listBoxItems.Text;
            EnableControl(textBoxNewTagOrItem, false);
            panelEnterOrEditText.Height = 200;
            textBoxNewItemValueCandidate.Top = textBoxNewTagOrItem.Top + textBoxNewTagOrItem.Height + 5;
            textBoxNewItemValueCandidate.Height = panelEnterOrEditText.Height - 50;
            textBoxNewItemValueCandidate.Left = textBoxNewTagOrItem.Left;
            textBoxNewItemValueCandidate.Width = textBoxNewTagOrItem.Width;

            EnableControl(buttonAddNewTag, false);
            EnableControl(buttonDeleteSelectedTag, false);
            EnableControl(groupBoxItems, false);
            EnableControl(groupBoxValue, false);

            _eis = EditItemState.ExistingValue;
        }

        //User clicked Cancel on pop up panel to edit existing / post new tag / item
        private void buttonCancelText_Click(object sender, EventArgs e)
        {
            List<Control> controlsToEnable = new List<Control>() { };
            List<Control> controlsToDisable = new List<Control>() { };

            //Determine the current state of the application 
            bool existingtag = (_eis == EditItemState.ExistingTag);
            bool newtag = (_eis == EditItemState.NewTag);
            bool existingitem = (_eis == EditItemState.ExistingItem);
            bool newitem = (_eis == EditItemState.NewItem);
            bool existingvalue = (_eis == EditItemState.ExistingValue);
            bool istag = existingtag & newtag;
            bool isitem = existingitem & newitem;

            //Reset text boxes
            textBoxNewTagOrItem.Text = string.Empty;
            textBoxNewItemValueCandidate.Text = string.Empty;
            panelEnterOrEditText.Visible = false;

            controlsToEnable.Add(buttonAddNewTag);

            if (existingvalue)
            {
                controlsToEnable.Add(groupBoxItems);
                controlsToEnable.Add(listBoxItems);
                controlsToEnable.Add(textBoxItemValue);
            }

            if (istag)
            {
                controlsToDisable.Add(buttonDeleteSelectedTag);
            }

            if (isitem)
            {
                controlsToEnable.Add(groupBoxItems);
                controlsToEnable.Add(listBoxItems);
                controlsToEnable.Add(buttonAddNewItem);
                controlsToDisable.Add(buttonDeleteSelectedItem);
            }

            EnableControls(controlsToEnable, true);
            EnableControls(controlsToDisable, false);

            _eis = EditItemState.None;
        }

        private void buttonSaveText_Click(object sender, EventArgs e)
        {
            TagItemValue oldtiv;
            TagItemValue newtiv;

            string useTagForRefresh = "";
            string useItemForRefresh = "";

            if (_eis == EditItemState.NewTag)
            {
                if(textBoxNewTagOrItem.Text.Trim() != "")
                {
                    TagItemValue tiv = new TagItemValue(textBoxNewTagOrItem.Text,null,null,null);
                    if(!_nvm.TIVManager._tivList.Contains(tiv))
                    {
                        _nvm.TIVManager._tivList.Add(tiv);
                    }
                }
               
                useTagForRefresh = textBoxNewTagOrItem.Text.Trim();
            }

            if (_eis == EditItemState.NewItem)
            {
                newtiv = new TagItemValue(listBoxTags.Text, textBoxNewTagOrItem.Text.Trim(), textBoxNewItemValueCandidate.Text.Trim(), _nvm.UserID);                
                if (!_nvm.TIVManager._tivList.Contains(newtiv))
                {
                    _nvm.TIVManager._tivList.Add(newtiv);
                }
                useTagForRefresh = listBoxTags.Text;
                useItemForRefresh = newtiv._item;
            }

            if (_eis == EditItemState.ExistingItem)
            {
                oldtiv = new TagItemValue(listBoxTags.Text, listBoxItems.Text.Trim(), textBoxItemValue.Text.Trim(), _nvm.UserID);
                newtiv = new TagItemValue(listBoxTags.Text, textBoxNewTagOrItem.Text.Trim(), textBoxNewItemValueCandidate.Text.Trim(), _nvm.UserID);
                _nvm.TIVManager._tivList.Remove(oldtiv);
                _nvm.TIVManager._tivList.Add(newtiv);

                useTagForRefresh = listBoxTags.Text;
                useItemForRefresh = newtiv._item;
            }

            if (_eis == EditItemState.ExistingValue)
            {
                oldtiv = new TagItemValue(listBoxTags.Text, listBoxItems.Text.Trim(), textBoxItemValue.Text.Trim(), _nvm.UserID);
                newtiv = new TagItemValue(listBoxTags.Text, listBoxItems.Text.Trim(), textBoxNewItemValueCandidate.Text.Trim(), _nvm.UserID);
                
                _nvm.TIVManager._tivList.Remove(oldtiv);
                _nvm.TIVManager._tivList.Add(newtiv);

                useTagForRefresh = listBoxTags.Text;
                useItemForRefresh = newtiv._item;
            }

            if (_eis == EditItemState.ExistingTag)
            {
                useTagForRefresh = listBoxTags.Text;

                oldtiv = new TagItemValue(listBoxTags.Text, listBoxItems.Text.Trim(), textBoxItemValue.Text.Trim(), _nvm.UserID);
                newtiv = new TagItemValue(textBoxNewTagOrItem.Text, listBoxItems.Text.Trim(), textBoxItemValue.Text.Trim(), _nvm.UserID);
                _nvm.TIVManager.RemoveItem(useTagForRefresh, useItemForRefresh, _nvm.UserID);
                _nvm.TIVManager._tivList.Add(newtiv);                
            }

            //need to fix UserID ... it is not getting saved for new items or updates  
            FillTagsListBoxFromInMemoryTagsList();
            FillItemsListBasedOnTagSelected(useTagForRefresh);
            listBoxTags.Text = useTagForRefresh;
            listBoxItems.Text = useItemForRefresh;         

            EnableControl(buttonAddNewTag, true);
            EnableControl(buttonDeleteSelectedTag, true);

            textBoxNewTagOrItem.Text = _nvm.EMPTYSTRING;
            textBoxNewItemValueCandidate.Text = _nvm.EMPTYSTRING;
            panelEnterOrEditText.Visible = false;
        }

        private void buttonOpenWithChrome_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                FileName = Environment.GetEnvironmentVariable("ProgramFiles(x86)") + @"\Google\Chrome\Application\chrome.exe",
                Arguments = textBoxItemValue.Text
            };

            process.Start();           
        }

        private void listBoxTags_DoubleClick(object sender, EventArgs e)
        {
            panelEnterOrEditText.Visible = true;
            panelEnterOrEditText.Left = groupBoxTags.Left + listBoxTags.Left;
            panelEnterOrEditText.Top = listBoxTags.Top + 100;
            textBoxNewTagOrItem.Text = listBoxTags.Text;
            textBoxNewItemValueCandidate.Visible = false;
            panelEnterOrEditText.Height = 40;

            EnableControl(buttonAddNewTag, false);
            EnableControl(buttonDeleteSelectedTag, false);
            EnableControl(groupBoxItems, false);
            EnableControl(groupBoxValue, false);

            _eis = EditItemState.ExistingTag;
        }
        private void listBoxItems_DoubleClick(object sender, EventArgs e)
        {
            panelEnterOrEditText.Visible = true;
            textBoxNewItemValueCandidate.Visible = false;
            panelEnterOrEditText.Height = 40;
            panelEnterOrEditText.Left = groupBoxItems.Left + listBoxItems.Left;
            panelEnterOrEditText.Top = listBoxItems.Top + 100;
            textBoxNewTagOrItem.Text = listBoxItems.Text;
            textBoxNewItemValueCandidate.Visible = false;
            panelEnterOrEditText.Height = 200;
            textBoxNewItemValueCandidate.Top = textBoxNewTagOrItem.Top + textBoxNewTagOrItem.Height + 5;
            textBoxNewItemValueCandidate.Height = panelEnterOrEditText.Height - 50;
            textBoxNewItemValueCandidate.Left = textBoxNewTagOrItem.Left;
            textBoxNewItemValueCandidate.Width = textBoxNewTagOrItem.Width;

            EnableControl(buttonAddNewTag, false);
            EnableControl(buttonDeleteSelectedTag, false);
            EnableControl(groupBoxItems, false);
            EnableControl(groupBoxValue, false);

            _eis = EditItemState.ExistingItem;
        }

        private void buttonExportFile_Click(object sender, EventArgs e)
        {
            ExportCurrentData();
        }

        private void ExportCurrentData()
        {
            string ODD_DELIMITER = "Ü"; // ALT + 666
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\Export" + Helper.ReturnDateTimeStampAsPseudoJulian(new DateTime()) + ".txt");

            foreach (TagItemValue tiv in _nvm.TIVManager._tivList)
            {
                string temptag = (tiv._tag == null) ? null : (tiv._tag);
                string tempitem = (tiv._item == null) ? null : (tiv._item);
                string tempvalue = (tiv._value == null) ? null : (tiv._value);

                sw.WriteLine(temptag + ODD_DELIMITER + tempitem + ODD_DELIMITER + tempvalue);
            }

            sw.Close();

            MessageBox.Show("done exporting file");
        }

        private void buttonExportFile_MouseEnter(object sender, EventArgs e)
        {
            _nvm.ChangeButtonImageResource((System.Windows.Forms.Button)sender, GetMouseOverBitmap(((System.Windows.Forms.Button)sender).Name, MouseActionEnum.Enter));
        }

        private void buttonExportFile_MouseLeave(object sender, EventArgs e)
        {
            _nvm.ChangeButtonImageResource((System.Windows.Forms.Button)sender, GetMouseOverBitmap(((System.Windows.Forms.Button)sender).Name, MouseActionEnum.Leave));
        }

        private void FormMain_FormClosed_1(object sender, FormClosedEventArgs e)
        {
            _nvm.OverwriteTagItemsValueTableInDatabase();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Helper.CreateCopyOfCurrentDB(999, "manual_backup");
        }

       
        private void tabPageClipboard_MouseClick(object sender, MouseEventArgs e)
        {
            ChangeStatusTextAndColor("ready", Color.Green);
        }

        private void textBoxItemValue_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonUpdateDatabaseVIEW_Click(object sender, EventArgs e)
        {
            try
            {
                dataGridViewAllData.DataSource = _nvm.TIVManager._tivList.ToArray();

                dataGridViewAllData.DataSource = "asdfsdaf";
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void buttondatagridvisible_Click(object sender, EventArgs e)
        {
            if(dataGridViewAllData.Visible)
            {
                dataGridViewAllData.Visible = false;
            }
            else
            {
                dataGridViewAllData.Visible = true;
            }
        }
    }//end of class
}//end of namespace