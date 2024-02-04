using MySql.Data.MySqlClient;

namespace booking_VillageNewbies
{
    internal class SQLtest
    {
        static void SQL()
        {
            string server = "localhost";
            string database = "hovi";
            string username = "root";
            string password = "Salasana-1212";
           // int port = 3306;
            string constring = "SERVER=" + server + ";" + "DATABASE=" + database + ";" +
            "UID=" + username + ";" + "PASSWORD=" + password + ";";


            try
            {
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    conn.Open();
                    Console.WriteLine("Yhteys tietokantaan onnistui.");
                    string insertQuery = "INSERT INTO henkilo (htun, enimi) VALUES (@htun, @enimi);";

                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@htun", "0210");
                        cmd.Parameters.AddWithValue("@enimi", "Uniikki");

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            Console.WriteLine("Henkilön nimi lisätty onnistuneesti.");
                        }
                        else
                        {
                            Console.WriteLine("Henkilön nimen lisääminen epäonnistui.");
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Virhe yhdistettäessä tietokantaan: " + ex.Message);
            }
        }
    }
    
}
