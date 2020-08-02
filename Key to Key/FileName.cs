using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Key_to_Key
{
    public partial class FileName : Form
    {
        public string fileName = null, confDir = null, boxmes = null, titlemes = null;
        char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
        public FileName()
        {
            InitializeComponent();
        }
        private void FileName_Load(object sender, EventArgs e)
        {
            this.Text = titlemes;
            textBox1.Text = boxmes;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!textBox1.Text.EndsWith(".cfg")) textBox1.AppendText(".cfg");
            if (textBox1.Text.StartsWith(".")) return;
            if (textBox1.Text.IndexOfAny(invalidChars) >= 0)
            {
                MessageBox.Show("使用できない文字が使用されてます");
                return;
            }
            if (System.IO.File.Exists(confDir + textBox1.Text))
            {
                MessageBox.Show("'" + textBox1.Text + "'は存在します。");
                return;
            }
            else
            {
                fileName = textBox1.Text;
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            textBox1.SelectAll();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) button1.Focus();
        }
    }
}
