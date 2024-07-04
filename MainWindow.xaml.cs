using System;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Data;
using QRCoder;
using System.Drawing;
using System.IO;
using OfficeOpenXml;
using Microsoft.Win32;
using WpfApp14;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace WpfApp14
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }
        


        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            string identification = txtIdentification.Text;
            string? etatSante = ((ComboBoxItem)cmbEtatSante.SelectedItem)?.Content.ToString();
            DateTime? dateArrivee = dpDateArrivee.SelectedDate;
            string provenance = txtProvenance.Text;
            string description = txtDescription.Text;
            string? stade = ((ComboBoxItem)cmbStade.SelectedItem)?.Content.ToString();
            string entreposage = cmbEntreposage.Text;  // À compléter avec données dynamiques
            bool actif = chkActif.IsChecked ?? false;
            DateTime? dateRetrait = dpDateRetrait.SelectedDate;
            string raisonRetrait = cmbRaisonRetrait.Text;
            string? responsable = ((ComboBoxItem)cmbResponsable.SelectedItem)?.Content.ToString();
            string note = txtNote.Text;

            string connectionString = "Data Source=ADLENE\\SQLEXPRESS;Initial Catalog=InventaireCannabis;Integrated Security=True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Plantules (Identification, EtatSante, DateArrivee, Provenance, Description, Stade, Entreposage, Actif, DateRetrait, RaisonRetrait, Responsable, Note) " +
                               "VALUES (@Identification, @EtatSante, @DateArrivee, @Provenance, @Description, @Stade, @Entreposage, @Actif, @DateRetrait, @RaisonRetrait, @Responsable, @Note)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Identification", identification);
                command.Parameters.AddWithValue("@EtatSante", etatSante);
                command.Parameters.AddWithValue("@DateArrivee", dateArrivee.HasValue ? (object)dateArrivee.Value : DBNull.Value);
                command.Parameters.AddWithValue("@Provenance", provenance);
                command.Parameters.AddWithValue("@Description", description);
                command.Parameters.AddWithValue("@Stade", stade);
                command.Parameters.AddWithValue("@Entreposage", entreposage);
                command.Parameters.AddWithValue("@Actif", actif);
                command.Parameters.AddWithValue("@DateRetrait", dateRetrait.HasValue ? (object)dateRetrait.Value : DBNull.Value);
                command.Parameters.AddWithValue("@RaisonRetrait", raisonRetrait);
                command.Parameters.AddWithValue("@Responsable", responsable);
                command.Parameters.AddWithValue("@Note", note);

                command.ExecuteNonQuery();
            }

            GenererQrCodeEtAfficher(identification);

            MessageBox.Show("Plantule enregistrée avec succès !");
        }

        private void BtnVoirInventaire_Click(object sender, RoutedEventArgs e)
        {
            InventairePage inventairePage = new InventairePage();
            Content = inventairePage;
        }

        private void BtnImporter_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                ImporterDonneesDepuisExcel(filePath);
            }
        }

        private void ImporterDonneesDepuisExcel(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // Supposant que la première ligne contient les en-têtes
                {
                    string identification = worksheet.Cells[row, 1].Text;
                    string etatSante = worksheet.Cells[row, 2].Text;

                    DateTime dateArrivee;
                    if (!DateTime.TryParseExact(worksheet.Cells[row, 3].Text, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out dateArrivee))
                    {
                        // Gérer les dates non valides ou manquantes
                        dateArrivee = DateTime.MinValue; // ou une autre valeur par défaut
                    }

                    string provenance = worksheet.Cells[row, 4].Text;
                    string description = worksheet.Cells[row, 5].Text;
                    string stade = worksheet.Cells[row, 6].Text;
                    string entreposage = worksheet.Cells[row, 7].Text;
                    bool actif = worksheet.Cells[row, 8].Text == "1";

                    DateTime? dateRetrait = null;
                    if (!string.IsNullOrEmpty(worksheet.Cells[row, 9].Text))
                    {
                        DateTime tempDateRetrait;
                        if (DateTime.TryParseExact(worksheet.Cells[row, 9].Text, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out tempDateRetrait))
                        {
                            dateRetrait = tempDateRetrait;
                        }
                    }

                    string raisonRetrait = worksheet.Cells[row, 10].Text;
                    string responsable = worksheet.Cells[row, 11].Text;
                    string note = worksheet.Cells[row, 12].Text;

                    EnregistrerPlantule(identification, etatSante, dateArrivee, provenance, description, stade, entreposage, actif, dateRetrait, raisonRetrait, responsable, note);
                    GenererQrCodeEtAfficher(identification); // Générer le QR code pour chaque ligne importée
                }
            }

            MessageBox.Show("Importation terminée avec succès !");
        }


        private void EnregistrerPlantule(string identification, string etatSante, DateTime dateArrivee, string provenance, string description, string stade, string entreposage, bool actif, DateTime? dateRetrait, string raisonRetrait, string responsable, string note)
        {
            string connectionString = "Data Source=ADLENE\\SQLEXPRESS;Initial Catalog=InventaireCannabis;Integrated Security=True";
            string query = "INSERT INTO Plantules (Identification, EtatSante, DateArrivee, Provenance, Description, Stade, Entreposage, Actif, DateRetrait, RaisonRetrait, Responsable, Note) " +
                           "VALUES (@Identification, @EtatSante, @DateArrivee, @Provenance, @Description, @Stade, @Entreposage, @Actif, @DateRetrait, @RaisonRetrait, @Responsable, @Note)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Identification", identification);
                command.Parameters.AddWithValue("@EtatSante", etatSante);
                command.Parameters.AddWithValue("@DateArrivee", dateArrivee);
                command.Parameters.AddWithValue("@Provenance", provenance);
                command.Parameters.AddWithValue("@Description", description);
                command.Parameters.AddWithValue("@Stade", stade);
                command.Parameters.AddWithValue("@Entreposage", entreposage);
                command.Parameters.AddWithValue("@Actif", actif);
                command.Parameters.AddWithValue("@DateRetrait", dateRetrait.HasValue ? (object)dateRetrait.Value : DBNull.Value);
                command.Parameters.AddWithValue("@RaisonRetrait", raisonRetrait);
                command.Parameters.AddWithValue("@Responsable", responsable);
                command.Parameters.AddWithValue("@Note", note);

                connection.Open();
                command.ExecuteNonQuery();
            }

            GenererQrCodeEtAfficher(identification);
        }

        private void GenererQrCodeEtAfficher(string text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            using (MemoryStream memory = new MemoryStream())
            {
                qrCodeImage.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                imgQrCode.Source = bitmapImage;
                imgQrCode.Visibility = Visibility.Visible;
            }
        }
    }
}



