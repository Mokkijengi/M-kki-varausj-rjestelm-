using Microsoft.Maui.Controls;

using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System.Collections.ObjectModel;
using System.Data;

namespace booking_VillageNewbies
{




    public partial class Mokkihallinta : ContentPage
    {



        public ObservableCollection<string> CabinNames { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<Alue> Alueet { get; set; } = new ObservableCollection<Alue>();

        //Muuttuja valitulle alueelle
        public Alue ValittuAlue { get; set; }

        public Mokkihallinta()
        {

            InitializeComponent();

            BindingContext = this;//Binding sitoo tiedot XAML näkymään

            Task.Run(async () => await InitializeDataAsync());


            cabinListPicker.SelectedIndexChanged += cabinListPicker_SelectedIndexChanged;

            //kun mökki valitaan listalta, avaa pääsivulle sen tiedot. 

            // Add a SelectionChanged event handler for aluePicker
            aluePicker.SelectedIndexChanged += async (sender, args) =>
            {
                if (aluePicker.SelectedIndex != -1)
                {
                    string selectedAlue = Alueet[aluePicker.SelectedIndex].Nimi;
                    await FetchCabinNamesByAlue(selectedAlue);

                }
            };

            cabinListPicker.ItemsSource = CabinNames;

        }
        private async Task InitializeDataAsync()
        {
            await HaeAlueNimet();//Haetaan nimet ja päivitetään ne Alueet-kokoelmaan
        }
        private async Task HaeAlueNimet()
        {
            string constring = $"SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=VN_password;";
            Alueet.Clear(); // Tyhjennä lista ennen uuden datan hakemista

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    string selectQuery = @"SELECT alue_id, nimi FROM alue ORDER BY nimi ASC";

                    using (MySqlCommand cmd = new MySqlCommand(selectQuery, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Alueet.Add(new Alue
                                {
                                    AlueId = reader.GetInt32("alue_id"),
                                    Nimi = reader.GetString("nimi")
                                });
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Tietokantavirhe", $"Virhe yhdistettäessä tietokantaan: {ex.Message}", "OK");
            }

            // Ilmoittaa, että Alueet-ominaisuuden arvo on päivitetty, jotta käyttöliittymä voi päivittyä
            OnPropertyChanged(nameof(Alueet));
        }
        private void AluePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && picker.SelectedItem != null)
            {
                ValittuAlue = picker.SelectedItem as Alue;//Asetetaan valitun alueen ValittuAlue-muuttujaan
            }
        }

