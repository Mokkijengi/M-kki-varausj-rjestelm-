using System;

public class VarauksenPalvelut //Jokaisessa classissa parametrit suoraan ER-kaaviosta. Lisäksi osaan alustettu
                               //listoija joita tullaan mahdollisesti tarvitsemaan.
{
    public int VarausId { get; set; }
    public int PalveluId { get; set; }
    public int Lkm { get; set; }


    public VarauksenPalvelut()
    {
    }

    public VarauksenPalvelut(int varausId, int palveluId, int lkm)
    {
        VarausId = varausId;
        PalveluId = palveluId;
        Lkm = lkm;
    }
}
