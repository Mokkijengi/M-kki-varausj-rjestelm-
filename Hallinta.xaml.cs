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


    private async void Mokki_Clicked(object sender, EventArgs e)
    {
        Mokkihallinta mokkihallinta = new Mokkihallinta();

        await Navigation.PushAsync(mokkihallinta);
    }
}