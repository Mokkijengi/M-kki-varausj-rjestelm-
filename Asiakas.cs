using System;
using System.Threading.Tasks;

public class Asiakas
{
    public int AsiakasId { get; set; }
    public int Postinro { get; set; }
    public string Etunimi { get; set; }
    public string Sukunimi { get; set; }
    public string LahiOsoite { get; set; }
    public string Email { get; set; }
    public string Puhelinnumero { get; set; }

    public List<Varaus> Varaus { get; set; }

    public List<Lasku> Lasku { get; set; }

   
    public Asiakas()
    {
        Varaus = new List<Varaus>();
        Lasku = new List<Lasku>();
    }

    public Asiakas(int asiakasId, int postinro, string etunimi, string sukunimi,
                   string lahiOsoite, string email, string puhelinnumero)
    {
        AsiakasId = asiakasId;
        Postinro = postinro;
        Etunimi = etunimi;
        Sukunimi = sukunimi;
        LahiOsoite = lahiOsoite;
        Email = email;
        Puhelinnumero = puhelinnumero;
        Varaus = new List<Varaus>();
        Lasku = new List<Lasku>();
    }
}
