using System;

public class Varaus
{
    public int VarausId { get; set; }
    public int AsiakasId { get; set; }
    public int MokkiId { get; set; }
    public DateTime VarattuPvm { get; set; }
    public DateTime VahvistusPvm { get; set; }
    public DateTime VarattuAlkupvm { get; set; }
    public DateTime VarattuLoppupvm { get; set; }

    // Navigointia varten alla olevat kolme viittausta
    public Asiakas Asiakas { get; set; }
    public Mokki Mokki { get; set; }
    public List<VarauksenPalvelut> VarauksenPalvelut { get; set; }


    public Varaus()
    {
        VarauksenPalvelut = new List<VarauksenPalvelut>();
    }

    public Varaus(int varausId, int asiakasId, int mokkiId, DateTime varattuPvm,
                  DateTime vahvistusPvm, DateTime varattuAlkupvm, DateTime varattuLoppupvm)
    {
        VarausId = varausId;
        AsiakasId = asiakasId;
        MokkiId = mokkiId;
        VarattuPvm = varattuPvm;
        VahvistusPvm = vahvistusPvm;
        VarattuAlkupvm = varattuAlkupvm;
        VarattuLoppupvm = varattuLoppupvm;
        VarauksenPalvelut = new List<VarauksenPalvelut>();
    }
}
