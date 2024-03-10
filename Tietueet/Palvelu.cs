using System;

public class Palvelu//Jokaisessa classissa parametrit suoraan ER-kaaviosta. Lisäksi osaan alustettu
                    //listoija joita tullaan mahdollisesti tarvitsemaan.
{
    public int PalveluId { get; set; }
    public string Nimi { get; set; }
    public string Kuvaus { get; set; }
    public decimal Hinta { get; set; }
    public int AlueId { get; set; }
 
    //public int Tyyppi {  get; set; }Tarvitaanko?
    public Alue Alue { get; set; } // Navigointiominaisuus Alue-olioon / Tarvitaanko?

    public List<VarauksenPalvelut> VarauksenPalvelut { get; set; }

    public Palvelu()
    {
        VarauksenPalvelut = new List<VarauksenPalvelut>();
    }

    public Palvelu(int palveluId, string nimi, string kuvaus, decimal hinta, int alueId)
    {
        PalveluId = palveluId;
        Nimi = nimi;
        Kuvaus = kuvaus;
        Hinta = hinta;
        AlueId = alueId;
        VarauksenPalvelut = new List<VarauksenPalvelut>();
    }
}
