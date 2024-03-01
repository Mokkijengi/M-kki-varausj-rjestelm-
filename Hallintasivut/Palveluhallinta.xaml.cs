using Microsoft.Maui.Controls;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;

namespace booking_VillageNewbies
{
    public partial class Palveluhallinta : ContentPage
    {
        //Collection pitää kirjaa palveluiden nimistä
        public ObservableCollection<string> PalveluNimet { get; set; } = new ObservableCollection<string>();

        public Palveluhallinta()
        {
            InitializeComponent();
            palveluPicker.ItemsSource = PalveluNimet;//Laitetaan palveluiden nimet pickeriin
            HaePalveluNimet();//Metodi hakee nimet tietokannasta
            BindingContext = this;//Binding sitoo tiedot XAML näkymään
        }


        private async void LisaaPalvelu_Clicked(object sender, EventArgs e)
        {
            await LisaaPalveluAsync();//Metodi lisää uuden palvelun
        }

        public async Task LisaaPalveluAsync()
        {
            string constring = $"SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=Salasana-1212;";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();

                    //SQL-lause uuden tiedon lisäämiseen
                    string insertQuery = @"INSERT INTO palvelu (alue_id, nimi, kuvaus, hinta, alv) VALUES (@alue_id, @nimi, @kuvaus, @hinta, @alv); SELECT LAST_INSERT_ID();";
                    //MySQLCommand-luokkaa käytetään lauseen suorittamiseen
                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        //Parametrit SQL`-lauseeseen jotka poimittu käyttöliittymästä
                        cmd.Parameters.AddWithValue("@alue_id", alueID.Text);
                        cmd.Parameters.AddWithValue("@nimi", palvelunNimi.Text);
                        cmd.Parameters.AddWithValue("@kuvaus", palvelunKuvaus.Text);
                        cmd.Parameters.AddWithValue("@hinta", double.TryParse(hinta.Text, out double hintaValue) ? hintaValue : 0);
                        cmd.Parameters.AddWithValue("@alv", double.TryParse(alv.Text, out double alvValue) ? alvValue : 0);
                        
                        //Suoritetaan SQL-lause ja palautetaan viimeisenä lisätyn rivin ID
                        object result = await cmd.ExecuteScalarAsync();
                        if (result != null)
                        {
                            int newPalveluId = Convert.ToInt32(result);//Muunnetaan kokonaisluvuksi ja otetaan uusi ID talteen
                            await NaytaPalvelunTiedot(newPalveluId); // Näyttää lisätyn palvelun tiedot
                            await HaePalveluNimet(); // Päivitetään palveluiden nimet listaan
                        }
                        else
                        {
                            //Jos lisäys ei onnistu annetaan herja
                            await DisplayAlert("Virhe", "Palvelun lisääminen epäonnistui.", "OK");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {   
                //Jos tietokantayhteydessä tapahtuu virhe kerrotaan myös siitä
                await DisplayAlert("Virhe", $"Tietokantaan yhdistäminen epäonnistui: {ex.Message}", "OK");
            }
        }


        //Palvelun poistamiseen tapahtuman käsittelijä
        private async void PoistaPalvelu_Clicked(object sender, EventArgs e)
        {
            if (palveluPicker.SelectedItem != null)//Katsotaan että on valittu kohde joka halutaan poistaa
            {
                string valittuPalveluNimi = palveluPicker.SelectedItem.ToString();//Valitun palvelun nimellä kutsutaan poistometodia
                await PoistaValittuPalvelu(valittuPalveluNimi);
                HaePalveluNimet(); // Päivitetään palveluiden nimet poiston jälkeen
            }
            else
            {
                //Jos ei ole valittu poistettavaa kohdetta, huomautetaan siitä
                await DisplayAlert("Virhe", "Valitse ensin poistettava palvelu.", "OK");
            }
        }

        //Metodi palvelun poistamiseen
        public async Task PoistaValittuPalvelu(string palvelunNimi)
        {
            string constring = $"SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=Salasana-1212;";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();

                    //SQL-lause joka tekee poiston nimen perusteella
                    string deleteQuery = @"DELETE FROM palvelu WHERE nimi = @nimi";

                    using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                    {
                        //Poistettavan palvelun nimi asetetaan parametriksi
                        cmd.Parameters.AddWithValue("@nimi", palvelunNimi);
                        
                        //Suoritetaan poisto
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            //Onnistuneesta poistosta annetaan ilmoitus
                            await DisplayAlert("Onnistui", $"Palvelu '{palvelunNimi}' poistettu onnistuneesti.", "OK");
                            HaePalveluNimet(); // Päivitetään palveluiden nimet poiston jälkeen
                        }
                        else
                        {
                            //Jos poistettavaa palvelua ei löydy, annetaan herja
                            await DisplayAlert("Virhe", $"Palvelua '{palvelunNimi}' ei löytynyt.", "OK");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                //Jos tietokantayhteydessä tapahtuu virhe kerrotaan myös siitä
                await DisplayAlert("Tietokantavirhe", $"Virhe yhdistettäessä tietokantaan: {ex.Message}", "OK");
            }
        }

        //Metodi palveluiden nimien hakemiseen
        private async Task HaePalveluNimet()
        {
            string constring = $"SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=Salasana-1212;";
            PalveluNimet.Clear(); // Tyhjennä lista ennen uuden datan hakemista

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();

                    //Kun kanta on saatu auki tehdään SQL-kysely joka hakee kaikki nimet aakosjärjestyksessä
                    string selectQuery = @"SELECT nimi FROM palvelu ORDER BY nimi ASC";

                    using (MySqlCommand cmd = new MySqlCommand(selectQuery, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())//Suoritetaan kysely
                        {
                            while (await reader.ReadAsync())//Käydään läpi tulokset
                            {
                                string palveluNimi = reader["nimi"].ToString();//Haetaan kenttien arvot ja muutetaan ne stringeiksi
                                PalveluNimet.Add(palveluNimi);//Lisätään palvelun nimi kokoelmaan
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Tietokantavirhe", $"Virhe yhdistettäessä tietokantaan: {ex.Message}", "OK");
            }

            // Ilmoittaa, että PalveluNimet-ominaisuuden arvo on päivitetty, jotta käyttöliittymä voi päivittyä
            OnPropertyChanged(nameof(PalveluNimet));
        }

        public async Task NaytaPalvelunTiedot(int palveluId)
        {
            string constring = $"SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=Salasana-1212;";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();

                    //Taas kun yhteys on luotu tehdään SQL-kysely, joka hakee kaikki tiedot palvelu-taulusta, jossa palvelu_id vastaa annettua arvoa
                    string selectQuery = "SELECT * FROM palvelu WHERE palvelu_id = @palveluId";

                    using (MySqlCommand cmd = new MySqlCommand(selectQuery, conn))
                    {
                        // Asetetaan palveluId-parametrin arvoksi annettu palveluId
                        cmd.Parameters.AddWithValue("@palveluId", palveluId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Haetaan ja tallennetaan jokaisen kentän arvo
                                string alue_id = reader["alue_id"].ToString();
                                string nimi = reader["nimi"].ToString();
                                string kuvaus = reader["kuvaus"].ToString();
                                string hinta = reader["hinta"].ToString();
                                string alv = reader["alv"].ToString();

                                string dataString = $"Palvelu ID: {palveluId}, Alue ID: {alue_id}, Nimi: {nimi}, Kuvaus: {kuvaus}, Hinta: {hinta}, ALV: {alv}";
                                await DisplayAlert("Lisätyn Palvelun Tiedot", dataString, "OK");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {   
                // Muodostetaan näytettävä tietojono lisätyn palvelun tiedoilla
                await DisplayAlert("Virhe", "Virhe yhdistettäessä tietokantaan: " + ex.Message, "OK");
            }
        }
    }
}
