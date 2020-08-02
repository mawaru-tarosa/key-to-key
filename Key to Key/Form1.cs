using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using RamGecTools;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using System.Diagnostics;
using System.Collections.Immutable;

namespace Key_to_Key
{

    public partial class Form1 : Form
    {
        WindowSwitchingHook wsh = new WindowSwitchingHook();
        KeyboardHook keyboardHook = new KeyboardHook();
        string confFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Key_To_Key\conf\";
        string settingFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Key_To_Key\setting\";
        string associtionFile = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Key_To_Key\setting\Assocition.csv";
        HashSet<string> confFile = new HashSet<string>();

        Dictionary<string, string> windowAssocition = new Dictionary<string, string>();
        Dictionary<byte, List<WrapKeybd_event>> keyeve = new Dictionary<byte, List<WrapKeybd_event>>(); 
        

        public Form1()
        {
            InitializeComponent();
            ListBoxDraw();
            SelectConfig();
            checkBox2.Checked = true;
        }

        private void ListBoxDraw()
        {
            if (!Directory.Exists(confFolder))
            {
                Directory.CreateDirectory(confFolder);
            }
            if (Directory.GetFiles(confFolder, "Default.cfg").Length == 0)
            {
                using (StreamWriter writer = new StreamWriter(confFolder + @"Default.cfg"))
                {
                    writer.Close();
                }
            }
            if (!Directory.Exists(settingFolder))
            {
                Directory.CreateDirectory(settingFolder);
            }
            if(!File.Exists(associtionFile))
            {
                using (var sw = new StreamWriter(associtionFile))
                {
                    sw.Close();
                }
            }

            var di = new DirectoryInfo(confFolder);
            FileInfo[] fn = di.GetFiles("*.cfg");

            string[] str = new string[fn.Length];
            for (int i = 0; i < fn.Length; i++)
            {
                str[i] = Path.GetFileNameWithoutExtension(fn[i].Name);
            }

            listBox1.DataSource = str;
            listBox1.DisplayMember = "Name";
            confFile.Clear();
            //confFile = new string[listBox1.Items.Count];
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                confFile.Add(listBox1.Items[i].ToString());
            }
            SetWindowAssocition();
        }
        /// <summary>
        /// リストボックスを選択します
        /// </summary>
        /// <param name="str"></param>
        void SelectConfig(string str = null)
        {
            if (str == null)
            {
                listBox1.SelectedIndex = listBox1.FindStringExact(@"Default");
                checkBox1.Checked = false;
            }
            else
            {
                listBox1.SelectedIndex = listBox1.FindStringExact(str);
                checkBox1.Checked = true;
            }
        }
        void  SetWindowAssocition()
        {
            using(var sr = new StreamReader(associtionFile))
            {
                while (!sr.EndOfStream)
                {

                    string line = sr.ReadLine();
                    string[] valuse = line.Split(',');
                    if (valuse.Length <= 1) continue;

                    if (windowAssocition.ContainsKey(valuse[0]))
                    {
                        windowAssocition[valuse[0]] = valuse[1];
                        continue;
                    }

                    windowAssocition.Add(valuse[0], valuse[1]);

                }
                sr.Close();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            WrapKeybd_event.StopAllLoop();

            if (confFile == null) return;
            int i = listBox1.SelectedIndex;
            if (i == -1) return;
            string str = confFolder + listBox1.SelectedItem.ToString() + @".cfg";
            if (!IsFileRead(str)) return;
            

            KeyboardHook.keyDisableList.Clear();
            keyeve.Clear();

            using (StreamReader sr = new StreamReader(str))
            {

                for (int l = 0; !sr.EndOfStream; l++)
                {
                    string line = sr.ReadLine();
                    string[] values = line.Split(',');
                    if (values.Length <= 4) continue;
                    byte inputKey = byte.Parse(values[0]);
                    if(Convert.ToBoolean(values[1]))KeyboardHook.keyDisableList.Add(inputKey);
                    byte sendKey = byte.Parse(values[2]);
                    int delay = Int32.Parse(values[3]);
                    bool toggle = Convert.ToBoolean(values[4]);

                    if (!keyeve.ContainsKey(inputKey))
                    {
                        keyeve.Add(inputKey, new List<WrapKeybd_event>() { new WrapKeybd_event(sendKey, false, 0x1) });
                        keyeve[inputKey][0].SleepTime = delay;
                        keyeve[inputKey][0].Toggle = toggle;
                    }
                    else
                    {
                        keyeve[inputKey].Add(new WrapKeybd_event(sendKey, false, 0x1));
                        i = keyeve[inputKey].Count - 1;
                        keyeve[inputKey][i].SleepTime = delay;
                        keyeve[inputKey][i].Toggle = toggle;
                    }
                    
                }
                sr.Close();
            }

            ListViewDorw();
        }

        void ListViewDorw()
        {
            listView1.BeginUpdate();
            listView1.Clear();
            listView1.View = View.Details;
            listView1.LabelEdit = false;
            listView1.AllowColumnReorder = false;
            listView1.MultiSelect = false;
            //listView1.CheckBoxes = true;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            //listView1.Sorting = SortOrder.Ascending;
            //listView1.Columns.Add("chkecbox");
            listView1.Columns.Add("Input key");           
            listView1.Columns.Add("Send key");
            listView1.Columns.Add("Push key");
            listView1.Columns.Add("Sleep time");
            listView1.Columns.Add("Toggle");
            
            ListViewItem lvi;
            foreach (var (id, key) in keyeve)
            {
                foreach (var l in key)
                {
                    lvi = listView1.Items.Add(((KeyboardHook.VKeys)id).ToString());

                    lvi.SubItems.Add(((KeyboardHook.VKeys)l.SendKey).ToString());
                    lvi.SubItems.Add(KeyboardHook.keyDisableList.Contains(id) ? "Stop" : "Through");
                    lvi.SubItems.Add(l.SleepTime.ToString());
                    lvi.SubItems.Add(l.Toggle ? "Enable" : "Disable");
                }
            }
            foreach (ColumnHeader ch in listView1.Columns) 
            {
                ch.Width = 70; 
            }
            listView1.EndUpdate();
           
        }

        /// <summary>
        /// ファイルが開ける状態か確認します
        /// 開ける状態ならtrueを返し
        /// エラーが出るならばfalseを返します
        /// </summary>
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            WrapKeybd_event.StopAllLoop();

            if (checkBox1.Checked)
            {
                // register evens
                keyboardHook.KeyDown += new KeyboardHook.KeyboardHookCallback(KeyboardHook_KeyDown);
                keyboardHook.KeyUp += new KeyboardHook.KeyboardHookCallback(KeyboardHook_KeyUp);

                keyboardHook.Install();
            }
            else
            {
                keyboardHook.KeyDown -= KeyboardHook_KeyDown;
                keyboardHook.KeyUp -= KeyboardHook_KeyUp;

                keyboardHook.Uninstall();
            }
        }

