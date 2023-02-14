using System;
using System.Windows.Forms;

namespace DCT
{
    public partial class AboutForm : Form, ISupportsEvents
    {
        private CustomEvents _ce = new CustomEvents();

        public CustomEvents ce { get { return _ce; } set { _ce = value; } }

        public AboutForm()
        {
            InitializeComponent();
        }

        private void buttonCloseAbout_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
