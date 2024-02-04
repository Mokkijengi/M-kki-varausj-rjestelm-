namespace booking_VillageNewbies
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }
        private async void OnHallintoClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"///{nameof(Hallinta)}");
        }

        private async void OnVarausClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"///{nameof(MainPage)}");
        }
    }
}