using Microsoft.Maui.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;

namespace booking_VillageNewbies
{
    public partial class Asiakashallinta : ContentPage
    {
        //Collection pit‰‰ sis‰ll‰‰n listan asiakkaita
        public ObservableCollection<Asiakas> AsiakasLista { get; set; } = new ObservableCollection<Asiakas>();
        //Valittu asiakas listalta jonka tietoja voidaan k‰sitell‰
        public Asiakas ValittuAsiakas { get; set; }

        //Konstruktori, joka alustaa n‰kym‰n ja hakee asiakkaiden tiedot tietokannasta.
        public Asiakashallinta()
        {
            InitializeComponent();
            BindingContext = this;
            HaeAsiakkaat();
        }

        //Tarkistus joka pit‰‰ huolen siit‰ ettei saman nimisi‰ asiakkaita luoda useampaa
        private async Task<bool> OnkoAsiakasOlemassa(string etunimi, string sukunimi)
        {
            string constring = "SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=Salasana-1212;";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT COUNT(1) 
                        FROM asiakas 
                        WHERE etunimi = @etunimi AND sukunimi = @sukunimi;";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@etunimi", etunimi);
                        cmd.Parameters.AddWithValue("@sukunimi", sukunimi);
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        return count > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Tietokantavirhe", $"Virhe haettaessa tietoja tietokannasta: {ex.Message}", "OK");

