using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HUST_1_Demo.View
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void BLogin_Click(object sender, EventArgs e)
        {
            string usrName = UsrName.Text;
            string psrWrd = PassWord.Text;
            if ( usrName == "admin" && psrWrd == "liuhui" )
            {
                //this.Hide();
                
                Form1 form = new Form1();
                form.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("密码错误，请重新输入！\r\n"); ;
            }
        }

        private void BQuit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