        private void KeyboardHook_KeyUp(byte key, UIntPtr uIntPtr)
        {
            if (uIntPtr == (UIntPtr)0x1)
            {
                return;
            }
            if (!keyeve.ContainsKey(key)) return;
            foreach (var item in keyeve[key])
            {
                item.KeyUp();
                item.StopLoop();
            }
        }

        private void KeyboardHook_KeyDown(byte key, UIntPtr uIntPtr)
        {
            if (uIntPtr == (UIntPtr)0x1)
            {
                return;
            }
            if (!keyeve.ContainsKey(key)) return;
            foreach (var item in keyeve[key])
            {
                item.KeyDown();
                item.StartLoop();
            }

        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            WrapKeybd_event.StopAllLoop();
            if (checkBox2.Checked)
            {
                wsh.Window += new WindowSwitchingHook.Callback(WindowHook);
                wsh.Install();
            }
            else
            {
                wsh.Window -= WindowHook;
                wsh.Uninstall();
            }
        }

        private void WindowHook(WindowSwitchEventArgs e)
        {
            string str = e.GetProcessName();
            label1.Text = str;
            if (windowAssocition == null) return;

            if(windowAssocition.ContainsKey(str))
            {
                SelectConfig(windowAssocition[str]);
                return;
            }
            str = GetPathWithoutExtension(str);
            if (windowAssocition.ContainsKey(str))
            {
                SelectConfig(windowAssocition[str]);
                return;
            }
            SelectConfig();
        }
        /// <summary>
        /// 指定されたパス文字列から拡張子を削除して返します
        /// </summary>
        public static string GetPathWithoutExtension(string path)
        {
            var extension = Path.GetExtension(path);
            if (string.IsNullOrEmpty(extension))
            {
                return path;
            }
            return path.Replace(extension, string.Empty);
        }

