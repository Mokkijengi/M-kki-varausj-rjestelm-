using MySqlX.XDevAPI;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace booking_VillageNewbies;

public partial class Varausprosessi : ContentPage
{
    //aasit m‰‰ritell‰‰n t‰ss‰
    public class Aasi
    {
        public string ClientId { get; set; }
        public string Nimi { get; set; } // Assuming "Nimi" is the Finnish translation for "Name"
    }

    //for checkbox list billing
    public class BillingOption
    {
        public string Label { get; set; }
        public bool IsSelected { get; set; }
    }


    //ehk‰ observablecollection, tiedot n‰kyisiv‰t listviewiss‰, ja voisi valita asiakkaan
    private ObservableCollection<Aasi> aasiakkaat;

    //billing options checkbox list
    public ObservableCollection<BillingOption> BillingOptions { get; set; }

    public ObservableCollection<Aasi> Aasiakkaat //t‰m‰ on se, joka n‰kyy listviewiss‰
    {
        get { return aasiakkaat; }
        set { aasiakkaat = value; }
    }

    public Varausprosessi()
    {
        InitializeComponent();

        // Initialize the mock collection of clients
        Aasiakkaat = new ObservableCollection<Aasi>
        {
            new Aasi { ClientId = "001", Nimi = "John Doe" },
            new Aasi { ClientId = "002", Nimi = "Jane Smith" },
            new Aasi { ClientId = "003", Nimi = "Alice Johnson" },
            new Aasi { ClientId = "004", Nimi = "Bob Brown" }
            // Add more mock clients as needed for testing
        };

        BillingOptions = new ObservableCollection<BillingOption>
        {
            new BillingOption { Label = "Email", IsSelected = false },
            new BillingOption { Label = "Print", IsSelected = false }
        };

        // Set the ObservableCollection as the ListView's ItemSource
        BindingContext = this;
    }

    // Event handler for the SearchBar's TextChanged event
    private void OnClientSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue;
        if (string.IsNullOrEmpty(searchText))
        {
            clientListView.ItemsSource = Aasiakkaat; // Reset the ListView to show all clients if the search text is empty
        }
        else
        {
            // Filter the clients based on the search text
            var filteredClients = Aasiakkaat.Where(aasi => aasi.ClientId.Contains(searchText) || aasi.Nimi.Contains(searchText)).ToList();
            clientListView.ItemsSource = filteredClients;
        }
    }

    // Handle the ItemSelected event of the clientListView
    private void OnClientListViewItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
            return;

    }



}