using System;

public class Posti
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
