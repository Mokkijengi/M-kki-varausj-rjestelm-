using System;

public class Lasku//Jokaisessa classissa parametrit suoraan ER-kaaviosta. Lisäksi osaan alustettu
                  //listoija joita tullaan mahdollisesti tarvitsemaan.
{
    public int LaskuId { get; set; }
    public int AsiakasId { get; set; }
    public int VarausId { get; set; }
    public double Summa { get; set; }
    public double Alv { get; set; }

    // Navigointi Asiakkaaseen ja Varaukseen, tätä vielä selviteltävä kuinka saadaan toimimaan/ tarvitseeko
    public Asiakas Asiakas { get; set; }
    public Varaus Varaus { get; set; }

    public Lasku()
    {
    }

    public Lasku(int laskuId, int asiakasId, double summa, int varausId, double alv)
    {
        LaskuId = laskuId;
        AsiakasId = asiakasId;
        Summa = summa;
        VarausId = varausId;
        Alv = alv;
    }
}
