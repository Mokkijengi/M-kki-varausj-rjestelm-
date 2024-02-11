using System;

public class Alue//Jokaisessa classissa parametrit suoraan ER-kaaviosta. Lisäksi osaan alustettu
                 //listoija joita tullaan mahdollisesti tarvitsemaan.
{

    public int AlueId { get; set; }
    public string Nimi { get; set; }
    public List<Mokki> AlueenMokit { get; set; }

    public Alue(int alueId, string nimi)
    {
        AlueId = alueId;
        Nimi = nimi;
        AlueenMokit = new List<Mokki>();
    }
    public Alue()
    {
     AlueenMokit = new List<Mokki>();
    }
}

