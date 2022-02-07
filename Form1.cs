using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using ECO.VeriFi.Importing.Excel;
using System.Text.RegularExpressions;
using System.Globalization;
using OfficeOpenXml;
using System.IO;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Live_Funds_Data_Exporter
{
    public partial class FundsDataExporterForm : Form
    {
        public FundsDataExporterForm()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            // Open file dialog until all files are selected
            FileChoice.openDialog();

            // Now store the sheets in an array of lists of objects
            for (int loop = 0; loop < Globals.numSheets; loop++)
            {
                ProcessSheet.CreateObjectList(loop);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            // Send sheet data to DB

            // Connect to DB
            using (MySqlConnection conn = new MySqlConnection(Globals.connString))
            {
                conn.Open();

                // Cycle through imported sheet data
                for (int loop = 0; loop < Globals.numSheets; loop++)
                {
                    //Get list out of array:
                    List<object> currentList = Globals.listarr[loop];

                    // Determine list type, applicable table, etc.:

                    //Get each object out of the list and Populate relevant table
                    DataToDB.PopulateTblByType(currentList, loop, conn);

                };
            }
        }
    }
}

// By J. Koekemoer