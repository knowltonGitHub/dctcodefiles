using DCT.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DCT
{
    public partial class Preferences : Form
    {

      

        public Preferences()
        {
            InitializeComponent();
        }

        private NVM _nvm = new NVM();
        public NVM NVMCont
        {
            get { return _nvm; }
            set { _nvm = value; }
        }
        private void buttonSavePreferences_Click(object sender, EventArgs e)
        {
            _nvm.SaveEmailPreferencesToDatabase(textBoxFromAddress.Text,
                textBoxHost.Text,
                textBoxPort.Text,
                textBoxUserName.Text,
                textBoxPassword.Text);

            _nvm.SaveLANPreferencesToDatabase(textBoxSourceIP.Text,
            textBoxSourcePort.Text,
            textBoxDestinationIP.Text,
            textBoxDestinationPort.Text);

            Close();
        }

        private void Preferences_Load(object sender, EventArgs e)
        {
            //get Email settings
            textBoxFromAddress.Text = _nvm.GetSingleRowValue("preferences", "FromAddress", "where UserID = " + _nvm.GetUserID(_nvm.UserName, _nvm.Password).ToString()).ToString();
            textBoxHost.Text = _nvm.GetSingleRowValue("preferences", "Host", "where UserID = " + _nvm.GetUserID(_nvm.UserName, _nvm.Password).ToString()).ToString();
            textBoxPort.Text = _nvm.GetSingleRowValue("preferences", "Port", "where UserID = " + _nvm.GetUserID(_nvm.UserName, _nvm.Password).ToString()).ToString();
            textBoxUserName.Text = _nvm.GetSingleRowValue("preferences", "UserName", "where UserID = " + _nvm.GetUserID(_nvm.UserName, _nvm.Password).ToString()).ToString();
            textBoxPassword.Text = _nvm.GetSingleRowValue("preferences", "Password", "where UserID = " + _nvm.GetUserID(_nvm.UserName, _nvm.Password).ToString()).ToString();
            //get LAN settings
            textBoxSourceIP.Text = _nvm.GetSingleRowValue("preferences", "SourceIP", "where UserID = " + _nvm.GetUserID(_nvm.UserName, _nvm.Password).ToString()).ToString();
            textBoxSourcePort.Text = _nvm.GetSingleRowValue("preferences", "SourcePort", "where UserID = " + _nvm.GetUserID(_nvm.UserName, _nvm.Password).ToString()).ToString();
            textBoxDestinationIP.Text = _nvm.GetSingleRowValue("preferences", "DestinationIP", "where UserID = " + _nvm.GetUserID(_nvm.UserName, _nvm.Password).ToString()).ToString();
            textBoxDestinationPort.Text = _nvm.GetSingleRowValue("preferences", "DestinationPort", "where UserID = " + _nvm.GetUserID(_nvm.UserName, _nvm.Password).ToString()).ToString();
        }

    }// end of class
}//end of namespace
