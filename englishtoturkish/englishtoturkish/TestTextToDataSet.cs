using System;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace TestTextToDataSet
{
    public class TextToDataSet
    {

        public static DataSet Convert(string File, string TableName, string delimiter)
        {
            DataSet result = new DataSet();
            StreamReader s = new StreamReader(File);
            result.Tables.Add(TableName);
            result.Tables[TableName].Columns.Add("POS");
            result.Tables[TableName].Columns.Add("ID");
            result.Tables[TableName].Columns.Add("PosScore");
            result.Tables[TableName].Columns.Add("NegScore");
            result.Tables[TableName].Columns.Add("SynsetTerms");
            result.Tables[TableName].Columns.Add("Gloss");
            string AllData = s.ReadToEnd();
            string[] rows = AllData.Split("\r".ToCharArray());
            foreach (string r in rows)
            {
                string[] items = r.Split(delimiter.ToCharArray());
                result.Tables[TableName].Rows.Add(items);
                //AddSWNRecord(items[0], items[1], items[2], items[3], items[4], items[5]);
            }
            return result;
        }

        private static void AddSWNRecord(string POS, string ID, string PosScore, string NegScore, string SynsetTerms, string Gloss)
        {
            string sql =
                "INSERT INTO sentiwordnet (POS,ID,PosScore,NegScore,SynsetTerms,Gloss) VALUES (@POS,@ID,@PosScore,@NegScore,@SynsetTerms,@Gloss)";
            using (SqlConnection conn = new SqlConnection(englishtoturkish.Properties.Settings.Default.filmreviewsConnectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@POS", SqlDbType.VarChar);
                cmd.Parameters.Add("@ID", SqlDbType.VarChar);
                cmd.Parameters.Add("@PosScore", SqlDbType.Float);
                cmd.Parameters.Add("@NegScore", SqlDbType.Float);
                cmd.Parameters.Add("@SynsetTerms", SqlDbType.VarChar);
                cmd.Parameters.Add("@Gloss", SqlDbType.VarChar);
                cmd.Parameters["@POS"].Value = POS.Replace('\n', ' ').Trim();
                cmd.Parameters["@ID"].Value = ID;
                cmd.Parameters["@PosScore"].Value = StrToFloat(PosScore);
                cmd.Parameters["@NegScore"].Value = StrToFloat(NegScore);
                cmd.Parameters["@SynsetTerms"].Value = SynsetTerms;
                cmd.Parameters["@Gloss"].Value = Gloss;
                try
                {
                    conn.Open();
                    cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        static float StrToFloat(string Astr)
        {
            return float.Parse(Astr.Replace('.', ','));
        }
    }
}