using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TicketSellingModule.Repo;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TicketSellingModule
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            TestDatabaseConnection();
        }

        private void TestDatabaseConnection()
        {
            DbConnectionFactory factory = new DbConnectionFactory();

            try
            {
                // The 'using' block ensures the door is closed immediately after the test
                using (SqlConnection connection = factory.GetConnection())
                {
                    connection.Open(); // Attempt to open the door
                    Debug.WriteLine("SUCCESS: The database connection is working perfectly!");
                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine($"ERROR: Could not connect to the database. Reason: {ex.Message}");
            }
        }
    }
}
