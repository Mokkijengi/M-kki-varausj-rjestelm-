using MySqlX.XDevAPI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;

namespace booking_VillageNewbies;

public partial class Varausprosessi : ContentPage
{

    //alue tuodaan t‰ll‰ muuttujalla mainpagelta
    //private string selectedAlue;
    public string selectedAlue { get; set; }

    // Define the file path as a class-level variable
    private string pdfFilePath;

    //asiakkaat m‰‰ritell‰‰n t‰ss‰
    public class Asiakas
    {
        public string ClientId { get; set; }
        public string Nimi { get; set; } // "Nimi" means "Name" in Finnish
    }

    //for checkbox list billing
    public class BillingOption
    {
        public string Label { get; set; }
        public bool IsSelected { get; set; }
    }

    //observablecollection, tiedot n‰kyisiv‰t listviewiss‰, ja voisi valita asiakkaan
    private ObservableCollection<Asiakas> asiakkaat;

    //billing options checkbox list
    public ObservableCollection<BillingOption> BillingOptions { get; set; }

    public ObservableCollection<Asiakas> Asiakkaat //t‰m‰ on se, joka n‰kyy listviewiss‰
    {
        get { return asiakkaat; }
        set { asiakkaat = value; }
    }

    public Varausprosessi(string selectedAlue)
    {
        InitializeComponent();

        this.selectedAlue = selectedAlue;

        // Set the page's context for bindings
        this.BindingContext = this;

        // Initialize the mock collection of clients
        Asiakkaat = new ObservableCollection<Asiakas>
        {
            new Asiakas { ClientId = "001", Nimi = "John Doe" },
            new Asiakas { ClientId = "002", Nimi = "Jane Smith" },
            new Asiakas { ClientId = "003", Nimi = "Alice Johnson" },
            new Asiakas { ClientId = "004", Nimi = "Bob Brown" }
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
            clientListView.ItemsSource = Asiakkaat; // Reset the ListView to show all clients if the search text is empty
        }
        else
        {
            // Filter the clients based on the search text
            var filteredClients = Asiakkaat.Where(asiakas => asiakas.ClientId.Contains(searchText) || asiakas.Nimi.Contains(searchText)).ToList();
            clientListView.ItemsSource = filteredClients;
        }
    }

