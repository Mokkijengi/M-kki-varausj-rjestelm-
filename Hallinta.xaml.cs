using booking_VillageNewbies.Hallintasivut;
using System.Collections.ObjectModel;

namespace booking_VillageNewbies;

public partial class Hallinta : ContentPage
{
    public Hallinta()
	{
		InitializeComponent();

  
    }
    private void NameEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Handle the text changed event
        // Example: Console.WriteLine("Text is now: " + e.NewTextValue);
    }


    private async void Mokit_Clicked(object sender, EventArgs e)
    {
        Mokkihallinta mokkihallinta = new Mokkihallinta();

        await Navigation.PushAsync(mokkihallinta);
    }

    private async void Palvelut_Clicked(object sender, EventArgs e)
    {
        Palveluhallinta palveluhallinta = new Palveluhallinta();

        await Navigation.PushAsync(palveluhallinta);
    }

    private async void Alueet_Clicked(object sender, EventArgs e)
    {
        Aluehallinta aluehallinta = new Aluehallinta();

        await Navigation.PushAsync(aluehallinta);
    }

    private async void Asiakkaat_Clicked(object sender, EventArgs e)
    {
        Asiakashallinta asiakashallinta = new Asiakashallinta();

        await Navigation.PushAsync(asiakashallinta);
    }

    private async void Varaukset_Clicked(object sender, EventArgs e)
    {
        Varaushallinta varaushallinta = new Varaushallinta();

        await Navigation.PushAsync(varaushallinta);
    }
}