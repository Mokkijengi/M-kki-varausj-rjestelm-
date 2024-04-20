using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace booking_VillageNewbies.Hallintasivut
{
    public partial class Varaushallinta : ContentPage
    {
        public ObservableCollection<string> AlueNimet { get; set; }

        public Varaushallinta()
        {
            InitializeComponent();
        }


        private void TarkasteleLaskuja_Clicked(object sender, EventArgs e)
        {
            // billingFolderPath on jokaisella lokaalisti oma, joten vaihda sen tilalle omalla koneella oleva polkusi
            string billingFolderPath = @"C:\Users\matil\Koulu NOT OneDrive\VillageNewbies\Mokkijengi\M-kki-varausj-rjestelm-\Billing\";

            if (!Directory.Exists(billingFolderPath))
            {
                Directory.CreateDirectory(billingFolderPath);
            }

            // Avaa kohdekansio käyttäjän koneella
            Process.Start("explorer.exe", billingFolderPath);
        }
    }
}