                return false;
            }
        }


        // Hakee kaikki asiakkaat tietokannasta ja lis‰‰ ne AsiakasLista-kokoelmaan.
        private async Task HaeAsiakkaat()
        {
            string constring = "SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=Salasana-1212;";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    string query = "SELECT asiakas_id, etunimi, sukunimi, lahiosoite, postinro, email, puhelinnro FROM asiakas;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            var tempList = new ObservableCollection<Asiakas>();
                            while (await reader.ReadAsync())
                            {
                                tempList.Add(new Asiakas
                                {
                                    AsiakasId = reader.IsDBNull(reader.GetOrdinal("asiakas_id")) ? 0 : reader.GetInt32("asiakas_id"),
                                    Etunimi = reader.IsDBNull(reader.GetOrdinal("etunimi")) ? string.Empty : reader.GetString("etunimi"),
                                    Sukunimi = reader.IsDBNull(reader.GetOrdinal("sukunimi")) ? string.Empty : reader.GetString("sukunimi"),
                                    Lahiosoite = reader.IsDBNull(reader.GetOrdinal("lahiosoite")) ? null : reader.GetString("lahiosoite"),
                                    Postinro = reader.IsDBNull(reader.GetOrdinal("postinro")) ? null : reader.GetString("postinro"),
                                    Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString("email"),
                                    Puhelinnro = reader.IsDBNull(reader.GetOrdinal("puhelinnro")) ? null : reader.GetString("puhelinnro"),
                                    AsiakkaanNimi = $"{reader.GetString("etunimi")} {reader.GetString("sukunimi")}"
                                });
                            }
                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                AsiakasLista.Clear();
                                foreach (var item in tempList)
                                {
                                    AsiakasLista.Add(item);
                                }
                            });
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Tietokantavirhe", $"Virhe haettaessa tietoja tietokannasta: {ex.Message}", "OK");
            }
        }





        // K‰sittelee Lis‰‰ asiakas-napin painalluksen ja lis‰‰ uuden asiakkaan tietokantaan.



        private async void LisaaAsiakas_Clicked(object sender, EventArgs e)
        {
            // Regex-s‰‰nnˆt s‰hkˆpostille, postinumerolle ja puhelinnumerolle.
            var emailRegex = new System.Text.RegularExpressions.Regex(@"^\S+@\S+\.\S+$");
            var phoneRegex = new System.Text.RegularExpressions.Regex(@"^\d{10}$");
            var postalCodeRegex = new System.Text.RegularExpressions.Regex(@"^\d{5}$");

            // Tarkista ett‰ kaikki pakolliset kent‰t on t‰ytetty ja ett‰ postinumero on 5 numeron pituinen.
            if (string.IsNullOrWhiteSpace(etuNimi.Text) ||
                string.IsNullOrWhiteSpace(sukuNimi.Text) ||
                string.IsNullOrWhiteSpace(lahiOsoite.Text) ||
                string.IsNullOrWhiteSpace(postinro.Text) ||
                !emailRegex.IsMatch(eMail.Text) || // Tarkistetaan, ett‰ s‰hkˆpostiosoite on oikeassa muodossa.
                !phoneRegex.IsMatch(puhNumero.Text) || // Tarkistetaan, ett‰ puhelinnumero on oikeassa muodossa.
                !postalCodeRegex.IsMatch(postinro.Text)) // Tarkistetaan, ett‰ postinumero on 5 numeron pituinen.
            {
                await DisplayAlert("Virhe", "Tarkista, ett‰ kaikki kent‰t ovat t‰ytetty, ja niiss‰ on tiedot asianmukaisessa muodossa.", "OK");
                return;
            }

            string constring = "SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=Salasana-1212;";

            bool asiakasOnOlemassa = await OnkoAsiakasOlemassa(etuNimi.Text, sukuNimi.Text);
            if (asiakasOnOlemassa)
            {
                await DisplayAlert("Virhe", "Saman niminen asiakas on jo olemassa.", "OK");
                return;
            }
            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    string insertQuery = @"INSERT INTO asiakas (etunimi, sukunimi, lahiosoite, postinro, email, puhelinnro) VALUES (@etunimi, @sukunimi, @lahiosoite, @postinro, @email, @puhelinnro);";

                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@etunimi", etuNimi.Text);
                        cmd.Parameters.AddWithValue("@sukunimi", sukuNimi.Text);
                        cmd.Parameters.AddWithValue("@lahiosoite", lahiOsoite.Text);
                        cmd.Parameters.AddWithValue("@postinro", postinro.Text);
                        cmd.Parameters.AddWithValue("@email", eMail.Text);
                        cmd.Parameters.AddWithValue("@puhelinnro", puhNumero.Text);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            // N‰yt‰ lis‰ttyjen tietojen yhteenveto
                            string addedDataString = $"Etunimi: {etuNimi.Text}, Sukunimi: {sukuNimi.Text}, Osoite: {lahiOsoite.Text}, Postinumero: {postinro.Text}, Email: {eMail.Text}, Puhelinnumero: {puhNumero.Text}";
                            await DisplayAlert("Onnistui", "Asiakas lis‰tty onnistuneesti.\nLis‰tyt tiedot:\n" + addedDataString, "OK");
                            await HaeAsiakkaat();
                        }
                        else
                        {
                            await DisplayAlert("Virhe", "Asiakkaan lis‰‰minen ep‰onnistui.", "OK");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Tietokantavirhe", $"Virhe yhdistett‰ess‰ tietokantaan: {ex.Message}", "OK");
            }
        }









        // K‰sittelee Tallenna muutokset-napin painalluksen ja p‰ivitt‰‰ valitun asiakkaan tiedot tietokantaan.
        private async void TallennaMuutokset_Clicked(object sender, EventArgs e)
        {
            if (ValittuAsiakas == null)
            {
                await DisplayAlert("Valitse asiakas", "Valitse ensin muokattava asiakas.", "OK");
                return;
            }

            string constring = "SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=Salasana-1212;";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    string updateQuery = @"UPDATE asiakas SET etunimi = @etunimi, sukunimi = @sukunimi, lahiosoite = @lahiosoite, email = @email, postinro = @postinro, puhelinnro = @puhelinnro WHERE asiakas_id = @asiakas_id";

                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@asiakas_id", ValittuAsiakas.AsiakasId);
                        cmd.Parameters.AddWithValue("@etunimi", etuNimi.Text);
                        cmd.Parameters.AddWithValue("@sukunimi", sukuNimi.Text);
                        cmd.Parameters.AddWithValue("@lahiosoite", lahiOsoite.Text);
                        cmd.Parameters.AddWithValue("@email", eMail.Text);
                        cmd.Parameters.AddWithValue("@puhelinnro", puhNumero.Text);
                        cmd.Parameters.AddWithValue("@postinro", postinro.Text);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            await DisplayAlert("Onnistui", "Asiakkaan tiedot p‰ivitetty onnistuneesti.", "OK");
                            HaeAsiakkaat(); // P‰ivitt‰‰ asiakaslistan muutosten j‰lkeen
                        }
                        else
                        {
                            await DisplayAlert("Virhe", "Asiakkaan tietojen p‰ivitys ep‰onnistui.", "OK");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Tietokantavirhe", $"Virhe yhdistett‰ess‰ tietokantaan: {ex.Message}", "OK");
            }
        }




        // K‰sittelee asiakasvalinnan muuttumisen picker-komponentissa ja asettaa valitun asiakkaan tiedot k‰yttˆliittym‰n kenttiin.
        private void asiakasPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            var valittuAsiakas = (Asiakas)picker.SelectedItem;

            if (valittuAsiakas != null)
            {
                etuNimi.Text = valittuAsiakas.Etunimi;
                sukuNimi.Text = valittuAsiakas.Sukunimi;
                lahiOsoite.Text = valittuAsiakas.Lahiosoite;
                eMail.Text = valittuAsiakas.Email;
                puhNumero.Text = valittuAsiakas.Puhelinnro;
                postinro.Text = valittuAsiakas.Postinro;
            }
        }





        // K‰sittelee Poista asiakax-napin painalluksen ja poistaa valitun asiakkaan tietokannasta.
        private async void PoistaAsiakas_Clicked(object sender, EventArgs e)
        {
            if (ValittuAsiakas == null)
            {
                await DisplayAlert("Valitse asiakas", "Valitse ensin poistettava asiakas.", "OK");
                return;
            }

            string valittuAsiakasId = ValittuAsiakas.AsiakasId.ToString();

            string constring = "SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=Salasana-1212;";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    await conn.OpenAsync();
                    string deleteQuery = @"DELETE FROM asiakas WHERE asiakas_id = @asiakas_id";

                    using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@asiakas_id", valittuAsiakasId);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            await DisplayAlert("Onnistui", "Asiakas poistettu onnistuneesti.", "OK");
                            AsiakasLista.Remove(ValittuAsiakas); // Poista asiakas myˆs paikallisesta listasta
                        }
                        else
                        {
                            await DisplayAlert("Virhe", "Asiakkaan poistaminen ep‰onnistui.", "OK");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Tietokantavirhe", $"Virhe yhdistett‰ess‰ tietokantaan: {ex.Message}", "OK");
            }
        }

    }




    // Apuluokka asiakastietojen s‰ilytt‰miseen.
    public class Asiakas
    {
        public int AsiakasId { get; set; }
        public string AsiakkaanNimi { get; set; }

        public string Etunimi { get; set; }
        public string Sukunimi { get; set; }
        public string Lahiosoite { get; set; }
        public string Email { get; set; }
        public string Puhelinnro { get; set; }
        public string Postinro { get; set; }

    }


}
