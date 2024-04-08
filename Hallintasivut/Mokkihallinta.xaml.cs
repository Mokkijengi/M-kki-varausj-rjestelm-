using Microsoft.Maui.Controls;

using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System.Collections.ObjectModel;

namespace booking_VillageNewbies
{
    public partial class Mokkihallinta : ContentPage
    {
        public class Alue
        {
            public string Nimi { get; set; }
            public int Alue_Id { get; set; }

        }
        public ObservableCollection<string> MokkiNames { get; set; } = new ObservableCollection<string>(); //LISTA KAIKISTA MÖKISTÄ HAETTU DATABASESTA
        public ObservableCollection<Alue> alueList { get; set; } = new ObservableCollection<Alue>();


        public Mokkihallinta()
        {

            InitializeComponent();

            FetchMokkiNames();
            FetchAreasFromDatabase();

            BindingContext = this; // Set the BindingContext

        }

        private async void FetchAreasFromDatabase()
        {
            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "password";
            string constring = "SERVER=" + server + ";" + "DATABASE=" + database + ";" +
                "UID=" + username + ";" + "PASSWORD=" + password + ";";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    Console.WriteLine("Connection to the database successful.");

                    string selectQuery = "SELECT nimi,alue_id FROM vn.alue";


                    using (MySqlCommand cmd = new MySqlCommand(selectQuery, conn))
                    {
                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string alueName = reader.GetString("nimi");
                                int alueId = reader.GetInt32("alue_id"); // Fetch alue_id from the database
                                alueList.Add(new Alue { Nimi = alueName, Alue_Id = alueId }); // Populate Alue_Id property
                            }
                        }
                    }
                }

                // Set AlueList as the ItemsSource for aluePicker
                aluePicker.ItemsSource = alueList.Select(a => a.Nimi).ToList();
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Error", "Error connecting to the database: " + ex.Message, "OK");
                Console.WriteLine("Error connecting to the database: " + ex.Message);
            }
        }


        public async Task LisaaMokkiAsync()
        {
            int newMokkiId = -1; //-1 KOSKA DATABASE EI VARMASTI SILLOIN SEKOITA MÖKKI ID:TA
            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "password";
            string constring = "SERVER=" + server + ";" + "DATABASE=" + database + ";" +
                "UID=" + username + ";" + "PASSWORD=" + password + ";";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    conn.Open();
                    Console.WriteLine("Connection to the database successful.");

                    // Retrieve values from UI elements

                    string postinro = postiNro.Text;
                    string mokkinimi = mokinNimi.Text;
                    string katuosoite = katuOsoite.Text;
                    string hintaText = hinta.Text;
                    double hintaValue;
                    int alue_id = alueList[aluePicker.SelectedIndex].Alue_Id;

                    if (!double.TryParse(hintaText, out hintaValue))
                    {
                        await DisplayAlert("FAIL", "Failed to parse hinta.", "OK");
                        return;
                    }

                    string kuvaus = mokinKuvaus.Text;
                    string henkilomaaraText = henkiloMaara.Text;
                    int henkilomaaraValue;

                    if (!int.TryParse(henkilomaaraText, out henkilomaaraValue))
                    {
                        await DisplayAlert("FAIL", "Failed to parse henkilomaara.", "OK");
                        return;
                    }

                    string varustelu = mokinVarustelu.Text;

                    // Check if the postal code exists in the posti table
                    string checkPostiQuery = "SELECT COUNT(*) FROM posti WHERE postinro = @postinro";
                    int postiCount;


                    using (MySqlCommand checkPostiCmd = new MySqlCommand(checkPostiQuery, conn))
                    {
                        checkPostiCmd.Parameters.AddWithValue("@postinro", postinro);
                        postiCount = Convert.ToInt32(checkPostiCmd.ExecuteScalar());
                    }

                    if (postiCount == 0)
                    {
                        // Postal code does not exist, insert it into the posti table
                        string insertPostiQuery = "INSERT INTO posti (postinro) VALUES (@postinro)";

                        using (MySqlCommand insertPostiCmd = new MySqlCommand(insertPostiQuery, conn))
                        {
                            insertPostiCmd.Parameters.AddWithValue("@postinro", postinro);
                            insertPostiCmd.ExecuteNonQuery();
                        }
                    }

                    // Proceed with inserting the entry into the mokki table
                    string insertQuery = @"INSERT INTO mokki 
                    (alue_id, postinro, mokkinimi, katuosoite, hinta, kuvaus, henkilomaara, varustelu) 
                    VALUES 
                    (@alue_id, @postinro, @mokkinimi, @katuosoite, @hinta, @kuvaus, @henkilomaara, @varustelu)";

                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        // Add parameter values
                        cmd.Parameters.AddWithValue("@alue_id", alue_id);
                        cmd.Parameters.AddWithValue("@postinro", postinro);
                        cmd.Parameters.AddWithValue("@mokkinimi", mokkinimi);
                        cmd.Parameters.AddWithValue("@katuosoite", katuosoite);
                        cmd.Parameters.AddWithValue("@hinta", hintaValue);
                        cmd.Parameters.AddWithValue("@kuvaus", kuvaus);
                        cmd.Parameters.AddWithValue("@henkilomaara", henkilomaaraValue);
                        cmd.Parameters.AddWithValue("@varustelu", varustelu);

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            newMokkiId = (int)cmd.LastInsertedId;
                            Console.WriteLine("Mokki inserted successfully.");
                            await DisplayAlert("Success", "Mokki inserted successfully.", "OK");
                            // Assuming you have a method to get the mokki_id of the newly added record
                            await ShowData(newMokkiId);
                            await FetchMokkiNames();
                        }
                        else
                        {
                            Console.WriteLine("Failed to insert mokki.");
                            await DisplayAlert("FAIL", "Failed to insert mokki.", "OK");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error connecting to the database: " + ex.Message);
                await DisplayAlert("FAIL", "Error connecting to the database: " + ex.Message, "OK");
            }
        }



        //CLICK 
        private async void Lisaa_Mokki_Clicked(object sender, EventArgs e)
        {
            await LisaaMokkiAsync();


        }



        //NÄYTÄ VIIMEISIN LISÄTTY MÖKKI
        public async Task ShowData(int mokkiId)
        {
            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "password";
            string constring = "SERVER=" + server + ";" + "DATABASE=" + database + ";" +
                "UID=" + username + ";" + "PASSWORD=" + password + ";";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    Console.WriteLine("Connection to the database successful.");

                    string selectQuery = "SELECT * FROM mokki WHERE mokki_id = @mokkiId";

                    using (MySqlCommand cmd = new MySqlCommand(selectQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@mokkiId", mokkiId);

                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string alue_id = reader["alue_id"].ToString();
                                string postinro = reader["postinro"].ToString();
                                string mokkinimi = reader["mokkinimi"].ToString();
                                string katuosoite = reader["katuosoite"].ToString();
                                string hinta = reader["hinta"].ToString();
                                string kuvaus = reader["kuvaus"].ToString();
                                string henkilomaara = reader["henkilomaara"].ToString();
                                string varustelu = reader["varustelu"].ToString();

                                // Create a string to display the fetched data
                                string dataString = $"Mokki ID: {mokkiId}, Alue ID: {alue_id}, Postinro: {postinro}, Mokkinimi: {mokkinimi}, Katusoite: {katuosoite}, Hinta: {hinta}, Kuvaus: {kuvaus}, Henkilomaara: {henkilomaara}, Varustelu: {varustelu}";

                                // Display the fetched data in a DisplayAlert dialog
                                await DisplayAlert("Fetched Data", dataString, "OK");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Error", "Error connecting to the database: " + ex.Message, "OK");
                Console.WriteLine("Error connecting to the database: " + ex.Message);
            }
        }

        private async Task FetchMokkiNames()
        {
            MokkiNames.Clear(); // Clear existing items
            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "password";
            string constring = "SERVER=" + server + ";" + "DATABASE=" + database + ";" +
                "UID=" + username + ";" + "PASSWORD=" + password + ";";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    Console.WriteLine("Connection to the database successful.");

                    string selectQuery = "SELECT mokkinimi FROM mokki";

                    using (MySqlCommand cmd = new MySqlCommand(selectQuery, conn))
                    {
                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string mokkiName = reader["mokkinimi"].ToString();
                                MokkiNames.Add(mokkiName); // Add mokki name to collection
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Error", "Error connecting to the database: " + ex.Message, "OK");
                Console.WriteLine("Error connecting to the database: " + ex.Message);
            }
        }

        // Define a method to handle mokki deletion
        private async Task DeleteSelectedMokki(string selectedMokkiName)
        {
            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "password";
            string constring = "SERVER=" + server + ";" + "DATABASE=" + database + ";" +
                "UID=" + username + ";" + "PASSWORD=" + password + ";";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    Console.WriteLine("Connection to the database successful.");

                    // Delete all records from the mokki table where mokkinimi matches the selected name
                    string deleteQuery = "DELETE FROM mokki WHERE mokkinimi = @mokkiName";

                    using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@mokkiName", selectedMokkiName);
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            await DisplayAlert("Success", $"{selectedMokkiName} and associated records deleted successfully.", "OK");

                            await FetchMokkiNames();
                        }
                        else
                        {
                            await DisplayAlert("Fail", "Failed to delete mokki.", "OK");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Error", "Error connecting to the database: " + ex.Message, "OK");
                Console.WriteLine("Error connecting to the database: " + ex.Message);
            }
        }




        private async void poistaMokki_Clicked(object sender, EventArgs e)
        {
            if (mokkiPicker.SelectedItem != null)
            {
                string selectedMokkiName = mokkiPicker.SelectedItem.ToString();
                await DeleteSelectedMokki(selectedMokkiName);
            }
            else
            {
                await DisplayAlert("Error", "Please select a mokki to delete.", "OK");
            }
        }

    }
}


