using System.Collections.ObjectModel;

namespace booking_VillageNewbies;

public partial class Hallinta : ContentPage
{
    public Hallinta()
	{
		InitializeComponent();



        // Additional configurations for nameEntry if needed
        nameEntry.TextChanged += NameEntry_TextChanged;
        // Other properties can be set here as well
    }
    private void NameEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Handle the text changed event
        // Example: Console.WriteLine("Text is now: " + e.NewTextValue);
    }

}