    // Handle the ItemSelected event of the clientListView
    private void OnClientListViewItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
            return;
    }


    // get information from selected client and booking options____________________________________

    // Methods to retrieve information from the InfoPage
    private string GetName()
    {
        // Retrieve the name from your data source (e.g., ViewModel)
        return "John Doe";
    }

    private string GetClientId()
    {
        // Retrieve the client ID from your data source
        return "123456";
    }

    private string GetAddress()
    {
        // Retrieve the address from your data source
        return "123 Main St, City, State, Zip";
    }

    private string GetPhone()
    {
        // Retrieve the phone number from your data source
        return "(555) 123-4567";
    }

    private string GetEmail()
    {
        // Retrieve the email from your data source
        return "test.email@gmail.com";
    }

    // Define the event handler method with the correct signature
    private void OpenPdf_Clicked(object sender, EventArgs e)
    {
        // Ensure the file path is not null or empty
        if (!string.IsNullOrEmpty(pdfFilePath))
        {
            // Attempt to open the PDF in the default viewer
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(pdfFilePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // Log error or notify user in case the PDF cannot be opened
                Console.WriteLine($"Error opening PDF: {ex.Message}");
            }
        }
        else
        {
            // Handle case where file path is not available
            Console.WriteLine("PDF file path is not available.");
        }
    }

    private void SendEmail_Clicked(object sender, EventArgs e)
    {
        string clientEmail = GetEmail();

        // Ensure the file path is not null or empty
        if (!string.IsNullOrEmpty(pdfFilePath))
        {
            try
            {
                // Launch the email composer
                var message = new EmailMessage
                {
                    Subject = "Invoice",
                    Body = "Please find the attached invoice.",
                    To = { clientEmail } // Add the recipient's email here
                };

                // Attach the PDF file
                var file = new FileInfo(pdfFilePath);
                message.Attachments.Add(new EmailAttachment(pdfFilePath));

                // Open the email composer with the pre-filled information
                Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException)
            {
                // Email is not supported on this device
                Console.WriteLine("Email is not supported on this device.");
            }
            catch (Exception ex)
            {
                // Log error or notify user in case of any other exception
                Console.WriteLine($"Error opening email: {ex.Message}");
            }
        }
        else
        {
            // Handle case where file path is not available
            Console.WriteLine("PDF file path is not available.");
        }
    }

    //generate pdf____________________________________________________________________________________
    private void GeneratePdf() //put generatepdf() to suorita varaus-nappi!
    {
        string clientId = GetClientId(); // Retrieve the client's ID
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        string fileName = $"{clientId}_{timestamp}.pdf";

        // Define the folder path relative to the solution directory
        string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        // Navigate up to the solution directory; adjust the number of ".." based on your project's structure
        string projectDirectory = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\..\.."));
        // Combine the project directory with the relative folder path
        string relativeFolderPath = Path.Combine(projectDirectory, "Billing");

        // Ensure the Billing directory exists
        if (!Directory.Exists(relativeFolderPath))
        {
            Directory.CreateDirectory(relativeFolderPath);
        }

        // Combine the folder path with the file name to get the full file path
        string filePath = Path.Combine(relativeFolderPath, fileName);

        // Check if the file already exists
        if (File.Exists(filePath))
        {
            // Display a warning message to the user
            DisplayAlert("File Exists", "A bill for this client already exists.", "OK");
            return; // Exit the method without generating the PDF
        }

        // Retrieve invoice data from SQL database (replace with actual SQL query)
        string[] items = { "Item 1", "Item 2", "Item 3" };
        int[] quantities = { 1, 2, 1 };
        decimal[] prices = { 10.50m, 20.00m, 15.75m };

        // Calculate total
        decimal total = prices.Sum();

        // Create a new PDF document
        PdfDocument document = new PdfDocument();
        PdfPage page = document.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);

        // Define fonts
        XFont titleFont = new XFont("Arial", 20, XFontStyle.Bold);
        XFont headingFont = new XFont("Arial", 14, XFontStyle.Bold);
        XFont defaultFont = new XFont("Arial", 12);

        // Define colors
        XBrush titleBrush = XBrushes.Black;
        XBrush headingBrush = XBrushes.DarkGray;
        XBrush textBrush = XBrushes.Black;

        // Draw title
        gfx.DrawString("Lasku", titleFont, titleBrush, new XRect(30, 30, page.Width.Point - 60, 40), XStringFormats.Center);

        // Draw company information
        string companyName = "VillageNewbies";
        string companyPhone = "0400123456";
        string companyEmail = "laskutus@villagenewbies.com";
        gfx.DrawString("Company Information:", headingFont, headingBrush, new XPoint(30, 80));
        gfx.DrawString($"Company Name: {companyName}", defaultFont, textBrush, new XPoint(30, 110));
        gfx.DrawString($"Phone: {companyPhone}", defaultFont, textBrush, new XPoint(30, 130));
        gfx.DrawString($"Email: {companyEmail}", defaultFont, textBrush, new XPoint(30, 150));

        // Draw client information
        gfx.DrawString("Client Information:", headingFont, headingBrush, new XPoint(30, 190));
        gfx.DrawString($"Client Name: {GetName()}", defaultFont, textBrush, new XPoint(30, 220));
        gfx.DrawString($"Client ID: {clientId}", defaultFont, textBrush, new XPoint(30, 240));

        // Draw invoice details
        gfx.DrawString("Invoice Details:", headingFont, headingBrush, new XPoint(30, 280));
        gfx.DrawString($"Invoice Number: INV-{timestamp}", defaultFont, textBrush, new XPoint(30, 310));
        gfx.DrawString($"Date: {DateTime.Now.ToShortDateString()}", defaultFont, textBrush, new XPoint(30, 330));

        // Draw items table header
        gfx.DrawString("Item", headingFont, headingBrush, new XPoint(30, 370));
        gfx.DrawString("Quantity", headingFont, headingBrush, new XPoint(200, 370));
        gfx.DrawString("Price", headingFont, headingBrush, new XPoint(300, 370));
        gfx.DrawString("Total", headingFont, headingBrush, new XPoint(400, 370));

        // Draw items table
        int startY = 400;
        for (int i = 0; i < items.Length; i++)
        {
            int yPos = startY + i * 20;
            gfx.DrawString(items[i], defaultFont, textBrush, new XPoint(30, yPos));
            gfx.DrawString(quantities[i].ToString(), defaultFont, textBrush, new XPoint(200, yPos));
            gfx.DrawString(prices[i].ToString("C"), defaultFont, textBrush, new XPoint(300, yPos));
            gfx.DrawString((quantities[i] * prices[i]).ToString("C"), defaultFont, textBrush, new XPoint(400, yPos));
        }

        // Draw total
        gfx.DrawString($"Total: {total.ToString("C")}", headingFont, headingBrush, new XPoint(200, startY + items.Length * 20 + 20));



        // Save the document
        document.Save(filePath);

        // Store the file path to open the PDF later
        pdfFilePath = filePath;
    }

    //billing options____________________________________________________________________________________
    // Class-level variable to track if a billing option has been selected
    private void OnSuoritaVarausClicked(object sender, EventArgs e)
    {
        // Generate the PDF
        GeneratePdf(); //oikeastaan lasku luodaan jo t‰ss‰ vaiheessa vaikka ei viel‰ valittaisi laskutusvaihtoehtoa
        //voisi muokata pdf tiedostojen nime‰ esim: "lasku" + asiakasid + timestamp + ".pdf" ->
        //estet‰‰n kopioden luonti tarkastuksella, esim: asiakkaalla: nimi + id on jo lasku, haluatko varmasti luoda uuden laskun?

        // Get the count of selected billing options
        var selectedOptionsCount = BillingOptions.Count(option => option.IsSelected);

        // Ensure exactly one billing option is selected
        if (selectedOptionsCount == 1)
        {
            var selectedOption = BillingOptions.FirstOrDefault(option => option.IsSelected)?.Label;

            // 
            if (selectedOption == "Email")
            {
                SendEmail_Clicked(sender, e);
                DisplayAlert("Varaus luotu. Lasku luotu Billing -kansioon.", "Lasku avattu s‰hkˆpostissa k‰sittely‰ varten.", "OK");
            }
            else if (selectedOption == "Print")
            {
                OpenPdf_Clicked(sender, e);
                DisplayAlert("Varaus luotu. Lasku luotu Billing -kansioon.", "Lasku avattu tulostusta varten.", "OK");
            }
        }
        else if (selectedOptionsCount > 1)
        {
            // Alert the user if more than one option is selected
            DisplayAlert("Useita vaihtoehtoja valittu!", "Valitse vain yksi laskutusvaihtoehto. Laskun lis‰kopiot saatavilla Billing -kansiossa.", "OK");
        }
        else
        {
            // Alert the user if no option is selected
            DisplayAlert("Laskutusta ei valittu!", "Valitse yksi vaihtoehto.", "OK");
        }
    }


}
