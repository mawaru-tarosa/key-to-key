using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Key_to_Key
{
    public partial class Form2 : Form
    {
        
        public object listbox;
        public string fileName;

        Dictionary<string, HashSet<string>> wh = new Dictionary<string, HashSet<string>>();
        

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.Text = "関連付け";
            
            listBox1.DataSource = listbox;
            Refresh_wh();
            listBox2.DataSource = wh[listBox1.Text].ToArray<string>();


        }
        private void Refresh_wh()
        {
            if (!IsFileRead(fileName)) return;
            using (var sr = new StreamReader(fileName))
            {
                while (!sr.EndOfStream)
                {

                    string line = sr.ReadLine();
                    string[] valuse = line.Split(',');
                    if (valuse.Length <= 1) continue;
                    if (valuse[0] == null) continue;
                    if (valuse[1] == null) continue;

                    if (wh.ContainsKey(valuse[1]))
                    {
                        if (!wh[valuse[1]].Contains(valuse[0]))
                        {
                            wh[valuse[1]].Add(valuse[0]);
                        }
                        continue;
                    }
                    wh.Add(valuse[1], new HashSet<string>() { valuse[0] });
                }
                sr.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox2.DataSource = null;
            if (listBox1.SelectedIndex == -1) return;
            if (!wh.ContainsKey(listBox1.Text)) return;
            if (wh[listBox1.Text].Count == 0) return;
            listBox2.DataSource = wh[listBox1.Text].ToArray<string>();
        }
        private bool IsFileRead(string filePath)
        {
            try
            {
                //ファイルを開く
                using (StreamReader readtext = new StreamReader(filePath))
                {
                    readtext.Close();
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("ファイルが見つかりません");
                return false;
            }
            catch (IOException)
            {
                MessageBox.Show("ファイルがロックされている可能性があります");
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("必要なアクセス許可がありません");
                return false;
            }
            catch (Exception)
            {
                //すべての例外をキャッチする
                //例外の説明を表示する
                MessageBox.Show("なんかエラーだって");
                return false;
            }
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null) return;
            if (textBox1.Text == "") return;
            if (!wh.ContainsKey(listBox1.Text))
            {
                wh.Add(listBox1.Text, new HashSet<string>() { textBox1.Text });
            }
            else
            {
                wh[listBox1.Text].Add(textBox1.Text);
            }
            SaveFile();
        }
        void SaveFile()
        {
            if (!IsFileRead(fileName)) return; ;
            using (var sw = new StreamWriter(fileName))
            {
                
                foreach(var (id,key) in wh)
                {
                    foreach(var item in key)
                    {
                        sw.WriteLine(item+","+id);
                    }
                }

                sw.Close();
            }
            listBox2.DataSource = wh[listBox1.Text].ToArray<string>();
            textBox1.Text = null;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            wh[listBox1.Text].Remove(listBox2.Text);
            SaveFile();
            listBox2.DataSource = wh[listBox1.Text].ToArray<string>();
        }
    }
}
