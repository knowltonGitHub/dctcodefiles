using DCT.Classes;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DCT
{
    public partial class GetGMailToAddress : Form, ISupportsEvents
    {
        private CustomEvents _ce = new CustomEvents();

        public CustomEvents ce { get { return _ce; } set { _ce = value; } }

        public GetGMailToAddress()
        {
            InitializeComponent();
        }

        private void buttonCancelSend_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonSendToAddress_Click(object sender, EventArgs e)
        {
            //NVM.ToAddress = textBoxToAddress.Text;
            //string where = "where UserID = " + NVM.GetUserID(NVM.UserName, NVM.Password).ToString();
            //NVM.Email("This value was sent to you by the DCT:" + System.Environment.NewLine + System.Environment.NewLine + NVM.ItemValue, NVM.GetSingleRowValue("preferences", "fromAddress", where).ToString(),
            //    textBoxToAddress.Text, "DCT sent you the value for:  " + NVM.ItemName, NVM.GetSingleRowValue("preferences", "Host", where).ToString(),
            //    Convert.ToInt32(NVM.GetSingleRowValue("preferences", "Port", where)), NVM.GetSingleRowValue("preferences", "NVM.UserName", where).ToString(), NVM.GetSingleRowValue("preferences", "NVM.Password", where).ToString());

            //Close();
        }

        private void textBoxToAddress_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == (char)Keys.Enter)
            {
                //save to database
                //NVM.SaveEmailToDatabase(NVM.GetUserID(NVM.UserName, NVM.Password).ToString(), textBoxToAddress.Text);
                //listBoxEmailAddresses.Items.Clear();
                //List<object> eadds = NVM.GetEmailAddresses();

                //foreach(object o in eadds)
                //{
                //    listBoxEmailAddresses.Items.Add(o);
                //}
            }
        }

        private void GetGMailToAddress_Load(object sender, EventArgs e)
        {
            //List<object> eadds = NVM.GetEmailAddresses();

            //foreach (object o in eadds)
            //{
            //    listBoxEmailAddresses.Items.Add(o);
            //}
        }
    }
}
