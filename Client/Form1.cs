using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        List<ClientTamplate> _clList = new List<ClientTamplate>();
        List<string> TextFiles = new List<string>();
        public Form1()
        {
            InitializeComponent();
            _clList = new List<ClientTamplate>();
            //for (int i =0; i <= 10; i++)
            //{
            //    _clList.Add(new ClientTamplate($"test {i}"));
                
            //}
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            TextFiles.Clear();
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            FBD.ShowNewFolderButton = false;

            if (FBD.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = FBD.SelectedPath;
            }
            else
            {
                return;
            }
            SearchFiles();
            LetsCheck();
            CheckResalts();
        }

        private void CheckResalts()
        {
            List<string> resalts = new List<string>();
            int dontProcesed = _clList.Count;
            while (dontProcesed != 0)
            {
                resalts.Clear();
                dontProcesed = 0;
                foreach(var clTmpl in _clList)
                {
                    if(clTmpl.Responce== String.Empty)
                    {
                        dontProcesed++;
                    }
                    else
                    {
                        resalts.Add(clTmpl.Path + " " + clTmpl.Responce);
                    }
                }
            }
            textBox2.Lines=resalts.ToArray();
        }

        private void SearchFiles()
        {
            string[] second = Directory.GetFiles(textBox1.Text);
            var res = second.Where(c => c.Contains(".txt"));
            TextFiles.AddRange(res);
        }

        private void LetsCheck()
        {
            foreach (var file in TextFiles)
            {
                StreamReader sr = new StreamReader(file);
                string str = "";
                while (!sr.EndOfStream)
                    str += sr.ReadLine();

                var clTmpl = new ClientTamplate(str, file);
                _clList.Add(clTmpl);
                Task.Run(clTmpl.WaitResalt);
            }
        }
    }
}
