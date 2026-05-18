using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTrack.Database;

namespace AutoTrack
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Test database connection
            if (DatabaseHelper.TestConnection())
                MessageBox.Show("Connected to AutoTrackDB successfully!",
                    "Connection OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Connection failed. Check your database.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}