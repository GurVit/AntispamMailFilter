using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISITLab2_o_
{
    public class BayesTheorem
    {
        static AntispamContext db = new AntispamContext();
        public static string FindSpamWord(string word)
        {
            if (word.Length > 6) word = word.Substring(0, word.Length - 2);
            antispam item = db.antispam.FirstOrDefault(p => p.Word.Contains(word));
            if (item != null && word.Length > 3)
                return (item.Spam * 1.0 / (item.Spam * 1.0 + item.Nspam * 1.0)).ToString();
            else return "";
        }

        public static void Learning(List<string> words2learn, bool IsSpam)
        {
            foreach (var word in words2learn)
            {
                string word1 = word;
                if (word.Length > 6) word1 = word.Substring(0, word.Length - 2);
                antispam item = db.antispam.FirstOrDefault(p => p.Word.Contains(word1.ToLower()));
                if (word.Length > 3)
                {
                    if (item == null)
                    {
                        antispam item2add;
                        if (IsSpam) item2add = new antispam() { Word = word.ToLower(), Nspam = 0, Spam = 1 };
                        else item2add = new antispam() { Word = word.ToLower(), Nspam = 1, Spam = 0 };
                        db.antispam.Add(item2add);
                    }
                    else
                    {
                        if (IsSpam) item.Spam++;
                        else item.Nspam++;
                    }
                    db.SaveChanges();

                }
            }
            
        }
    }
}
