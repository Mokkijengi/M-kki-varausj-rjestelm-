using Microsoft.Maui.Controls;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Data;

namespace booking_VillageNewbies
{
    public partial class Palveluhallinta : ContentPage
    {
        //Collection pitää kirjaa palveluiden nimistä, alueista ja tiedoista, ne päivittyvät UI:hin kun niiden sisältö muuttuu
        public ObservableCollection<string> PalveluNimet { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<Alue> Alueet { get; set; } = new ObservableCollection<Alue>();

        public ObservableCollection<PalveluTieto> PalveluTiedot { get; set; } = new ObservableCollection<PalveluTieto>();

        //Muuttuja valitulle alueelle
        public Alue ValittuAlue { get; set; }

        //Muuttuja säilyttää valitun palvelun ID:n mm. tietojen hakemista ja päivittämistä varten
        private string valittuPalveluId;

        //Luokkaan määritellään palvelun tiedot, mahdollistaa niiden esittelemisen UIssä
        public class PalveluTieto
        {
            public int PalveluId { get; set; }
            public string PalveluNimi { get; set; }
            public string AlueNimi { get; set; }
            public int AlueId { get; set; }
            public string NäyttöNimi => $"{PalveluNimi}, {AlueNimi}";
        }




        public Palveluhallinta()
        {
            InitializeComponent();
            BindingContext = this;//Binding sitoo tiedot XAML näkymään

            Task.Run(async () => await InitializeDataAsync());

            palveluPicker.ItemsSource = PalveluNimet;//Laitetaan palveluiden nimet pickeriin

        }






        private async Task HaePalveluJaAlueTiedot()
        {
            string constring = "SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=Salasana-1212;";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    string query = @"SELECT p.palvelu_id, p.nimi as palvelu_nimi, a.nimi as alue_nimi, p.alue_id FROM palvelu p JOIN alue a ON p.alue_id = a.alue_id ORDER BY p.nimi, a.nimi";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                PalveluTiedot.Add(new PalveluTieto
                                {
                                    PalveluId = reader.GetInt32("palvelu_id"),
                                    PalveluNimi = reader.GetString("palvelu_nimi"),
                                    AlueNimi = reader.GetString("alue_nimi"),
                                    AlueId = reader.GetInt32("alue_id")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", "Virhe haettaessa tietoja: " + ex.Message, "OK");
            }
        }






        //Metodi joka tarkistaa annetuilla ehdoilla ettei duplikaatti palveluja päädy samalle alueelle.
        private async Task<bool> OnkoPalveluOlemassa(string nimi, int alueId)
        {
            string constring = "SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=Salasana-1212;";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    string query = @"SELECT COUNT(1) FROM palvelu WHERE nimi = @nimi AND alue_id = @alueId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nimi", nimi);
                        cmd.Parameters.AddWithValue("@alueId", alueId);
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        return count > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Tietokantavirhe", $"Tietokantavirhe: {ex.Message}", "OK");
                return false;
            }
        }






        //Tässä haetaan palvelun tiedot kannasta sekä sijoitellaan ne UI:stä löytyviin kenttiin, muokkausta/poistoa varten
        private async void PalveluPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            var selectedItem = picker.SelectedItem as PalveluTieto;
            if (selectedItem != null)
            {
                valittuPalveluId = selectedItem.PalveluId.ToString();
                palvelunNimiEntry.Text = selectedItem.PalveluNimi;
                aluePicker.SelectedItem = Alueet.FirstOrDefault(a => a.AlueId == selectedItem.AlueId);

                // Hakee lisätiedot valitusta palvelusta
                await HaeValitunPalvelunLisatiedot(selectedItem.PalveluId);
            }
        }

        public async Task HaeValitunPalvelunLisatiedot(int palveluId)
        {
            string constring = "SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=Salasana-1212;";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    string query = "SELECT hinta, kuvaus, alv FROM palvelu WHERE palvelu_id = @palveluId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@palveluId", palveluId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                hintaEntry.Text = reader["hinta"].ToString();
                                palvelunKuvausEntry.Text = reader["kuvaus"].ToString();
                                alvEntry.Text = reader["alv"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", "Virhe haettaessa palvelun lisätietoja: " + ex.Message, "OK");
            }
        }







