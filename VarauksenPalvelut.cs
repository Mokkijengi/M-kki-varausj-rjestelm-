using System;

public class VarauksenPalvelut
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
