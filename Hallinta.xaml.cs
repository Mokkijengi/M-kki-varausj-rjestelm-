<<<<<<< HEAD
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
=======
using booking_VillageNewbies.Hallintasivut;
using System.Collections.ObjectModel;
>>>>>>> 67e491f3625d5d71676be0bb94f86d41eb42f35b


namespace booking_VillageNewbies
{
<<<<<<< HEAD
    public partial class Hallinta : ContentPage
=======
    public Hallinta()
	{
		InitializeComponent();

  
    }
    private void NameEntry_TextChanged(object sender, TextChangedEventArgs e)
>>>>>>> 67e491f3625d5d71676be0bb94f86d41eb42f35b
    {
        public Hallinta()
        {
            InitializeComponent();
        }

<<<<<<< HEAD
        public async Task ShowData()
        {
            List<string> fetchedData = await FetchDataAsync();

            // Convert the list of strings to a single string for display
            string dataString = string.Join("\n", fetchedData);

            // Display the data in a DisplayAlert dialog
            await DisplayAlert("Fetched Data", dataString, "OK");
        }

        public async Task<List<string>> FetchDataAsync()
        {
            List<string> data = new List<string>();

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

                    string selectQuery = "SELECT nimi FROM alue;";

                    using (MySqlCommand cmd = new MySqlCommand(selectQuery, conn))
                    {
                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string nimi = reader.GetString("nimi");
                                data.Add(nimi);
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

            return data;
        }

        public async Task LisaaMokkiAsync()  //Vie databaseen
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
                    conn.Open();
                    Console.WriteLine("Connection to the database successful.");
                    string insertQuery = "INSERT INTO alue (nimi) VALUES (@nimi);";

                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@nimi", "Uniikki");

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            Console.WriteLine("Name inserted successfully.");

                            await DisplayAlert("Success", "Name inserted successfully.", "OK");

                        }
                        else
                        {
                            Console.WriteLine("Failed to insert name.");
                            await DisplayAlert("FAIL", "No names inserted.", "OK");

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
        private async void lisaa_mokki_clicked(object sender, EventArgs e)
        {
            
            LisaaMokkiAsync(); // Pass 'this' as the instance of Page
        }


        private async void tuo_tietoja_clicked(object sender, EventArgs e)
        {
            await FetchDataAsync(); // Asynchronous call to fetch data
            await ShowData(); // Asynchronous call to show data
        }
    }
}
=======

    private async void Mokit_Clicked(object sender, EventArgs e)
    {
        Mokkihallinta mokkihallinta = new Mokkihallinta();

        await Navigation.PushAsync(mokkihallinta);
    }

    private async void Palvelut_Clicked(object sender, EventArgs e)
    {
        Palveluhallinta palveluhallinta = new Palveluhallinta();

        await Navigation.PushAsync(palveluhallinta);
    }

    private async void Alueet_Clicked(object sender, EventArgs e)
    {
        Aluehallinta aluehallinta = new Aluehallinta();

        await Navigation.PushAsync(aluehallinta);
    }

    private async void Asiakkaat_Clicked(object sender, EventArgs e)
    {
        Asiakashallinta asiakashallinta = new Asiakashallinta();

        await Navigation.PushAsync(asiakashallinta);
    }

    private async void Varaukset_Clicked(object sender, EventArgs e)
    {
        Varaushallinta varaushallinta = new Varaushallinta();

        await Navigation.PushAsync(varaushallinta);
    }
}
>>>>>>> 67e491f3625d5d71676be0bb94f86d41eb42f35b
