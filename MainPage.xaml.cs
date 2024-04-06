using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;

namespace booking_VillageNewbies
{
    public partial class MainPage : ContentPage
    {

        public class CheckBoxItem
        {
            public bool IsSelected { get; set; }
            public string Label { get; set; }
        }

        //create variable for the cabin list
        public ObservableCollection<string> CabinNames { get; set; }
        public ObservableCollection<string> AlueList { get; set; }
        public ObservableCollection<CheckBoxItem> CheckBoxItems { get; set; }

        public MainPage()
        {
            InitializeComponent();


            // Initialize the ObservableCollection with fake cabin names
            CabinNames = new ObservableCollection<string>();


            // Set the ObservableCollection as the ListView's ItemSource
            cabinListPicker.ItemsSource = CabinNames;

            // Initialize the ObservableCollection for area list
            AlueList = new ObservableCollection<string>();

            cabinListPicker.SelectedIndexChanged += cabinListPicker_SelectedIndexChanged;

            //kun mökki valitaan listalta, avaa pääsivulle sen tiedot. 

            CheckBoxItems = new ObservableCollection<CheckBoxItem>
            {
                new CheckBoxItem { IsSelected = false, Label = "Option 1" },
                new CheckBoxItem { IsSelected = false, Label = "Option 2" },
                new CheckBoxItem { IsSelected = false, Label = "Option 3" }
            };

            checkBoxList.ItemsSource = CheckBoxItems;


            // Add a SelectionChanged event handler for aluePicker
            aluePicker.SelectedIndexChanged += async (sender, args) =>
            {
                if (aluePicker.SelectedIndex != -1)
                {
                    string selectedAlue = AlueList[aluePicker.SelectedIndex];
                    await FetchCabinNamesByAlue(selectedAlue);
                    await FetchServicesByAlue(selectedAlue);
                }
            };



            // Fetch areas from the database and populate AlueList
            FetchAreasFromDatabase();
        }


