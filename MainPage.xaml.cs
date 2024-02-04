using System.Collections.ObjectModel;

namespace booking_VillageNewbies
{
    public partial class MainPage : ContentPage
    {

        public class CheckBoxItem
        {
            public bool IsSelected { get; set; }
            public string Label { get; set; }
        }

        //create variable for the cabin list
        public ObservableCollection<string> CabinNames { get; set; }
        public ObservableCollection<CheckBoxItem> CheckBoxItems { get; set; }

        public MainPage()
        {
            InitializeComponent();

            // Set the initial value of the Picker for trying out the binding
            aluePicker.Items.Add("Ylläs");
            aluePicker.Items.Add("Levi");
            aluePicker.Items.Add("Ruka");

            // Initialize the ObservableCollection with fake cabin names
            CabinNames = new ObservableCollection<string>
            {
                "Metsämökki",
                "Järvenranta",
                "Tunturilodge",
                "Rantasauna",
                "Korpikoti",
                "Talvimaja",
                "Koivutupa",
                "Hirsihuvila",
                "Lomamökki",
                "Revontulimaja"
            };

            // Set the ObservableCollection as the ListView's ItemSource
            cabinListView.ItemsSource = CabinNames;

            //kun mökki valitaan listalta, avaa pääsivulle sen tiedot. 

            CheckBoxItems = new ObservableCollection<CheckBoxItem>
            {
                new CheckBoxItem { IsSelected = false, Label = "Option 1" },
                new CheckBoxItem { IsSelected = false, Label = "Option 2" },
                new CheckBoxItem { IsSelected = false, Label = "Option 3" }
            };

            checkBoxList.ItemsSource = CheckBoxItems;
        }


    }
}