        void ResultForm3(Form3 f)
        {
            f.ShowDialog(this);
            if (f.cancelflag) return;
            if (f.pushkey == 0x7) return;

            if (!f.rabbit) f.sleep = 0;
            if (keyeve.ContainsKey(f.pushkey))
            {
                foreach (var l in keyeve[f.pushkey])
                {
                    if (l.SendKey == f.sendkey)
                    {
                        goto terukeccha;
                    }
                }
                keyeve[f.pushkey].Add(new WrapKeybd_event(f.sendkey, false, 1));
            }
            else
            {
                keyeve.Add(f.pushkey, new List<WrapKeybd_event>() { new WrapKeybd_event(f.sendkey, false, 1) });
            }
        terukeccha:

            foreach (var l in keyeve[f.pushkey])
            {
                if (l.SendKey == f.sendkey)
                {
                    l.SleepTime = f.sleep;
                    l.Toggle = f.toggle;
                }
            }

            if (f.block)
            {
                KeyboardHook.keyDisableList.Add(f.pushkey);
            }
            else
            {
                KeyboardHook.keyDisableList.Remove(f.pushkey);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            //新規作成
            using(var f = new FileName())
            {
                bool winflag = checkBox2.Checked;
                checkBox2.Checked = false;
                f.confDir = confFolder;
                f.titlemes = "新規作成";
                f.StartPosition = FormStartPosition.CenterParent;
                f.ShowDialog(this);
                string file_name = f.fileName;                
                if (file_name == null) return;
                using (var wt = new StreamWriter(confFolder + file_name)) wt.Close();
                ListBoxDraw();
                SelectConfig(file_name);
                checkBox2.Checked = winflag;
                f.Dispose();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //名前変更
            using (var f = new FileName())
            {
                bool winflag = checkBox2.Checked;
                checkBox2.Checked = false;
                f.confDir = confFolder;
                f.boxmes = listBox1.Text;
                f.titlemes = "名前の変更";
                f.StartPosition = FormStartPosition.CenterParent;
                f.ShowDialog(this);
                string file_name = f.fileName;
                string str = confFolder + listBox1.Text + ".cfg";
                if (file_name == null) return;
                if (!IsFileRead(str)) return;
                File.Move(str, confFolder + file_name);
                ListBoxDraw();
                SelectConfig(file_name);
                checkBox2.Checked = winflag;
                f.Dispose();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //コピー
            using (var f = new FileName())
            {
                bool winflag = checkBox2.Checked;
                checkBox2.Checked = false;
                f.confDir = confFolder;
                f.boxmes = listBox1.Text;
                f.titlemes = "ファイルのコピー";
                f.StartPosition = FormStartPosition.CenterParent;
                f.ShowDialog(this);
                string file_name = f.fileName;
                string str = confFolder + listBox1.Text + ".cfg";
                if (file_name == null) return;
                if (!IsFileRead(str)) return;
                File.Copy(str, confFolder + file_name);
                ListBoxDraw();
                SelectConfig(file_name);
                checkBox2.Checked = winflag;
                f.Dispose();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            bool winflag = checkBox2.Checked;
            checkBox2.Checked = false;
            //削除
            DialogResult result = MessageBox.Show(this, "ファイルを削除しますか", "WARNING", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                string str = confFolder + listBox1.Text + ".cfg";
                if (!IsFileRead(str)) return;
                File.Delete(str);
                ListBoxDraw();
                SelectConfig();
            }
            checkBox2.Checked = winflag;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //フォルダ
            //Process.Start(confFolder);
            System.Diagnostics.Process.Start("EXPLORER.EXE", Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Key_To_Key\" );
        }

        private void button6_Click(object sender, EventArgs e)
        {
            bool windowhook = checkBox2.Checked;
            checkBox2.Checked = false;
            //追加
            using (var f = new Form3())
            {
                f.pushkey = 0x7;
                f.sendkey = 0x7;
                ResultForm3(f);
                if (f.sendkey == 0x7) return;
                f.Dispose();
            }
            checkBox2.Checked = windowhook;
            SaveFile();
            ListViewDorw();

        }

        private void button7_Click(object sender, EventArgs e)
        {
            //登録削除
            if (listView1.SelectedItems.Count == 0) return;
            keyeve.Clear();
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.SelectedItems[0].Index == i) continue;
                var item = listView1.Items[i];
                var inputKey = ((byte)(KeyboardHook.VKeys)Enum.Parse(typeof(KeyboardHook.VKeys), item.SubItems[0].Text));
                
                var sendKey = ((byte)(KeyboardHook.VKeys)Enum.Parse(typeof(KeyboardHook.VKeys), item.SubItems[1].Text));
                var delay = ((byte)(KeyboardHook.VKeys)Enum.Parse(typeof(KeyboardHook.VKeys), item.SubItems[3].Text));
                bool toggle = false;
                if (((item.SubItems[4].Text)) == "Enabel") toggle = true;


                if (!keyeve.ContainsKey(((byte)(KeyboardHook.VKeys)Enum.Parse(typeof(KeyboardHook.VKeys), item.SubItems[0].Text))))
                {
                    keyeve.Add(inputKey, new List<WrapKeybd_event>() { new WrapKeybd_event(sendKey, false, 0x1) });
                    keyeve[inputKey][0].SleepTime = delay;
                    keyeve[inputKey][0].Toggle = toggle;
                }
                else
                {
                    keyeve[inputKey].Add(new WrapKeybd_event(sendKey, false, 0x1));
                    i = keyeve[inputKey].Count - 1;
                    keyeve[inputKey][i].SleepTime = delay;
                    keyeve[inputKey][i].Toggle = toggle;
                }
            }
            SaveFile();
            ListViewDorw();

        }
        void SaveFile()
        {
            using (var sw = new StreamWriter(confFolder+listBox1.Text+@".cfg"))
            {
                foreach(var (key,val)in keyeve)
                {
                    foreach(var l in val)
                    {
                        sw.WriteLine(key + "," + ((KeyboardHook.keyDisableList.Contains(key)).ToString()) + "," + l.SendKey + "," + l.SleepTime + "," + ((l.Toggle).ToString()));
                    }
                }

            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //登録クリア
            keyeve.Clear();
            SaveFile();
            ListViewDorw();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //終了
            this.Close();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //関連付け
            using (var f = new Form2())
            {
                bool winflag = checkBox2.Checked;
                checkBox2.Checked = false;

                f.fileName = associtionFile ;
                f.listbox = listBox1.DataSource;
                f.StartPosition = FormStartPosition.CenterParent;
                f.ShowDialog(this);

                //if (!IsFileRead(str)) return;

                ListBoxDraw();
                checkBox2.Checked = winflag;
                f.Dispose();
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            bool windowhook = checkBox2.Checked;
            checkBox2.Checked = false;
            var item = listView1.SelectedItems[0];
            //追加
            using (var f = new Form3())
            {
                var enumval = (KeyboardHook.VKeys)Enum.Parse(typeof(KeyboardHook.VKeys), item.SubItems[0].Text);
                f.pushkey = ((byte)(KeyboardHook.VKeys)Enum.Parse(typeof(KeyboardHook.VKeys), item.SubItems[0].Text));
                f.sendkey= ((byte)(KeyboardHook.VKeys)Enum.Parse(typeof(KeyboardHook.VKeys), item.SubItems[1].Text));
                f.block = KeyboardHook.keyDisableList.Contains(f.pushkey);
                f.sleep = Int32.Parse((item.SubItems[3].Text));
                if (item.SubItems[4].Text == "Enable") f.toggle = true;
                if (f.sleep > 0) f.rabbit = true;
               

                ResultForm3(f);
                if (f.sendkey == 0x7) return;
                f.Dispose();
            }
            checkBox2.Checked = windowhook;
            SaveFile();
            ListViewDorw();
        }


    }
}
