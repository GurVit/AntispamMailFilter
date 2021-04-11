using D.Net.EmailClient;
using D.Net.EmailInterfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ISITLab2_o_
{
    public partial class Form1 : Form
    {
        IEmailClient ImapClient = EmailClientFactory.GetClient(EmailClientEnum.IMAP);
        List<IEmail> Msms = new List<IEmail>();
        public Form1()
        {
            InitializeComponent();
        }

        //проверка писем
        private void button1_Click(object sender, EventArgs e)
        {
            if (Msms.Count != 0)
            {
                int i = 0;
                foreach (var msm in Msms)
                {
                    bool isSpam = false;
                    double SpamPercent = 0.5;
                    string inputContent = msm.TextBody;
                    СleaningUpUnnecessary(ref inputContent);
                    msm.TextBody = inputContent;
                    inputContent = msm.Subject + " " + inputContent;
                    string[] AllWords = Separator(inputContent);
                    List<string> words = new List<string>();
                    foreach (var word in AllWords)
                    {
                        if (word.Length > 4 && word.Length < 100 && word.Substring(0, 4) != "http") words.Add(word);
                    }
                    Dictionary<string, int> dictionary = WordCounter(words);
                    List<string> words2check = WordCheck(dictionary);
                    Checking(ref SpamPercent, words2check);
                    SpamResult(SpamPercent, ref isSpam, i);
                    i++;
                }
            }
        }

        //проверка текста и обучение программы
        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                bool isSpam = false;
                double SpamPercent = 0.5;
                string inputContent = textBox1.Text;
                СleaningUpUnnecessary(ref inputContent);
                string[] AllWords = Separator(inputContent);
                List<string> words = new List<string>();
                foreach (var word in AllWords)
                {
                    if (word.Length > 4 && word.Length < 100 && word.Substring(0, 4) != "http") words.Add(word);
                }
                Dictionary<string, int> dictionary = WordCounter(words);
                List<string> words2check = WordCheck(dictionary);
                Checking(ref SpamPercent, words2check);
                SpamTextResult(SpamPercent, ref isSpam);
                ManualSetting(ref isSpam);
                BayesTheorem.Learning(words2check, isSpam);
                label7.Text = SpamPercent.ToString();
            }
        }

        public void СleaningUpUnnecessary(ref string inputContent)
        {
            for (int letterIndex = 0; letterIndex < inputContent.Length; letterIndex++)
            {
                if (inputContent[letterIndex] == '<')
                {
                    int startIndex = letterIndex;
                    while (inputContent[letterIndex] != '>')
                    {
                        letterIndex++;
                    }
                    
                    inputContent = inputContent.Remove(startIndex, letterIndex - startIndex);
                    letterIndex = startIndex;
                }
            }

            inputContent = inputContent.Replace("\t", " ");
            inputContent = inputContent.Replace("\n", " ");
            inputContent = inputContent.Replace("\r", " ");
            inputContent = inputContent.Replace(",", "");
            inputContent = inputContent.Replace(".", "");
            inputContent = inputContent.Replace(";", "");
            inputContent = inputContent.Replace(":", "");
            inputContent = inputContent.Replace("?", "");
            inputContent = inputContent.Replace("!", "");
            inputContent = inputContent.Replace("&", "");
            inputContent = inputContent.Replace("<", "");
            inputContent = inputContent.Replace(">", "");
        }

        public string[] Separator(string inputContent)
        {
            char separator = ' ';
            string[] words = inputContent.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            return words;
        }

        public Dictionary<string, int> WordCounter(List<string> words)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            foreach (var word in words)
            {
                if (!dictionary.ContainsKey(word.ToLower()))
                    dictionary.Add(word.ToLower(), 1);
                else
                    dictionary[word.ToLower()]++;
            }
            return dictionary;
        }

        List<string> WordCheck(Dictionary<string, int> dictionary)
        {
            List<string> words2check = new List<string>();
            for (int i = 0; i < 15; i++)
            {
                if (dictionary.Count != 0)
                {
                    int maxValue = dictionary.Values.Max();
                    words2check.Add(dictionary.First(x => x.Value == maxValue).Key.ToLower());
                    dictionary.Remove(words2check[i]);
                }
            }
            return words2check;
        }

        public void Checking(ref double SpamPercent, List<string> words2check)
        {
            double NeSpamPersent = 0;

            string DBaseFirstValue = BayesTheorem.FindSpamWord(words2check[0]);
            if (DBaseFirstValue == "")
            {
                DBaseFirstValue = "0,5";
            }
            SpamPercent = Convert.ToDouble(DBaseFirstValue);
            NeSpamPersent = 1 - SpamPercent;
            bool isTheFirst = true;

            

            foreach (var word2check in words2check)
            {
                if (isTheFirst)
                {
                    isTheFirst = false;
                    continue;
                }
                string DBaseValue = BayesTheorem.FindSpamWord(word2check);
                if (DBaseValue == "")
                {
                    DBaseValue = "0,5";
                }

                double P_X = SpamPercent;
                double P_Y = Convert.ToDouble(DBaseValue);

                SpamPercent *= P_Y;
                NeSpamPersent *= (1 - P_Y);

                SpamPercent = P_X * P_Y / (P_X * P_Y + (1 - P_X) * (1 - P_Y));
            }

            //SpamPercent = SpamPercent / (SpamPercent + NeSpamPersent);
        }

        public void ManualSetting(ref bool isSpam)
        {
            if (radioButton1.Checked)
            {
                isSpam = true;
                //radioButton1.Checked = false;
            }
            if (radioButton2.Checked)
            {
                isSpam = false;
                //radioButton2.Checked = false;
            }
        }

        public void SpamTextResult(double SpamPercent, ref bool isSpam)
        {
            if (SpamPercent > 0.8)
            {
                label8.Text = "Spam Mail";
                label8.BackColor = Color.Red;
                isSpam = true;
            }
            else
            {
                label8.Text = "NonSpam Mail";
                label8.BackColor = Color.Green;
            }
        }

        public void SpamResult(double SpamPercent, ref bool isSpam, int i)
        {
            string listItemBefore = "";
            if (listBox1.Items[i].ToString().Length > 80)
                listItemBefore = listBox1.Items[i].ToString().Substring(0, 80);
            else listItemBefore = listBox1.Items[i].ToString();
            listBox1.Items.RemoveAt(i);
            if (SpamPercent > 0.8)
            {
                listBox1.Items.Insert(i, string.Format("{0, -80} {1}",listItemBefore, $"SPAM {Math.Round(SpamPercent, 2)}"));
                isSpam = true;
            }
            else
            {
                listBox1.Items.Insert(i, string.Format("{0, -80} {1}", listItemBefore, Math.Round(SpamPercent, 2)));
                isSpam = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            label7.Text = "";
            label8.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                ImapClient.Connect("imap.gmail.com", textBox2.Text, textBox3.Text, 993, true);
                checkBox1.Checked = true;
            }
            catch
            {
            }
        }

        //загрузка писем в listbox
        private void button4_Click(object sender, EventArgs e)
        {
            Msms.Clear();
            listBox1.Items.Clear();
            ImapClient.SetCurrentFolder("INBOX");
            ImapClient.LoadMessages();
            {
                for (int i = 0; i < ImapClient.Messages.Count; i++)
                {
                    if (i == numericUpDown1.Value) break;
                    IEmail msm = (IEmail)ImapClient.Messages[i];
                    msm.LoadInfos();
                    Msms.Add(msm);
                }
            }
            foreach (var msm in Msms)
            {
                if (msm.Subject.Length > 55 && msm.From[0].Length > 20)
                {
                    listBox1.Items.Add(msm.From[0].Substring(0, 20) + "\t" + msm.Subject.Substring(0, 55));
                }
                else
                {
                    if (msm.From[0].Length > 20)
                    {
                        listBox1.Items.Add(msm.From[0].Substring(0, 20) + "\t" + msm.Subject);
                    }
                    else
                    {
                        listBox1.Items.Add(msm.From[0] + "\t" + msm.Subject);
                    }
                }

            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                textBox1.Text = Msms[listBox1.SelectedIndex].Subject + Environment.NewLine + Msms[listBox1.SelectedIndex].TextBody;
            }
        }
    }
}

