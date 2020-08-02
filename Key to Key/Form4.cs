using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Key_to_Key
{
    public partial class Form4 : Form
    {
        public byte Result { get; set; }

        public Form4()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            Result = 0;
            this.Close();
        }

        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            Result = (byte)e.KeyCode;
            this.Close();
        }
    }
}
