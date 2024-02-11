using System;

public class Posti//Jokaisessa classissa parametrit suoraan ER-kaaviosta. Lisäksi osaan alustettu
                  //listoija joita tullaan mahdollisesti tarvitsemaan.
{
    public int Postinro { get; set; }
    public string Toimipaikka { get; set; }

    public Posti()
    {
    }
    public Posti(int postinro, string toimipaikka)
    {
        Postinro = postinro;
        Toimipaikka = toimipaikka;
    }
}
