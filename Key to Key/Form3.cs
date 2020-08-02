using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Key_to_Key
{
    public partial class Form3 : Form
    {
        public byte sendkey, pushkey;
        public bool block, toggle, rabbit,cancelflag;
        public int sleep;

        public Form3()
        {
            InitializeComponent();
        }
        private void Form3_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = Enum.GetValues(typeof(RamGecTools.KeyboardHook.VKeys));
            comboBox2.DataSource = Enum.GetValues(typeof(RamGecTools.KeyboardHook.VKeys));
            comboBox1.SelectedIndex = comboBox1.FindStringExact(((RamGecTools.KeyboardHook.VKeys)pushkey).ToString());
            comboBox2.SelectedIndex = comboBox2.FindStringExact(((RamGecTools.KeyboardHook.VKeys)sendkey).ToString());
            checkBox1.Checked = block;
            if (rabbit)
            {
                checkBox2.Checked = rabbit;
                textBox1.Text = sleep.ToString();
                checkBox3.Checked = toggle;
            }
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;

            Enableds();
        }

        void Enableds()
        {
            textBox1.Enabled = checkBox2.Checked;
            checkBox3.Enabled = checkBox2.Checked;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            var tag = ((Button)sender).Tag;
            byte re;
            using (var f = new Form4())
            {
                f.ShowDialog(this);
                re = f.Result;               
                f.Dispose();
            }
            switch (tag)
            {
                case 1:
                    comboBox1.SelectedIndex = comboBox1.FindStringExact(((RamGecTools.KeyboardHook.VKeys)re).ToString());
                    pushkey = re;
                    break;
                case 2:
                    comboBox2.SelectedIndex = comboBox1.FindStringExact(((RamGecTools.KeyboardHook.VKeys)re).ToString());
                    sendkey = re;
                    break;
                default:break;
            }
           
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            block = checkBox1.Checked;
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            rabbit = checkBox2.Checked;
            Enableds();
        }
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            toggle = checkBox3.Checked;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b') e.Handled = true;
            if (e.KeyChar == 46) e.Handled = true;
            
            string str = Regex.Replace(textBox1.Text, @"[^0-9]", "");
            textBox1.Text = str;
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            string str = Regex.Replace(textBox1.Text, @"[^0-9]", "");
            sleep = Int32.Parse(str);
            if (sleep == 0) return;
            if (sleep <= 16) sleep = 16;
            textBox1.Text = sleep.ToString();
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
            cancelflag = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
