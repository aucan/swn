using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SingularizeSWN
{
   public class Singularize
    {
       public static void Sigularization()
       {
           SqlConnection conn = new SqlConnection(englishtoturkish.Properties.Settings.Default.filmreviewsConnectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            cmd.CommandText = 
                "SELECT swnid,POS,ID,PosScore,NegScore,SynsetTerms "+
                "FROM sentiwordnet "+
                "where SynsetTerms like '%#1'";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = conn;
            conn.Open();
            reader = cmd.ExecuteReader();  
            while (reader.Read())
            {
                foreach (string word in reader[5].ToString().Split(' '))
                {
                    if (word.EndsWith("#1"))
                        AddSingleRecord(reader[0].ToString(), reader[1].ToString(), reader[2].ToString(), reader[3].ToString(), reader[4].ToString(), word.Substring(0, word.Length - 2));
                }
            }
            conn.Close();
        }

       static void AddSingleRecord(string swnid, string POS, string ID, string PosScore, string NegScore, string EngTerm)
        {
            string sql = "INSERT INTO translatedswn " +
                "(swnid,POS,ID,PosScore,NegScore,EngTerm) " +
                "VALUES (@swnid,@POS,@ID,@PosScore,@NegScore,@EngTerm)";
            using (SqlConnection conn = new SqlConnection(englishtoturkish.Properties.Settings.Default.filmreviewsConnectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = 0;
                cmd.Parameters.Add("@swnid", SqlDbType.Int);
                cmd.Parameters.Add("@POS", SqlDbType.VarChar);
                cmd.Parameters.Add("@ID", SqlDbType.Int);
                cmd.Parameters.Add("@PosScore", SqlDbType.Float);
                cmd.Parameters.Add("@NegScore", SqlDbType.Float);
                cmd.Parameters.Add("@EngTerm", SqlDbType.VarChar);
                cmd.Parameters["@swnid"].Value = swnid;
                cmd.Parameters["@POS"].Value = POS;
                cmd.Parameters["@ID"].Value = ID;
                cmd.Parameters["@PosScore"].Value = float.Parse(PosScore);
                cmd.Parameters["@NegScore"].Value = float.Parse(NegScore);
                cmd.Parameters["@EngTerm"].Value = EngTerm;

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (SystemException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        
        }
    }

}
