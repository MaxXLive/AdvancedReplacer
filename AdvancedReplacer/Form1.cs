using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvancedReplacer
{
    public partial class Form1 : Form
    {
        private int selectedMode = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open Text File";
            theDialog.Filter = "Text files|*.txt|All files|*.*";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = theDialog.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            StreamReader reader = new StreamReader(myStream);
                            InputTextbox.Text = reader.ReadToEnd();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            updateElements();
        }

        private void updateElements()
        {
            int width = this.Size.Width;
            int height = this.Size.Height;

            // window
            InputTextbox.Size = new Size((width / 2) - 20, height - 250);
            OutputTextbox.Size = new Size((width / 2) - 30, height - 250);
            OutputTextbox.Location = new Point(width / 2, OutputTextbox.Location.Y);
            label2.Location = new Point(width / 2, label2.Location.Y);
            tabControl1.Location = new Point(tabControl1.Location.X, height - 200);

            //tab1
            textBox1.Size = new Size((width / 2) - 30, textBox1.Size.Height);
            textBox2.Location = new Point((width / 2) - 10, textBox2.Location.Y);
            textBox2.Size = new Size((width / 2) - 50, textBox2.Size.Height);
            label4.Location = new Point((width / 2) - 10, label4.Location.Y);

            //tab2
            textBox4.Size = new Size((width / 2) - 30, textBox4.Size.Height);
            textBox3.Location = new Point((width / 2) - 10, textBox3.Location.Y);
            textBox3.Size = new Size((width / 2) - 50, textBox3.Size.Height);
            label5.Location = new Point((width / 2) - 10, label5.Location.Y);

        }

        private void copyOutputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OutputTextbox.Text.Length > 0)
            {
                System.Windows.Forms.Clipboard.SetText(OutputTextbox.Text);
            }
        }

        private void reuseOutputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OutputTextbox.Text.Length > 0)
            {
                string temp = OutputTextbox.Text;
                OutputTextbox.Text = "";
                InputTextbox.Text = temp;
            }
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InputTextbox.Text = "";
            OutputTextbox.Text = "";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void updateOutput()
        {
            switch (selectedMode)
            {
                case 0:
                    {
                        // Find and replace
                        String inTxt = InputTextbox.Text;
                        String replaceTxt = textBox1.Text;
                        String withTxt = textBox2.Text.Replace("\\n", Environment.NewLine);

                        if (textBox1.Text.Length == 0)
                        {
                            replaceTxt = " ";
                            withTxt = " ";
                        }
                        else
                        {
                            // Count
                            if (checkBox1.Checked) {
                                label7.Text = "Occurrences: " + Regex.Matches(inTxt, Regex.Escape(replaceTxt), RegexOptions.IgnoreCase).Count.ToString();
                            }
                            else
                            {
                                label7.Text = "Occurrences: " + Regex.Matches(inTxt, Regex.Escape(replaceTxt), RegexOptions.Compiled).Count.ToString();
                            }
                                
                        }

                        if (checkBox1.Checked)
                        {
                            String outStr = Regex.Replace(inTxt, Regex.Escape(replaceTxt), withTxt, RegexOptions.IgnoreCase);
                            OutputTextbox.Text = outStr;
                        }
                        else
                        {
                            OutputTextbox.Text = inTxt.Replace(replaceTxt, withTxt);
                        }

                        break;
                    }
                case 1:
                    {
                        //in between
                        if (checkBox2.Checked)
                        {
                            string output = "";

                            using (StringReader reader = new StringReader(InputTextbox.Text))
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    output += GetStringInBetween(textBox4.Text, textBox3.Text, line, false, false) + Environment.NewLine;
                                }
                            }

                            if (output.Length > 0)
                            {
                                OutputTextbox.Text = output;
                            }
                            else
                            {
                                OutputTextbox.Text = "No match";
                            }
                        }
                        else
                        {
                            string output = GetStringInBetween(textBox4.Text, textBox3.Text, InputTextbox.Text, false, false);

                            if (output.Length > 0)
                            {
                                OutputTextbox.Text = output;
                            }
                            else
                            {
                                OutputTextbox.Text = "No match";
                            }
                        }

                        
                        break;
                    }
                case 2:
                    {
                        // line tools
                        using (StringReader reader = new StringReader(InputTextbox.Text))
                        {
                            string output = "";
                            string line;
                            int orgnr = 0;
                            while ((line = reader.ReadLine()) != null)
                            {
                                RegexOptions regexOptions = checkBox4.Checked ? RegexOptions.IgnoreCase : RegexOptions.None;

                                if(Regex.IsMatch(line, Regex.Escape(textBox6.Text), regexOptions) == !checkBox3.Checked)
                                {
                                    orgnr++;
                                    string temp = textBox5.Text;

                                    if (textBox5.Text.Contains("$l"))
                                    {
                                        temp = replace(temp, "$l", line);
                                    }

                                    if (textBox5.Text.Contains("$n"))
                                    {
                                        temp = replace(temp, "$n", orgnr.ToString());
                                    }

                                    if (textBox5.Text.Contains("$f"))
                                    {
                                        if (line.Length > 0)
                                        {
                                            temp = replace(temp, "$f", line[0].ToString().ToUpper() + line.Substring(1));
                                        }

                                    }

                                    if (textBox5.Text.Contains("$g"))
                                    {
                                        if (line.Length > 0)
                                        {
                                            temp = replace(temp, "$g", line[0].ToString().ToLower() + line.Substring(1));
                                        }

                                    }

                                    if (textBox5.Text.Contains("$u"))
                                    {
                                        temp = replace(temp, "$u", line.ToUpper());
                                    }

                                    if (textBox5.Text.Contains("$d"))
                                    {
                                        temp = replace(temp, "$d", line.ToLower());
                                    }


                                    output += temp + Environment.NewLine;
                                }
                            }

                            if (output.Length > 0)
                            {
                                OutputTextbox.Text = output;
                            }
                            else
                            {
                                OutputTextbox.Text = "No match";
                            }
                        }
                        break;
                    }
                case 3:
                    {
                        using (StringReader reader = new StringReader(InputTextbox.Text))
                        {
                            List<String> lines = new List<string>();

                            string output = "";
                            string line;
                            int orgnr = 0;
                            while ((line = reader.ReadLine()) != null)
                            {
                                lines.Add(line);
                            }

                            if (radioButton1.Checked)
                            {
                                // sort
                                lines = lines.OrderBy(c => c).ToList();
                            }
                            else if(radioButton2.Checked)
                            {
                                // sort desc
                                lines = lines.OrderByDescending(c => c).ToList();
                            }
                            else if (radioButton3.Checked)
                            {
                                // randomize
                                int n = lines.Count;
                                while (n > 1)
                                {
                                    n--;
                                    int k = RandomIntFromRNG(0, n+1);
                                    String value = lines[k];
                                    lines[k] = lines[n];
                                    lines[n] = value;
                                }
                            }
                            else
                            {
                                //remove duplicates
                                lines = lines.Distinct().ToList();
                            }
                            

                            foreach(String myline in lines)
                            {
                                output += myline + Environment.NewLine;
                            }

                            OutputTextbox.Text = output;
                        }
                        break;
                    }
                default: break;
            }
            
        }

        private int RandomIntFromRNG(int min, int max)
        {
            RNGCryptoServiceProvider CprytoRNG = new RNGCryptoServiceProvider();

            // Generate four random bytes
            byte[] four_bytes = new byte[4];
            CprytoRNG.GetBytes(four_bytes);

            // Convert the bytes to a UInt32
            UInt32 scale = BitConverter.ToUInt32(four_bytes, 0);

            // And use that to pick a random number >= min and < max
            return (int)(min + (max - min) * (scale / (uint.MaxValue + 1.0)));
        }

        public string replace(string source, string replace, string with)
        {
            if (replace != "")
            {
                string output = source.Replace(replace, with);
                return output;

            }
            else
            {
                return source;
            }


        }

        public static string GetStringInBetween(string strBegin, string strEnd, string strSource, bool includeBegin, bool includeEnd)
        {
            string[] result = { string.Empty, string.Empty };
            int iIndexOfBegin = strSource.IndexOf(strBegin);

            if (iIndexOfBegin != -1)
            {
                if (includeBegin)
                    iIndexOfBegin -= strBegin.Length;

                strSource = strSource.Substring(iIndexOfBegin + strBegin.Length);

                int iEnd = strSource.IndexOf(strEnd);
                if (iEnd != -1)
                {
                    if (includeEnd)
                        iEnd += strEnd.Length;
                    result[0] = strSource.Substring(0, iEnd);
                    if (iEnd + strEnd.Length < strSource.Length)
                        result[1] = strSource.Substring(iEnd + strEnd.Length);
                }
            }
            else
                result[1] = strSource;
            return result[0];
        }

        private void tabControl1_tabChanged(object sender, EventArgs e)
        {
            updateElements();
            selectedMode = tabControl1.SelectedIndex;
            updateOutput();
        }

        private void label8_Click(object sender, EventArgs e)
        {
            string info = "$l - Original Line" + Environment.NewLine +
                              "$n - Line Nr." + Environment.NewLine +
                              "$f - First Letter to Upper" + Environment.NewLine +
                              "$g - First Letter to Lower" + Environment.NewLine +
                              "$u - Line to Upper" + Environment.NewLine +
                              "$d - Line to Lower" + Environment.NewLine;
            MessageBox.Show(info, "Variables");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (InputTextbox.SelectedText.Length > 0)
            {
                InputTextbox.Text = InputTextbox.Text.Replace(InputTextbox.SelectedText, InputTextbox.SelectedText);
            }

            if (textBox1.Text.Length > 0)
            {

                InputTextbox.SelectionBackColor = Color.Empty;
                int index = 0;
                while (index < InputTextbox.Text.LastIndexOf(textBox1.Text))
                {
                    if (checkBox1.Checked)
                    {
                        InputTextbox.Find(textBox1.Text, index, InputTextbox.TextLength, RichTextBoxFinds.None);
                    }
                    else
                    {
                        InputTextbox.Find(textBox1.Text, index, InputTextbox.TextLength, RichTextBoxFinds.MatchCase);
                    }
                    
                    InputTextbox.SelectionBackColor = Color.Yellow;
                    index = InputTextbox.Text.IndexOf(textBox1.Text, index) + 1;
                }

            }
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Title: Advanced Replacer\n" +
                "Version: 1.0\n" +
                "Author: Max Ermackov\n" +
                "Copyright © EMX-Tech " + DateTime.Now.Year.ToString(), "Information about this Application");
        }

        private void eventUpdateOutput(object sender, EventArgs e)
        {
            updateOutput();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog theDialog = new SaveFileDialog();
            theDialog.Title = "Save as Text File";
            theDialog.Filter = "Text files|*.txt|All files|*.*";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(theDialog.FileName, OutputTextbox.Text, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file to disk. Original error: " + ex.Message);
                }
            }
        }
    }
}