        private async void cabinListPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cabinListPicker.SelectedIndex != -1)
            {
                string selectedCabin = CabinNames[cabinListPicker.SelectedIndex];

                await PopulateEntryFields(selectedCabin);
            }
        }


        private async Task FetchCabinNamesByAlue(string selectedAlue)
        {
            CabinNames.Clear(); // Clear existing items

            // Use selectedAlue in your query to filter cabins based on the selected alue
            string selectQuery = "SELECT mokkinimi FROM vn.mokki WHERE alue_id = (SELECT alue_id FROM vn.alue WHERE nimi = @selectedAlue)";

            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "VN_password";
            string constring = "SERVER=" + server + ";" + "DATABASE=" + database + ";" +
                "UID=" + username + ";" + "PASSWORD=" + password + ";";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    Console.WriteLine("Connection to the database successful.");

                    using (MySqlCommand cmd = new MySqlCommand(selectQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@selectedAlue", selectedAlue);

                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                // int mokki_id = reader.GetInt32("mokki_id"); 
                                string cabinName = reader.GetString("mokkinimi");
                                CabinNames.Add(cabinName); // Add cabin name to collection
                            }
                        }
                    }
                }
                cabinListPicker.ItemsSource = CabinNames;
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Error", "Error fetching cabin names: " + ex.Message, "OK");
                Console.WriteLine("Error fetching cabin names: " + ex.Message);
            }
        }








        private async Task PopulateEntryFields(string selectedMokkiName)
        {
            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "VN_password";
            string constring = "SERVER=" + server + ";" + "DATABASE=" + database + ";" +
                "UID=" + username + ";" + "PASSWORD=" + password + ";";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    Console.WriteLine("Connection to the database successful.");

                    string selectQuery = "SELECT mokki_id, alue_id, postinro, mokkinimi, katuosoite, hinta, kuvaus, henkilomaara, varustelu FROM mokki WHERE mokkinimi = @mokkiName";

                    using (MySqlCommand cmd = new MySqlCommand(selectQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@mokkiName", selectedMokkiName);

                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Populate entry fields with fetched data

                                postiNro.Text = reader["postinro"].ToString();
                                mokinNimi.Text = reader["mokkinimi"].ToString();
                                katuOsoite.Text = reader["katuosoite"].ToString();
                                hinta.Text = reader["hinta"].ToString();
                                mokinKuvaus.Text = reader["kuvaus"].ToString();
                                henkiloMaara.Text = reader["henkilomaara"].ToString();
                                mokinVarustelu.Text = reader["varustelu"].ToString();
                                int mokkiId = reader.GetInt32("mokki_id");
                                int alueIdFromDatabase = reader.GetInt32("alue_id");
                                int selectedIndex = -1;



                                // Set the selected index of aluePicker
                                if (selectedIndex != -1)
                                {
                                    aluePicker.SelectedIndex = selectedIndex;
                                }
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







                    //NÄYTÄ VIIMEISIN LISÄTTY MÖKKI
                    async Task ShowData(int mokkiId)
                    {
                        string server = "localhost";
                        string database = "vn";
                        string username = "root";
                        string password = "VN_password";
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
                                            string dataString = $"NIMI: {mokkinimi}, Postinro: {postinro},  Katusoite: {katuosoite}, Hinta: {hinta}, Kuvaus: {kuvaus}, Henkilomaara: {henkilomaara}, Varustelu: {varustelu}, ID: {mokkiId}, Alue ID: {alue_id}";

                                            // Display the fetched data in a DisplayAlert dialog
                                            await DisplayAlert("Mökki tallennettu onnistuneesti", dataString, "OK");
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

                
            
            
        //LISÄÄ MOKKI
        public async Task LisaaMokkiAsync()
        {
            int newMokkiId = -1; //-1 KOSKA DATABASE EI VARMASTI SILLOIN SEKOITA MÖKKI ID:TA
            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "VN_password";
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
                    if (ValittuAlue == null)
                    {
                        await DisplayAlert("Error", "Valitse ensin alue", "OK");
                        return;
                    }

                    // Proceed with inserting the entry into the mokki table
                    string insertQuery = @"INSERT INTO mokki 
                    (alue_id, postinro, mokkinimi, katuosoite, hinta, kuvaus, henkilomaara, varustelu) 
                    VALUES 
                    (@alue_id, @postinro, @mokkinimi, @katuosoite, @hinta, @kuvaus, @henkilomaara, @varustelu)";

                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        // Add parameter values
                        cmd.Parameters.AddWithValue("@alue_id", ValittuAlue.AlueId);
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

        // Define a method to handle mokki deletion
        private async Task DeleteSelectedMokki(string selectedMokkiName)
        //SelectedIndex = cabinListPicker.SelectedIndex
        {
            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "VN_password";
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
                            await DisplayAlert("Onnistui", $"{selectedMokkiName} tiedot poistettu onnistuneesti", "OK");


                        }
                        else
                        {
                            await DisplayAlert("Fail", "Mökin poisto epäonnistui", "OK");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Tietokantavirhe", "Mökkiä ei voi poistaa: Mökkiin liittyy aktiivisia varauksia tai palveluita." + ex.Message, "OK");
                Console.WriteLine("Error connecting to the database: " + ex.Message);
            }
        }


        private async void UpdateSelectedMokki(string selectedMokkiName)
        {

            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "VN_password";
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

                    // Get the mokki_id of the selected mokki
                    string selectMokkiIdQuery = "SELECT mokki_id FROM mokki WHERE mokkinimi = @mokkinimi";
                    int mokkiId = -1;

                    using (MySqlCommand mokkiIdCmd = new MySqlCommand(selectMokkiIdQuery, conn))
                    {
                        mokkiIdCmd.Parameters.AddWithValue("@mokkinimi", selectedMokkiName);
                        var result = mokkiIdCmd.ExecuteScalar();
                        if (result != null)
                        {
                            mokkiId = Convert.ToInt32(result);
                        }
                    }

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
                    string updateQuery = @"UPDATE mokki 
                                   SET alue_id = @alue_id, 
                                       postinro = @postinro,
                                       mokkinimi = @mokkinimi, 
                                       katuosoite = @katuosoite, 
                                       hinta = @hinta, 
                                       kuvaus = @kuvaus, 
                                       henkilomaara = @henkilomaara,
                                       varustelu = @varustelu 
                                   WHERE mokkinimi = @mokkinimi";

                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        // Add parameter values
                        cmd.Parameters.AddWithValue("@alue_id", ValittuAlue.AlueId);
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


                            Console.WriteLine("Mokki updated successfully.");

                            // Assuming you have a method to get the mokki_id of the newly added record
                            await ShowData(mokkiId);
                        }
                        else
                        {
                            Console.WriteLine("Failed to update mokki.");
                            await DisplayAlert("FAIL", "Failed to update mokki.", "OK");
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



        private void ClearEntryFields()
        {
            // Set text properties of entry fields to an empty string
            postiNro.Text = "";
            mokinNimi.Text = "";
            katuOsoite.Text = "";
            hinta.Text = "";
            mokinKuvaus.Text = "";
            henkiloMaara.Text = "";
            mokinVarustelu.Text = "";

            // Optionally, clear the selected index of aluePicker and cabinListPicker
            aluePicker.SelectedIndex = -1;
            cabinListPicker.SelectedIndex = -1;
        }


        //////////////////////////bUTTONS //////////////////////////////////////////////////////////////////////////////////////////
        private async void Lisaa_Mokki_Clicked(object sender, EventArgs e)
        {

            await LisaaMokkiAsync();

            ClearEntryFields();

        }



        private async void poistaMokki_Clicked(object sender, EventArgs e)
        {
            if (cabinListPicker.SelectedItem != null)

            {
                // int selectedIndex = cabinListPicker.SelectedIndex;
                string selectedMokkiName = cabinListPicker.SelectedItem.ToString();
                await DeleteSelectedMokki(selectedMokkiName);
            }
            else
            {
                await DisplayAlert("Error", "Please select a mokki to delete.", "OK");
            }
            ClearEntryFields();

        }



        private async void tallennaMokki_Clicked(object sender, EventArgs e)
        {

            if (cabinListPicker.SelectedItem != null)
            {
                string selectedMokkiName = cabinListPicker.SelectedItem.ToString();
                UpdateSelectedMokki(selectedMokkiName);





            }
            else
            {
                DisplayAlert("Error", "Please select a mokki to view.", "OK");
            }

            ClearEntryFields();


        }

        private void clear_button_Clicked(object sender, EventArgs e)
        {
            // Set text properties of entry fields to an empty string
            postiNro.Text = "";
            mokinNimi.Text = "";
            katuOsoite.Text = "";
            hinta.Text = "";
            mokinKuvaus.Text = "";
            henkiloMaara.Text = "";
            mokinVarustelu.Text = "";

            cabinListPicker.SelectedIndex = -1;
            aluePicker.SelectedIndex = -1;

        }
    }


}