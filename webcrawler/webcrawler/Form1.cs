using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Globalization;
using net.zemberek.erisim;
using net.zemberek.tr.yapi;
using net.zemberek.yapi;



namespace webcrawler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string siteurl = "http://www.beyazperde.com/filmler/tum-filmleri/kullanici-puani/";
        string pageurl = "http://www.beyazperde.com/filmler/tum-filmleri/kullanici-puani/?page={0}";
        string filmurl = "http://www.beyazperde.com{0}kullanici-elestirileri/";
        int frewcount = 0;
        float frate = 0;
        string rlink="";
        string rcontent="";
        string fresidue = "";
        float rrating = 0;
        int pagecount = 0;

        public string ConvertUTF(string AStr) {
            byte[] bytes = Encoding.Default.GetBytes(AStr);
            return Encoding.UTF8.GetString(bytes);
        }

        public float StrToFloat(string Astr)
        {
            return float.Parse(Astr.Replace('.', ','));
        }

        public float FindRating(string AStr)
        {
            return StrToFloat(AStr.Substring(AStr.IndexOf("ratingDecimal") + 15, 3));
        }

        public string FindSid(string AStr)
        {
            return AStr.Substring(AStr.IndexOf("-") + 1, AStr.Length - AStr.IndexOf("-")-2);
        }

        public int MaxItemInStringList(List<string> strList)
        {
            int max = 0;
            int tmp = 0;
            foreach (string item in strList)
            {
                try
                {
                    tmp = Convert.ToInt32(item);
                }
                catch (FormatException ex) { adderrorlog(ex.Message); }
                if (max < tmp)
                    max = Convert.ToInt32(item);

            }
            return max;  
        }

        public HtmlAgilityPack.HtmlDocument LoadPage(string AUrl){
            Uri url = new Uri(AUrl);
            WebClient client = new WebClient();
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            try
            {
                string html = client.DownloadString(url);
                document.LoadHtml(html);
            }
            catch (Exception ex)
            {
                LoadPage(AUrl);
                adderrorlog(ex.Message);
            }
            return document;        
        }

        public int GetReviewPageCount(HtmlAgilityPack.HtmlDocument document)
        {
            int count = 0;
            try
            {
                if (frewcount > 10)
                    count = MaxItemInStringList(document.DocumentNode.SelectNodes("//div[@class='pager ']//ul//li").Select(node => node.InnerText).ToList());
            }
            catch (FormatException ex) { adderrorlog(ex.Message); }
            return count;
        }

        public int GetReviewCount(HtmlAgilityPack.HtmlDocument document)
        {
            int count = 0;
            HtmlAgilityPack.HtmlNodeCollection col = document.DocumentNode.SelectNodes("//span[@itemprop='reviewCount']");
            try
            { 
                if (col!= null)
                    count=Int32.Parse(col[0].InnerText);
            }
            catch (FormatException ex) { adderrorlog(ex.Message); }
            return count;
        }

        public float GetRating(HtmlAgilityPack.HtmlDocument document)
        {
            float count = 0;
            HtmlAgilityPack.HtmlNodeCollection col = document.DocumentNode.SelectNodes("//span[@itemprop='ratingValue']");
            try
            { if (col!= null)
                count =StrToFloat(col[0].Attributes["content"].Value);
            }
            catch (FormatException ex) { adderrorlog(ex.Message); }
            return count;
        }
        
        public void GetReviews(string AUrl) {
            HtmlAgilityPack.HtmlDocument document = LoadPage(AUrl);
            frate = GetRating(document);
            frewcount = GetReviewCount(document);    
            pagecount = GetReviewPageCount(document);
            progressBar2.Maximum = frewcount;
        }

        public int FindFilmPageCount(string AUrl)
        {
            HtmlAgilityPack.HtmlDocument document = LoadPage(AUrl);
            return MaxItemInStringList(document.DocumentNode.SelectNodes("//div[@class='pager navbar margin_20t']//ul//li").Select(node => node.InnerText).ToList());    
        }

        public HtmlAgilityPack.HtmlNodeCollection FindFilm(string AUrl)
        {
            HtmlAgilityPack.HtmlDocument document = LoadPage(AUrl);            
            return document.DocumentNode.SelectNodes("//div[@class='content']//h2//a[@href]");
        }

        public void Crawling(int StartPage)
        {
            int fpcnt=FindFilmPageCount(siteurl);
            int percent=0;
            progressBar1.Maximum = fpcnt*20;
            for (int i = StartPage; i < fpcnt; i++)
            {
                foreach (HtmlAgilityPack.HtmlNode Film in FindFilm(String.Format(pageurl, i)))
                {
                    AddFilmRecord(Film);
                    progressBar1.Value++;
                    percent = (int)(((double)progressBar1.Value / (double)progressBar1.Maximum) * 100);
                    progressBar1.CreateGraphics().DrawString(percent.ToString() + "%", new Font("Arial", (float)8.25, FontStyle.Regular), Brushes.Black, new PointF(progressBar1.Width / 2 - 10, progressBar1.Height / 2 - 7));
                }                
            }
        }


        public void AddFilmRecord(HtmlAgilityPack.HtmlNode Film)
        {
            GetReviews(String.Format(filmurl, Film.Attributes["href"].Value));
            Int32 newFilmID = 0;
            string sql =
                "INSERT INTO films (sid,fname,flink,frate,frewcount) VALUES (@sid,@fname,@flink,@frate,@frewcount); "
                + "SELECT CAST(scope_identity() AS int)";
            using (SqlConnection conn = new SqlConnection(webcrawler.Properties.Settings.Default.filmreviewsConnectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@sid", SqlDbType.Int);
                cmd.Parameters.Add("@fname", SqlDbType.VarChar);
                cmd.Parameters.Add("@flink", SqlDbType.VarChar);
                cmd.Parameters.Add("@frate", SqlDbType.Decimal);
                cmd.Parameters.Add("@frewcount", SqlDbType.Int);
                cmd.Parameters["@sid"].Value = FindSid(Film.Attributes["href"].Value);
                cmd.Parameters["@fname"].Value = ConvertUTF(Film.InnerText);
                cmd.Parameters["@flink"].Value = Film.Attributes["href"].Value;
                cmd.Parameters["@frate"].Value = frate;
                cmd.Parameters["@frewcount"].Value = frewcount;
                frate = 0;
                frewcount = 0;
                try
                {
                    conn.Open();
                    newFilmID = (Int32)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    adderrorlog(ex.Message);
                }
            }

        //    if (newFilmID == 0)
          //      MessageBox.Show("film cannot added");
            AddReviewRecords((int)newFilmID, String.Format(filmurl, Film.Attributes["href"].Value));
        }

        private void AddReviewRecords(int fid,string AUrl)
        {
            HtmlAgilityPack.HtmlDocument document;
            int percent;
                for (int i = 1; i <= pagecount; i++)
                {
                    document = LoadPage(AUrl + "?page=" + i.ToString());
                    HtmlAgilityPack.HtmlNodeCollection reviews = document.DocumentNode.SelectNodes("//p[@itemprop='description']");
                    foreach (HtmlAgilityPack.HtmlNode review in reviews)
                    {
                        progressBar2.Value++;
                        percent = (int)(((double)progressBar2.Value / (double)progressBar2.Maximum) * 100);
                        progressBar2.CreateGraphics().DrawString(percent.ToString() + "%", new Font("Arial", (float)8.25, FontStyle.Regular), Brushes.Black, new PointF(progressBar2.Width / 2 - 10, progressBar2.Height / 2 - 7));
                        rlink = AUrl;
                        rcontent = ConvertUTF(review.InnerText);
                        rrating = FindRating(review.Attributes["data-entities"].Value);
                        fresidue = "";
                        AddReviewRecord(fid);
                    }                    
                }
            progressBar2.Value = 0;
        }

        private int AddReviewRecord(int fid)
        {
            Int32 newReviewID = 0;
            string sql =
                "INSERT INTO reviews (fid,rlink,rcontent,rrating,rfiltered,rresidue) VALUES (@fid,@rlink,@rcontent,@rrating,@rfiltered,@rresidue); "
                + "SELECT CAST(scope_identity() AS int)";
            using (SqlConnection conn = new SqlConnection(webcrawler.Properties.Settings.Default.filmreviewsConnectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@fid", SqlDbType.Int);
                cmd.Parameters.Add("@rlink", SqlDbType.VarChar);
                cmd.Parameters.Add("@rcontent", SqlDbType.VarChar);
                cmd.Parameters.Add("@rrating", SqlDbType.Decimal);
                cmd.Parameters.Add("@rfiltered", SqlDbType.VarChar);
                cmd.Parameters.Add("@rresidue", SqlDbType.VarChar);
                cmd.Parameters["@fid"].Value = fid;
                cmd.Parameters["@rlink"].Value = rlink;
                cmd.Parameters["@rcontent"].Value = rcontent;
                cmd.Parameters["@rrating"].Value = rrating;
                cmd.Parameters["@rfiltered"].Value = "";// filtering(rcontent);
                cmd.Parameters["@rresidue"].Value = "";//rresidue;
                try
                {
                    conn.Open();
                    newReviewID = (Int32)cmd.ExecuteScalar();             
                }
                catch (Exception ex)
                {
                    adderrorlog(ex.Message);
                }
            }
      //      if (newReviewID == 0)
        //        MessageBox.Show("review cannot added");
            return (int)newReviewID;
        }

        private void button1_Click(object sender, EventArgs e)
        {
           // Crawling(1);
        }

        public string Normalize(string RStr)
        {
            string[] clear = { ".", ",", "!", "?", "\n", "'", "/", "-", "/", "&quot;", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            foreach (string item in clear)
                RStr = RStr.Replace(item, "");
            RStr = RStr.ToLower(CultureInfo.CurrentCulture);
            RStr = RStr.Replace("türk", "Türk");
            return RStr.Trim();
        }

        private string filtering(string Astr)
        {             
            string filtered = "";
            string checkedword = "";            
            Zemberek filter = new Zemberek(new TurkiyeTurkcesi());
            Astr = Normalize(Astr);
            foreach (string word in Astr.Split(' '))
            {
                if (filter.kelimeDenetle(word))
                {
                    checkedword = word;
                }
                else
                {
                    String[] suggestions = filter.asciidenTurkceye(word);
                    if (suggestions.Length > 0)
                    {
                        checkedword = suggestions[0];
                    }
                    else
                    {
                        try
                        {
                            suggestions = filter.oner(word);
                        }
                        catch (System.NullReferenceException ex)
                        {
                            adderrorlog(ex.Message);
                        }

                        if (suggestions.Length>0)
                        {                            
                            checkedword = suggestions[0];
                        }
                        else
                            fresidue += word + " ";
                    }
                }
                Kelime[] solutions = filter.kelimeCozumle(checkedword);
                foreach (Kelime solution in solutions)
                    {
                        if (!filtered.Contains(solution.kok().icerik()))
                        {
                            filtered += solution.kok().icerik()+ " ";
                        }
                    }                
            }
            return filtered;
        }

        void FilterAllReviews()
        {
            SqlConnection conn = new SqlConnection(webcrawler.Properties.Settings.Default.filmreviewsConnectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            cmd.CommandText = "select rcontent,rid,rrating from reviews where rid>(SELECT MAX(rid) FROM filteredreviews) order by rid";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = conn;
            conn.Open();
            reader = cmd.ExecuteReader();  
            while (reader.Read())
            {
                FilterReview(reader["rcontent"].ToString(), Int32.Parse(reader["rid"].ToString()), float.Parse(reader["rrating"].ToString()));
                progressBar3.Value = Int32.Parse(reader["rid"].ToString());
            }
            conn.Close();
        }

        void FilterReview(string review,int rid,float rating)
        {
            string sql = "INSERT INTO filteredreviews (rid,frating,ffiltered,fresidue) VALUES (@rid,@frating,@ffiltered,@fresidue);";
            using (SqlConnection conn = new SqlConnection(webcrawler.Properties.Settings.Default.filmreviewsConnectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = 0;
                cmd.Parameters.Add("@rid", SqlDbType.Int);
                cmd.Parameters.Add("@frating", SqlDbType.Float);
                cmd.Parameters.Add("@ffiltered", SqlDbType.VarChar);
                cmd.Parameters.Add("@fresidue", SqlDbType.VarChar);
                cmd.Parameters["@rid"].Value = rid;
                cmd.Parameters["@frating"].Value = rating;
                cmd.Parameters["@ffiltered"].Value = filtering(review);
                cmd.Parameters["@fresidue"].Value =fresidue;
                fresidue = "";
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (SystemException ex)
                {
                    adderrorlog(ex.Message);
                }
            }
        
        }

        void adderrorlog(string strErrorMsg)
        { 
             string sql = "INSERT INTO errorlogs (errtime,errmessage) VALUES (@errtime,@message);";
             using (SqlConnection conn = new SqlConnection(webcrawler.Properties.Settings.Default.filmreviewsConnectionString))
             {
                 SqlCommand cmd = new SqlCommand(sql, conn);
                 cmd.Parameters.Add("@errtime", SqlDbType.Timestamp);
                 cmd.Parameters.Add("@message", SqlDbType.VarChar);
                 cmd.Parameters["@errtime"].Value = DateTime.Now;
                 cmd.Parameters["@message"].Value = strErrorMsg;
                 try
                 {
                     conn.Open();
                     cmd.ExecuteNonQuery();
                 }
                 catch (SystemException ex)
                 { }
             }                    
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FilterAllReviews();
        }
    }
}