        //haetaan alueet alue.id mukaan tietokannasta.
        private async void FetchAreasFromDatabase()
        {
            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "Salasana-1212";
            string constring = "SERVER=" + server + ";" + "DATABASE=" + database + ";" +
                "UID=" + username + ";" + "PASSWORD=" + password + ";";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    Console.WriteLine("Connection to the database successful.");

                    string selectQuery = "SELECT nimi FROM vn.alue";

                    using (MySqlCommand cmd = new MySqlCommand(selectQuery, conn))
                    {
                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string alueName = reader.GetString("nimi");
                                AlueList.Add(alueName); // Add area name to collection
                            }
                        }
                    }
                }

                // Set AlueList as the ItemsSource for aluePicker
                aluePicker.ItemsSource = AlueList;
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Error", "Error connecting to the database: " + ex.Message, "OK");
                Console.WriteLine("Error connecting to the database: " + ex.Message);
            }
        }


        // haetaan mökkien nimet valitulta alueelta
        private async Task FetchCabinNamesByAlue(string selectedAlue)
        {
            CabinNames.Clear(); // Clear existing items

            // Use selectedAlue in your query to filter cabins based on the selected alue
            string selectQuery = "SELECT mokkinimi FROM vn.mokki WHERE alue_id = (SELECT alue_id FROM vn.alue WHERE nimi = @selectedAlue)";

            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "Salasana-1212";
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
                                string cabinName = reader.GetString("mokkinimi");
                                CabinNames.Add(cabinName); // Add cabin name to collection
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Error", "Error fetching cabin names: " + ex.Message, "OK");
                Console.WriteLine("Error fetching cabin names: " + ex.Message);
            }
        }

        private async void cabinListPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cabinListPicker.SelectedIndex != -1)
            {
                string selectedCabin = CabinNames[cabinListPicker.SelectedIndex];
                await FetchCabinDetails(selectedCabin);
            }
        }

        //haetaan valitun mökin tiedot tietokannasta
        private async Task FetchCabinDetails(string selectedCabin)
        {
            string selectQuery = "SELECT mokkinimi, alue.nimi AS alue, katuosoite, kuvaus, hinta, henkilomaara, varustelu " +
                         "FROM vn.mokki " +
                         "JOIN vn.alue ON mokki.alue_id = alue.alue_id " +
                         "WHERE mokkinimi = @selectedCabin";

            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "Salasana-1212";
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
                        cmd.Parameters.AddWithValue("@selectedCabin", selectedCabin);

                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                string cabinName = reader.GetString("mokkinimi");
                                string area = reader.GetString("alue");
                                string address = reader.GetString("katuosoite");
                                string description = reader.GetString("kuvaus");
                                double price = reader.GetDouble("hinta");
                                int capacity = reader.GetInt32("henkilomaara");
                                string amenities = reader.GetString("varustelu");

                                // Update labels with fetched cabin details
                                cabinNameLabel.Text = "Mökin nimi: " + cabinName;
                                areaLabel.Text = "Alue: " + area;
                                addressLabel.Text = "Osoite: " + address;
                                descriptionLabel.Text = "Mökin kuvaus: " + description;
                                priceLabel.Text = "Hinta: " + price + "/viikko";
                                capacityLabel.Text = "Henkilömäärä: " + capacity;
                                amenitiesLabel.Text = "Varustelu: " + amenities;
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Error", "Error fetching cabin details: " + ex.Message, "OK");
                Console.WriteLine("Error fetching cabin details: " + ex.Message);
            }
        }


        // haetaan alueen tajoamat palvelut.
        private async Task FetchServicesByAlue(string selectedAlue)
        {
            string selectQuery = "SELECT palvelu_id, nimi FROM vn.palvelu WHERE alue_id = (SELECT alue_id FROM vn.alue WHERE nimi = @selectedAlue)";

            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "Salasana-1212";
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
                            CheckBoxItems.Clear(); // Clear existing items

                            while (await reader.ReadAsync())
                            {
                                int palveluId = reader.GetInt32("palvelu_id");
                                string palveluNimi = reader.GetString("nimi");

                                // Add service to CheckBoxItems collection
                                CheckBoxItems.Add(new CheckBoxItem { IsSelected = false, Label = palveluNimi });
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Error", "Error fetching services: " + ex.Message, "OK");
                Console.WriteLine("Error fetching services: " + ex.Message);
            }
        }


        //viedään valitut tiedot varausprosessiin
        private async void JatkaVaraukseenClicked(object sender, EventArgs e)
        {
            if (alkuPvm.Date < loppuPvm.Date)
            {
                if (aluePicker.SelectedIndex != -1 && cabinListPicker.SelectedIndex != -1)
                {
                    DateTime alkuPvmDate = alkuPvm.Date;
                    DateTime loppuPvmDate = loppuPvm.Date;

                    string selectedAlue = AlueList[aluePicker.SelectedIndex];
                    string selectedMokki = CabinNames[cabinListPicker.SelectedIndex];

                    //pass checkbox items to nxt page
                    List<string> selectedServices = new List<string>();
                    foreach (CheckBoxItem item in CheckBoxItems)
                    {
                        if (item.IsSelected)
                        {
                            selectedServices.Add(item.Label);
                        }
                    }
                    string selectedLisapalvelut = string.Join(",", selectedServices);

                    await Navigation.PushAsync(new Varausprosessi(selectedAlue, selectedMokki, alkuPvmDate, loppuPvmDate, selectedLisapalvelut));
                }
                else
                {
                    await DisplayAlert("Virhe", "Valitse alue ja mökki ensin", "OK");
                }
            }
            else
            {
                await DisplayAlert("Virhe", "Loppupäivämäärän tulee olla alkupäivämäärän jälkeen", "OK");
            }
        }


    }
}