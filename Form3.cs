using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace viewffsetting
{
    public partial class Form3 : Form
    {
        static IDictionary<int, string> dic = new SortedDictionary<int, string>();

        static Form3()
        {
            using (var reader = new StringReader(Properties.Resources.ItemList))
            {
                string line;
                string[] spl;
                while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
                {
                    spl = line.Split('\t');
                    if (spl.Length != 2)
                        continue;

                    dic.Add(int.Parse(spl[0]), spl[1]);
                }
            }
        }

        public Form3(int[] ids)
        {
            InitializeComponent();

            this.listView1.Items.Add("주 무기").SubItems.Add("");
            this.listView1.Items.Add("보조 무기").SubItems.Add("");

            this.listView1.Items.Add("머리").SubItems.Add("");
            this.listView1.Items.Add("몸통").SubItems.Add("");
            this.listView1.Items.Add("손").SubItems.Add("");
            this.listView1.Items.Add("허리").SubItems.Add("");
            this.listView1.Items.Add("다리").SubItems.Add("");
            this.listView1.Items.Add("발").SubItems.Add("");

            this.listView1.Items.Add("귀걸이").SubItems.Add("");
            this.listView1.Items.Add("목걸이").SubItems.Add("");
            this.listView1.Items.Add("팔찌").SubItems.Add("");
            this.listView1.Items.Add("반지").SubItems.Add("");
            this.listView1.Items.Add("반지").SubItems.Add("");

            this.listView1.Items.Add("소크").SubItems.Add("");

            int id;
            for (int i = 0; i < 14; ++i)
            {
                this.listView1.Items[i].SubItems[1].Tag = ids[i];

                id = ids[i] > 1000000 ? ids[i] - 1000000 : ids[i];

                if (id == 0)
                    continue;

                else if (!dic.ContainsKey(id))
                    this.listView1.Items[i].SubItems[1].Text = "알 수 없는 장비";

                else if (ids[i] > 1000000)
                    this.listView1.Items[i].SubItems[1].Text = dic[id] + " (HQ)";
                    
                else
                    this.listView1.Items[i].SubItems[1].Text = dic[id];
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count != 1)
                return;

            var tag = (int)this.listView1.SelectedItems[0].SubItems[1].Tag;

            if (tag == 0)
                return;

            if (tag  > 1000000)
                tag -= 1000000;

            Process.Start("explorer", string.Format("\"http://ff14.inven.co.kr/dataninfo/item/detail.php?code={0}\"", tag)).Dispose();
        }

        private void Form3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }
    }
}
