using System.Diagnostics;
using System.IO;
using Windows.System;

namespace booking_VillageNewbies.Hallintasivut
{
    public partial class Varaushallinta : ContentPage
    {
        public Varaushallinta()
        {
            InitializeComponent();
        }

        private void TarkasteleLaskuja_Clicked(object sender, EventArgs e)
        {
            // billingFolderPath on jokaisella lokaalisti oma, joten vaihda sen tilalle omalla koneella oleva polkusi
            string billingFolderPath = @"C:\Users\kalle\source\repos\Mokkijengi\M-kki-varausj-rjestelm-\Billing";

            if (!Directory.Exists(billingFolderPath))
            {
                Directory.CreateDirectory(billingFolderPath);
            }

            // Avaa kohdekansio käyttäjän koneella
            Process.Start("explorer.exe", billingFolderPath);
        }
    }
}