        private async void TallennaMuutokset_Clicked(object sender, EventArgs e)
        {
            // Tarkistetaan, että palvelun ID on kelvollinen kokonaisluku.
            if (!string.IsNullOrEmpty(valittuPalveluId) && int.TryParse(valittuPalveluId, out int parsedId))
            {
                // Kutsutaan muokkauslogiikkaa.
                await MuokkaaValittuaPalvelua();
            }
            else
            {
                // Jos valittuPalveluId ei ole määritetty tai se ei ole kelvollinen, kutsutaan lisäyslogiikkaa.
                await LisaaPalveluAsync();
            }

            // Päivitetään palveluiden ja alueiden tiedot muutosten jälkeen.
            await HaePalveluJaAlueTiedot();

            // Päivitetään picker jotta muuttuneet tiedot näkyvät samantien
            palveluPicker.ItemsSource = null;
            palveluPicker.ItemsSource = PalveluTiedot;
        }







        private async Task MuokkaaValittuaPalvelua()
        {
            string constring = $"SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=Salasana-1212;";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    string updateQuery = @"UPDATE palvelu SET alue_id = @alue_id, nimi = @nimi, kuvaus = @kuvaus, hinta = @hinta, alv = @alv WHERE palvelu_id = @palveluId";

                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        // Täytä parametrit lomakkeen kentistä
                        cmd.Parameters.AddWithValue("@alue_id", ValittuAlue.AlueId);
                        cmd.Parameters.AddWithValue("@nimi", palvelunNimiEntry.Text);
                        cmd.Parameters.AddWithValue("@kuvaus", palvelunKuvausEntry.Text);
                        cmd.Parameters.AddWithValue("@hinta", double.TryParse(hintaEntry.Text, out double hintaValue) ? hintaValue : 0);
                        cmd.Parameters.AddWithValue("@alv", double.TryParse(alvEntry.Text, out double alvValue) ? alvValue : 0);
                        cmd.Parameters.AddWithValue("@palveluId", int.Parse(valittuPalveluId));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            await DisplayAlert("Onnistui", "Palvelun tiedot päivitetty onnistuneesti.", "OK");
                            await HaePalveluJaAlueTiedot(); // Päivitetään palvelulista
                            palveluPicker.ItemsSource = null;
                            palveluPicker.ItemsSource = PalveluTiedot;
                        }
                        else
                        {
                            await DisplayAlert("Virhe", "Muutosten tallentaminen epäonnistui.", "OK");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Tietokantavirhe", $"Virhe yhdistettäessä tietokantaan: {ex.Message}", "OK");
            }
        }






        public async Task InitializeDataAsync()
        {
            await HaeAlueNimet(); // Haetaan alueiden nimet
            await HaePalveluJaAlueTiedot(); // Haetaan palveluiden ja alueiden tiedot

            //asetetaan Pickeriin tiedot
            palveluPicker.ItemsSource = PalveluTiedot;
        }



        private async void LisaaPalvelu_Clicked(object sender, EventArgs e)
        {
            string nimi = palvelunNimiEntry.Text;
            int alueId = ValittuAlue?.AlueId ?? 0;

            // Tarkistetaan onko samanlainen palvelu jo olemassa ennen kuin yritetään lisätä uusi
            bool olemassa = await OnkoPalveluOlemassa(nimi, alueId);
            if (olemassa)
            {
                await DisplayAlert("Virhe", "Saman niminen palvelu on jo olemassa valitulla alueella.", "OK");
                return;
            }

            // Jos ei ole olemassa, jatketaan uuden palvelun lisäämistä
            await LisaaPalveluAsync();
        }




        public async Task LisaaPalveluAsync()
        {
            string constring = $"SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=Salasana-1212;";
            if (ValittuAlue == null)
            {
                await DisplayAlert("Virhe", "Aluetta ei ole valittu.", "OK");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    string insertQuery = @"INSERT INTO palvelu (alue_id, nimi, kuvaus, hinta, alv) VALUES (@alue_id, @nimi, @kuvaus, @hinta, @alv); SELECT LAST_INSERT_ID();";

                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@alue_id", ValittuAlue.AlueId);
                        cmd.Parameters.AddWithValue("@nimi", palvelunNimiEntry.Text);
                        cmd.Parameters.AddWithValue("@kuvaus", palvelunKuvausEntry.Text);
                        cmd.Parameters.AddWithValue("@hinta", double.TryParse(hintaEntry.Text, out double hintaValue) ? hintaValue : 0);
                        cmd.Parameters.AddWithValue("@alv", double.TryParse(alvEntry.Text, out double alvValue) ? alvValue : 0);

                        object result = await cmd.ExecuteScalarAsync();
                        if (result != null)
                        {
                            int newPalveluId = Convert.ToInt32(result);

                            // Päivitetään tässä kokoelmaa ja UI-komponenttia
                            var uusiPalvelu = new PalveluTieto
                            {
                                PalveluId = newPalveluId,
                                PalveluNimi = palvelunNimiEntry.Text,
                                AlueNimi = ValittuAlue.Nimi,
                                AlueId = ValittuAlue.AlueId
                            };
                            PalveluTiedot.Add(uusiPalvelu);

                            // Varmistetaan että UI päivittyy
                            palveluPicker.ItemsSource = null;
                            palveluPicker.ItemsSource = PalveluTiedot;

                            await NaytaPalvelunTiedot(newPalveluId);
                        }
                        else
                        {
                            await DisplayAlert("Virhe", "Palvelun lisääminen epäonnistui.", "OK");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Virhe", $"Tietokantaan yhdistäminen epäonnistui: {ex.Message}", "OK");
            }
        }

        //Palvelun poistamiseen tapahtuman käsittelijä
        private async void PoistaPalvelu_Clicked(object sender, EventArgs e)
        {
            var valittuItem = palveluPicker.SelectedItem as PalveluTieto;
            if (valittuItem != null)
            {
                await PoistaValittuPalvelu(valittuItem.PalveluId);

                // Poista palvelu ObservableCollectionista, mikä päivittää UI:n automaattisesti
                PalveluTiedot.Remove(valittuItem);

                // Varmistetaan että UI päivittyy
                palveluPicker.ItemsSource = null;
                palveluPicker.ItemsSource = PalveluTiedot;
            }
            else
            {
                await DisplayAlert("Virhe", "Valitse ensin poistettava palvelu.", "OK");
            }
        }







        //Metodi palvelun poistamiseen
        public async Task PoistaValittuPalvelu(int palveluId)
        {
            string constring = "SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=Salasana-1212;";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();

                    // Käytetään palvelu_id:tä poiston tunnisteena
                    string deleteQuery = @"DELETE FROM palvelu WHERE palvelu_id = @palveluId";

                    using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                    {
                        // Asetetaan palveluId-parametriksi
                        cmd.Parameters.AddWithValue("@palveluId", palveluId);

                        // Suoritetaan poisto
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            // Onnistuneesta poistosta annetaan ilmoitus
                            await DisplayAlert("Onnistui", "Palvelu poistettu onnistuneesti.", "OK");
                        }
                        else
                        {
                            // Jos poistettavaa palvelua ei löydy, annetaan herja
                            await DisplayAlert("Virhe", "Palvelua ei löytynyt.", "OK");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Jos tietokantayhteydessä tapahtuu virhe, kerrotaan myös siitä
                await DisplayAlert("Tietokantavirhe", "Virhe yhdistettäessä tietokantaan: " + ex.Message, "OK");
            }
        }






        private async Task HaeAlueNimet()
        {
            string constring = $"SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=Salasana-1212;";
            Alueet.Clear(); // Tyhjennetään lista ennen uuden datan hakemista

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
            OnPropertyChanged(nameof(Alueet));
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
                // Näytetään lisätyt tiedot käyttäjälle
                await DisplayAlert("Virhe", "Virhe yhdistettäessä tietokantaan: " + ex.Message, "OK");
            }
        }
    }
}
