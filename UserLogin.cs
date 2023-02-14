using DCT.Classes;
using System;
using System.Windows.Forms;

namespace DCT
{
    public partial class UserLogin : Form, ISupportsEvents
    {

        private NVM _nvm = new NVM();
        public NVM NVMCont
        {
            get { return _nvm; }
            set { _nvm = value; }
        }

        private CustomEvents _ce = new CustomEvents();

        public CustomEvents ce { get { return _ce; } set { _ce = value; } }

        public UserLogin()
        {
            InitializeComponent();
            ExecuteLogin();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            ExecuteLogin();
        }

        private void ExecuteLogin()
        {
            _nvm.UserName = textBoxUserName.Text;
            _nvm.Password = textBoxPassword.Text;
            _nvm.UserID = _nvm.GetUserID(textBoxUserName.Text, textBoxPassword.Text);
            Close();
        }
    }
}