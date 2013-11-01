using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace EngTurEr
{
    class EngTur
    {
        private static HtmlAgilityPack.HtmlDocument LoadPage(string AUrl)
        {
            var web = new HtmlAgilityPack.HtmlWeb();
            web.AutoDetectEncoding = true;
            var document = new HtmlAgilityPack.HtmlDocument();
            try
            {
                document = web.Load(AUrl);
            }
            catch (Exception ex)
            {
                LoadPage(AUrl);
            }
            return document;
        }

        private static string GetTurkishFromTurEng(string english)
        {
            string turkish = "";
            var document = LoadPage("http://tureng.com/search/" + english);
            var col = document.DocumentNode.SelectNodes("//table[@id='englishResultsTable']//tr[2]//td[5]//a");
            try
            {
                if (col != null)
                    turkish = WebUtility.HtmlDecode(col[0].InnerText);
            }
            catch (FormatException ex) { }
            return turkish;
        }

        private static string GetTurkishFromZargan(string english)
        {
            string turkish = "";
            var document = LoadPage("http://www.zargan.com/sozluk.asp?sozcuk=" + english.Replace(" ", "+"));
            var col = document.DocumentNode.SelectNodes("/html/body/div/section/section[4]/div/table/tbody/tr/td[4]/a");
            try
            {
                if (col != null)
                    turkish = WebUtility.HtmlDecode(col[0].InnerText);
            }
            catch (FormatException ex) { }
            return turkish;
        }

        private static string GetTurkishFromGoogle(string english)
        {
            string turkish = "";
            var document = LoadPage("http://translate.google.com/#en/tr/" + english);
            var col = document.DocumentNode.SelectNodes("//span[@id='result_box']//span");
            try
            {
                if (col != null)
                    turkish = WebUtility.HtmlDecode(col[0].InnerText);
            }
            catch (FormatException ex) { }
            return turkish;
        }

        private static string GetTurkish(string english)
        {
            string turkish = "";
            turkish = GetTurkishFromTurEng(english);
            if (turkish == "")
                turkish = GetTurkishFromZargan(english);
            if (turkish == "")
                turkish = GetTurkishFromGoogle(english);
            Console.WriteLine("{0} : {1}", english, turkish);
            return turkish;                    
        }

        private static void InsertARecord(int mswnid,string turterm)
        {
            string sql =
                "INSERT INTO temptranslate (mswnid, turterm) values (@mswnid, qturterm)";
            using (SqlConnection conn = new SqlConnection(englishtoturkish.Properties.Settings.Default.filmreviewsConnectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@mswnid", SqlDbType.Int);
                cmd.Parameters.Add("@turterm", SqlDbType.VarChar);
                cmd.Parameters["@mswnid"].Value = mswnid;
                cmd.Parameters["@turterm"].Value = turterm;
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {}        
            }
        }

        private static string NormalizeWord(string AStr)
        {
            return AStr.Replace("-", " ").Replace("_", " ");
        }

        public static void Translate()
        {
            SqlConnection conn = new SqlConnection(englishtoturkish.Properties.Settings.Default.filmreviewsConnectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            cmd.CommandText = "select mswnid,engterm "+
                              "from translatedswn "+
                              "where (posscore + negscore)> 0 "+
	                          "and turterm is null";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = conn;
            conn.Open();
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                InsertARecord(reader.GetInt32(0), GetTurkish(NormalizeWord(reader.GetString(1))));
            }
            conn.Close();        
        }

        public static void Test()
        {
            MessageBox.Show(
            GetTurkishFromTurEng("hello")+
            GetTurkishFromZargan("hello")+
            GetTurkishFromGoogle("hello"));
        }
    }
}
