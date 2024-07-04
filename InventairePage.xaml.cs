using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using WpfApp14;

namespace WpfApp14
{
    public partial class InventairePage : Page
    {
        public InventairePage()
        {
            InitializeComponent();
        }

        private void BtnChargerInventaire_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string connectionString = "Data Source=ADLENE\\SQLEXPRESS;Initial Catalog=InventaireCannabis;Integrated Security=True";
            string query = "SELECT * FROM Plantules";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                dataGridInventaire.ItemsSource = dataTable.DefaultView;
            }
        }

        private void BtnRetour_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (NavigationService != null)
            {
                NavigationService.GoBack();
            }
            else
            {
                Window mainWindow = new MainWindow();
                mainWindow.Show();
                Window.GetWindow(this)?.Close();
            }
        }

    }
}

