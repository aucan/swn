using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using TestTextToDataSet;
using SingularizeSWN;
using EngTurEr;

namespace englishtoturkish
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataSet ds = TextToDataSet.Convert("swn.txt", "SentiWordNet", "\t");
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = ds.Tables["SentiWordNet"];
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Singularize.Sigularization();
        }



        private void button3_Click(object sender, EventArgs e)
        {
            EngTur.Test();  
            
        }

    }
}
