using System;

public class Mokki//Jokaisessa classissa parametrit suoraan ER-kaaviosta. Lisäksi osaan alustettu
                  //listoija joita tullaan mahdollisesti tarvitsemaan.
{
    public int MokkiId { get; set; }
    public int AlueId { get; set; }
    public int Postinro { get; set; }

    public string MokkiNimi { get; set; }
    public string Katuosoite { get; set; }
    
    public double Hinta { get; set; }

    public string Kuvaus {  get; set; }
    public int Henkilomaara { get; set; }

    public string Varustelu {  get; set; }

    public Mokki(int mokkiId, int alueId, int postinro, string mokkiNimi, string katuosoite,
    double hinta, string kuvaus, int henkilomaara, string varustelu)
    {
        MokkiId = mokkiId;
        AlueId = alueId;
        Postinro = postinro;
        MokkiNimi = mokkiNimi;
        Katuosoite = katuosoite;
        Hinta = hinta;
        Kuvaus = kuvaus;
        Henkilomaara = henkilomaara;
        Varustelu = varustelu;
    }
    public Mokki()
    {
    }
}


