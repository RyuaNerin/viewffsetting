using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace viewffsetting
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            this.button1_Click(null, null);
        }

        private string GetPath()
        {
            var dirPath =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "My Games",
                    "FINAL FANTASY XIV - KOREA");

            return dirPath;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.listView1.Items.Clear();
            foreach (var dir in Directory.GetDirectories(GetPath(), "FFXIV_CHR*"))
                this.listBox1.Items.Add(Path.GetFileName(dir));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedItems.Count != 1)
                return;

            Process.Start("explorer", string.Format("/select,\"{0}\"", Path.Combine(GetPath(), (string)this.listBox1.SelectedItem))).Dispose();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            this.listView1.Items.Clear();
            this.listView2.Items.Clear();

            var path = Path.Combine(GetPath(), (string)this.listBox1.SelectedItem);

            using (var gear  = File.OpenRead(Path.Combine(path, "GEARSET.DAT")))
                ReadGear(gear);

            using (var macro = File.OpenRead(Path.Combine(path, "MACRO.DAT")))
                ReadMacro(macro);
        }

        private void ReadGear(Stream stream)
        {
            var reader = new BinaryReader(stream);

            stream.Position = 8;
            var dataSize = reader.ReadInt32() + 0x10;
            stream.Position = 0x15;
            
            var    setNumber = 0;
            var    rawName   = new byte[46];
            string name;
            int[]  ids;
            int    i;
            var    buff      = new byte[128];
            

            var lst = new List<Tuple<int, string, int[]>>();

            while (stream.Position < dataSize)
            {
                setNumber = reader.ReadByte() ^ 0x73;

                stream.Read(rawName, 0, 46);
                Xor(rawName, 0x73);
                name = GetName(rawName, 46);
                
                stream.Position += 5;
                
                ids = new int[14];
                for (i = 0; i < 14; ++i)
                {
                    stream.Read(buff, 0, 4);
                    Xor(buff, 4, 0x73);
                    ids[i] = BitConverter.ToInt32(buff, 0);

                    stream.Position += 24;
                }

                if (name != null)
                    lst.Add(new Tuple<int,string,int[]>(setNumber, name, ids));
            }
            
            lst.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            lst.ForEach(e =>
                {
                    var item = this.listView1.Items.Add(e.Item1.ToString());
                    item.SubItems.Add(e.Item2);
                    item.Tag = e.Item3;
                });
        }

        private void ReadMacro(Stream stream)
        {
            var reader = new BinaryReader(stream);

            stream.Position = 8;
            var dataSize = reader.ReadInt32() + 0x10;
            stream.Position = 0x11;

            int i;
            int num = 0;
            byte[] macroName;
            byte[] macroLine;
            byte[] key;
            string[] macroLines;

            var lst = new List<Tuple<int, string, string[]>>();

            while (stream.Position < dataSize)
            {
                macroName = ReadMacroSector(reader, true);
                ReadMacroSector(reader, false);
                key  = ReadMacroSector(reader, true);

                macroLines = new string[15];
                for (i = 0; i < 15; ++i)
                {
                    macroLine = ReadMacroSector(reader, true);
                    macroLines[i] = GetName(macroLine, macroLine.Length);
                }

                if (key[0] != 0x30 || key[1] != 0x30 || key[2] != 0x30 || key[3] != 0)
                    lst.Add(new Tuple<int, string, string[]>(num, GetName(macroName, macroName.Length), macroLines));

                num++;
            }

            lst.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            lst.ForEach(e =>
                {
                    var item = this.listView2.Items.Add(e.Item1.ToString());
                    item.SubItems.Add(e.Item2);
                    item.Tag = String.Join("\r\n", e.Item3);
                });
        }

        private byte[] ReadMacroSector(BinaryReader reader, bool @return)
        {
            // type
            reader.BaseStream.Position++;

            // length
            var rawLen = reader.ReadBytes(2);
            Xor(rawLen, 0x73);
            int len = BitConverter.ToInt16(rawLen, 0);

            // data
            if (!@return)
            {
                reader.BaseStream.Position += len;
                return null;
            }
            else
            {
                var buff = reader.ReadBytes(len);
                Xor(buff, 0x73);

                return buff;
            }
        }

        private void Xor(byte[] array, int key)
        {
            Xor(array, array.Length, key);
        }
        private void Xor(byte[] array, int count, int key)
        {
            for (int i = 0; i < count; ++i)
                array[i] = (byte)(array[i] ^ key);
        }

        private string GetName(byte[] array, int count)
        {
            int len = 0;
            
            while (array[len] != 0 && len < count - 1)
                ++len;

            if (len == 0)
                return null;
            else
                return Encoding.UTF8.GetString(array, 0, len);
        }

        private void listView2_DoubleClick(object sender, EventArgs e)
        {
            if (this.listView2.SelectedItems.Count != 1)
                return;

            using (var frm = new Form2())
            {
                frm.Text = this.listView2.SelectedItems[0].SubItems[1].Text;
                frm.textBox1.Text = (string)this.listView2.SelectedItems[0].Tag;
                frm.ShowDialog(this);
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count != 1)
                return;

            using (var frm = new Form3((int[])this.listView1.SelectedItems[0].Tag))
            {
                frm.Text = this.listView1.SelectedItems[0].SubItems[1].Text;
                frm.ShowDialog(this);
            }
        }
    }
}
