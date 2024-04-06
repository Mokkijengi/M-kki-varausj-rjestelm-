using Microsoft.Maui.Controls;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace booking_VillageNewbies
{
    public partial class Aluehallinta : ContentPage
    {
        public ObservableCollection<string> AlueNimet { get; set; }

        public Aluehallinta()
        {
            InitializeComponent();
            AlueNimet = new ObservableCollection<string>();
            HaeAlueNimet(); // Alusta alueiden nimien lataus
            BindingContext = this;
        }

        private async Task HaeAlueNimet()//Metodi hakee nimet tietokannasta ja lis‰‰ ne AlueNimet kokoelmaan
        {
            AlueNimet.Clear();
            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "Salasana-1212";
            string constring = $"SERVER={server};DATABASE={database};UID={username};PASSWORD={password};";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();// Avataan tietokanta yhteys
                    string selectQuery = "SELECT nimi FROM alue";//SQL-komento jolla haetaan kaikkien alueiden nimet

                    using (MySqlCommand cmd = new MySqlCommand(selectQuery, conn))//Luodaan SQL Query
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())//Suoritetaan kysely
                        {
                            while (await reader.ReadAsync())//Ja k‰yd‰‰n tulokset l‰pi yksitellen
                            {
                                string alueNimi = reader["nimi"].ToString();
                                AlueNimet.Add(alueNimi);
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Error", "Error connecting to the database: " + ex.Message, "OK");
            }
        }

        public async Task<int> LisaaAlueAsync()
        {
            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "Salasana-1212";
            string constring = $"SERVER={server};DATABASE={database};UID={username};PASSWORD={password};";
            int newAlueId = -1;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    string alueenNimiValue = alueenNimi.Text;

                    string insertQuery = @"INSERT INTO alue (nimi) VALUES (@nimi); SELECT LAST_INSERT_ID();";

                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@nimi", alueenNimiValue);

                        // ExecuteScalarAsync palauttaa lis‰tyn rivin id:n
                        object result = await cmd.ExecuteScalarAsync();
                        if (result != null)
                        {
                            newAlueId = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Error", "Error connecting to the database: " + ex.Message, "OK");
            }

            return newAlueId;
        }


        private async void LisaaAlue_Clicked(object sender, EventArgs e)
        {
            int alueId = await LisaaAlueAsync();
            if (alueId > -1)
            {
                await NaytaAlueenTiedot(alueId);
                await HaeAlueNimet(); // P‰ivit‰ alueiden nimet lis‰yksen j‰lkeen
            }
        }

        public async Task NaytaAlueenTiedot(int alueId)//N‰ytt‰‰ lis‰tyn alueen tiedot
        {
            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "Salasana-1212";
            string constring = $"SERVER={server};DATABASE={database};UID={username};PASSWORD={password};";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    string selectQuery = "SELECT * FROM alue WHERE alue_id = @alueId";

                    using (MySqlCommand cmd = new MySqlCommand(selectQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@alueId", alueId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string nimi = reader["nimi"].ToString();
                                await DisplayAlert("Lis‰tyn Alueen Tiedot", $"Alue ID: {alueId}, Nimi: {nimi}", "OK");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Error", "Error connecting to the database: " + ex.Message, "OK");
            }
        }

        private async void PoistaAlue_Clicked(object sender, EventArgs e)//Napilla suoritetaan "PoistaValittuAlue"
         //Ensin tarkistetaan ett‰ alue on valittu, jos on, poistetaan valittu alue tietokannasta
        {
            if (aluePicker.SelectedItem != null)
            {
                string valittuAlueNimi = aluePicker.SelectedItem.ToString();
                await PoistaValittuAlue(valittuAlueNimi);
                await HaeAlueNimet(); // P‰ivit‰ alueiden nimet poiston j‰lkeen
            }
            else
            {
                await DisplayAlert("Virhe", "Valitse ensin poistettava alue.", "OK");
            }
        }

        public async Task PoistaValittuAlue(string alueenNimi)//Poistetaan valittu alue, k‰ytt‰en sen nime‰
        {
            string server = "localhost";
            string database = "vn";
            string username = "root";
            string password = "VN_password";
            string constring = $"SERVER={server};DATABASE={database};UID={username};PASSWORD={password};";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    string deleteQuery = @"DELETE FROM alue WHERE nimi = @nimi";

                    using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@nimi", alueenNimi);
                        await cmd.ExecuteNonQueryAsync();
                        await DisplayAlert("Onnistui", $"Alue '{alueenNimi}' poistettu onnistuneesti.", "OK");
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Tietokantavirhe", $"Virhe yhdistett‰ess‰ tietokantaan: {ex.Message}", "OK");
            }
        }
    }
}
