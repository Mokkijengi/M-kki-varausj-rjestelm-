using MySqlX.XDevAPI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using MySql.Data.MySqlClient; //for sql connection


namespace booking_VillageNewbies;

public partial class Varausprosessi : ContentPage
{
    //varaustiedot tuodaan n‰ill‰ muuttujalla mainpagelta
    public DateTime alkuPvmDate { get; set; }
    public DateTime loppuPvmDate { get; set; }
    public string selectedAlue { get; set; }
    public string selectedMokki { get; set; }

    public string selectedEtunimi { get; set; }
    public string selectedSukunimi { get; set; }
    public string FullName => $"{selectedEtunimi} {selectedSukunimi}";



    //selected services from mainpage:
    public string selectedLisapalvelut { get; set; }
    public ObservableCollection<string> lisapalveluLista { get; set; }

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

    public Varausprosessi(string selectedAlue, string selectedMokki, DateTime alkuPvmDate, DateTime loppuPvmDate, string selectedLisapalvelut, string selectedEtunimi, string selectedSukunimi)
    {
        InitializeComponent();

        this.alkuPvmDate = alkuPvmDate;
        this.loppuPvmDate = loppuPvmDate;
        this.selectedAlue = selectedAlue;
        this.selectedMokki = selectedMokki;
        //CheckBoxItems = checkBoxItems;

        //selected lisapalvelut from mainpage, decomposed to a list for easier viewing, used in varausprosessi.xaml Lis‰palvelut
        this.selectedLisapalvelut = selectedLisapalvelut;
        lisapalveluLista = new ObservableCollection<string>(selectedLisapalvelut.Split(','));

        this.selectedEtunimi = selectedEtunimi;
        this.selectedSukunimi = selectedSukunimi;

        BillingOptions = new ObservableCollection<BillingOption>
        {
            new BillingOption { Label = "Email", IsSelected = false },
            new BillingOption { Label = "Print", IsSelected = false }
        };

        // Set the ObservableCollection as the ListView's ItemSource
        // Set the page's context for bindings
        this.BindingContext = this;
    }

    //gets email from sql using selectedEtunimi and selectedSukunimi
    //used in SendEmail_Clicked to input client's email
    public async Task<string> GetEmailByName(string selectedEtunimi, string selectedSukunimi)
    {
        string server = "localhost";
        string database = "vn";
        string username = "root";
        string password = "VN_password"; // Ensure this is your correct password
        string constring = $"SERVER={server};DATABASE={database};UID={username};PASSWORD={password};";
        string asiakasEmail = "";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(constring))
            {
                await conn.OpenAsync();
                string query = "SELECT email FROM asiakas WHERE etunimi = @etunimi AND sukunimi = @sukunimi";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@etunimi", selectedEtunimi);
                    cmd.Parameters.AddWithValue("@sukunimi", selectedSukunimi);

                    object result = await cmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        asiakasEmail = Convert.ToString(result);
                    }
                }
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Error connecting to the database: {ex.Message}");
            // Use Console.WriteLine for debugging purposes. 
            // Replace with DisplayAlert or a suitable method for showing errors in your application context.
        }
        return asiakasEmail;
    }


    //Opens pdf file for view, used in OpenPdf_Clicked
    //can be used to open the invoice pdf and view or use it, for example printing
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
    
    //Send the generated email invoice to the client
    //used in OnSuoritaVarausClicked, opens email composer with pre-filled information when a booking is made
    //opens users default email client with the invoice attached
    private async void SendEmail_Clicked(object sender, EventArgs e)
    {
        //asiakkaanEmail
        string clientEmail = await GetEmailByName(selectedEtunimi, selectedSukunimi);

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

    //gets client's Id from sql using selectedEtunimi and selectedSukunimi
    //needed for creating a new varaus for connecting the client to the booking
    //used in invoice generation
    public async Task<int> GetIdByName(string selectedEtunimi, string selectedSukunimi)
    {
        string server = "localhost";
        string database = "vn";
        string username = "root";
        string password = "VN_password"; // Ensure this is your correct password
        string constring = $"SERVER={server};DATABASE={database};UID={username};PASSWORD={password};";
        int asiakasId = -1;

        try
        {
            using (MySqlConnection conn = new MySqlConnection(constring))
            {
                await conn.OpenAsync();
                string query = "SELECT asiakas_id FROM asiakas WHERE etunimi = @etunimi AND sukunimi = @sukunimi";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@etunimi", selectedEtunimi);
                    cmd.Parameters.AddWithValue("@sukunimi", selectedSukunimi);

                    object result = await cmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        asiakasId = Convert.ToInt32(result);
                    }
                }
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Error connecting to the database: {ex.Message}");

        }
        return asiakasId;
    }

    //gets alueId from sql using selectedAlue
    //needed for booking process that need to know about the area
    public async Task<int> GetAlueIdByName(string selectedAlue)
    {
        string server = "localhost";
        string database = "vn";
        string username = "root";
        string password = "VN_password"; // Ensure this is your correct password
        string constring = $"SERVER={server};DATABASE={database};UID={username};PASSWORD={password};";
        int alueId = -1;

        try
        {
            using (MySqlConnection conn = new MySqlConnection(constring))
            {
                await conn.OpenAsync();
                string query = "SELECT alue_id FROM alue WHERE nimi = @nimi";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nimi", selectedAlue);

                    object result = await cmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        alueId = Convert.ToInt32(result);
                    }
                }
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Error connecting to the database: {ex.Message}");

        }
        return alueId;
    }

    //gets cabin's id and price from sql using selectedMokki
    //used to process the booking and calculate the total price
    public async Task<(int, double)> GetMokkiIdAnPriceByName(string selectedMokki)
    {
        int mokkiId = -1; // Default value if no ID is found
        double hinta = 0.0; // Default value if no price is found

        string server = "localhost";
        string database = "vn";
        string username = "root";
        string password = "VN_password"; // Ensure this is your correct password
        string constring = $"SERVER={server};DATABASE={database};UID={username};PASSWORD={password};";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(constring))
            {
                await conn.OpenAsync();
                string query = "SELECT mokki_id, hinta FROM mokki WHERE mokkinimi = @mokkinimi";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@mokkinimi", selectedMokki); //this does: WHERE mokkinimi = selectedMokki

                    object result = await cmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        var reader = await cmd.ExecuteReaderAsync();
                        if (reader.Read())
                        {
                            mokkiId = reader.GetInt32(0); // Get mokki_id
                            hinta = reader.GetDouble(1);   // Get hinta
                        }
                        reader.Close();
                    }
                    else
                    {
                        Console.WriteLine("No mokki ID found for the selected cabin.");
                    }
                }
            }
        }
        catch (MySqlException ex)
        {
            // Handle exception or log error
            Console.WriteLine($"Error retrieving mokkiId: {ex.Message}");
        }

        return (mokkiId, hinta);
    }

    //gets additional services' id and price from sql using selectedLisapalvelut
    //used to process the booking and calculate the total price
    public class LisapalveluInfo
    {
        public int PalveluId { get; set; }
        public double PalveluHinta { get; set; }
    }
    public async Task<List<LisapalveluInfo>> FetchLisapalveluDataAsync(List<string> serviceNames)
    {
        string server = "localhost";
        string database = "vn";
        string username = "root";
        string password = "VN_password";
        string connectionString = $"SERVER={server};DATABASE={database};UID={username};PASSWORD={password};";

        List<LisapalveluInfo> serviceData = new List<LisapalveluInfo>();

        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                foreach (var serviceName in serviceNames)
                {
                    string query = @"
                    SELECT p.palvelu_id, p.hinta, a.alue_id
                    FROM palvelu p
                    JOIN alue a ON p.alue_id = a.alue_id
                    WHERE p.nimi = @nimi";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nimi", serviceName);
                        cmd.Parameters.AddWithValue("@alue_id", 1); // Add this line to set the alueId parameter in the SQL query

                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                serviceData.Add(new LisapalveluInfo
                                {
                                    PalveluId = reader.GetInt32(0),
                                    PalveluHinta = reader.GetDouble(1),
                                    //AlueId = reader.GetInt32(2)
                                });
                            }
                        }
                    }
                }
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Error in database operation: {ex.Message}");
            // Handle exceptions or errors here
        }

        return serviceData;
    }

    //calculates the total price of the additional services, also adds VAT
    public async Task<double> TotalHintaPalvelut(List<string> serviceNames, int alueId)
    {
        const double ALV = 0.24;  // Example VAT rate of 24%
        List<LisapalveluInfo> additionalServices = await FetchLisapalveluDataAsync(serviceNames);

        // Calculate total price after all data is retrieved
        double total = additionalServices.Sum(service => service.PalveluHinta);

        double totalPalveluHintaJaAlv = total * (1 + ALV);
        return totalPalveluHintaJaAlv;
        //tulos on v‰‰r‰, 
    }


    //inserts a new booking to sql using information from the booking process
    //returns booking id for further use
    //used in OnSuoritaVarausClicked to create a new booking
    //stores the booking information to the database
    public async Task<int> LisaaVarausAsync(int asiakasId, int mokkiId, DateTime alkuPvm, DateTime loppuPvm)
    {
        string server = "localhost";
        string database = "vn";
        string username = "root";
        string password = "VN_password"; // Ensure this is your correct password
        string constring = $"SERVER={server};DATABASE={database};UID={username};PASSWORD={password};";
        int newVarausId = -1;

        try
        {
            using (MySqlConnection conn = new MySqlConnection(constring))
            {
                await conn.OpenAsync();
                string insertQuery = @"
                INSERT INTO varaus (asiakas_id, mokki_mokki_id, varattu_pvm, varattu_alkupvm, varattu_loppupvm)
                VALUES (@asiakasId, @mokkiId, NOW(), @alkuPvm, @loppuPvm); SELECT LAST_INSERT_ID();";

                using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@asiakasId", asiakasId);
                    cmd.Parameters.AddWithValue("@mokkiId", mokkiId); //MOKKI_ID EI MENE JOSTAIN SYYSTƒ PERILLE
                    cmd.Parameters.AddWithValue("@alkuPvm", alkuPvm);
                    cmd.Parameters.AddWithValue("@loppuPvm", loppuPvm);

                    object result = await cmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        newVarausId = Convert.ToInt32(result);
                    }
                }
            }
        }
        catch (MySqlException ex)
        {
            await DisplayAlert("Error", "Error connecting to the database: " + ex.Message, "OK");
        }

        return newVarausId; //palauttaa varausID:n koodissa k‰ytett‰v‰ksi, varaus luotu JO t‰ss‰ vaiheessa SQL p‰‰ss‰
    }

    //inserts booking spesific additional services to sql using information from the booking process
    //used in OnSuoritaVarausClicked to add additional services to the booking
    //used to track which services are added to the booking
    public async Task InsertAdditionalServices(int varausId, List<LisapalveluInfo> services)
    {
        string server = "localhost";
        string database = "vn";
        string username = "root";
        string password = "VN_password";
        string constring = $"SERVER={server};DATABASE={database};UID={username};PASSWORD={password};";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(constring))
            {
                await conn.OpenAsync();
                foreach (var service in services)
                {
                    string insertQuery = @"
                INSERT INTO varauksen_palvelut (varaus_id, palvelu_id, lkm)
                VALUES (@varausId, @palveluId, @lkm);";

                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@varausId", varausId);
                        cmd.Parameters.AddWithValue("@palveluId", service.PalveluId);
                        cmd.Parameters.AddWithValue("@lkm", 1);  // Assuming the quantity is 1 for each service unless specified otherwise

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Error inserting additional services: {ex.Message}");
        }
    }



    //generates a pdf invoice using information from the booking process
    //triggered by OnSuoritaVarausClicked
    //details client, booking, cabin, additional services, total price
    //saves the pdf to the Billing folder, uses the client's Id and timestamp as the file name
    private async void GeneratePdf(int asiakasId, double hinta, double totalPalveluHintaJaAlv, int varausId) //put generatepdf() to suorita varaus-nappi!
    {
        int days = (loppuPvmDate - alkuPvmDate).Days; // Calculate the number of days between the start and end dates
        double totalHinta = hinta * days; // Initialize the total price with the cabin price

        //lisapalveluiden yksitt‰iset hinnat
        List<LisapalveluInfo> additionalServices = await FetchLisapalveluDataAsync(lisapalveluLista.ToList());

        //string clientId = GetIdByName(selectedEtunimi, selectedSukunimi).ToString(); // T‰m‰ kaataa sovelluksen koska GetIdByName palauttaa Task<int>
        string clientId = asiakasId.ToString(); // Get the client ID
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        long laskuId = long.Parse(timestamp); // Convert the timestamp to a long (e.g., 20210915120000
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
        gfx.DrawString($"Client Name: {FullName}", defaultFont, textBrush, new XPoint(30, 220));
        gfx.DrawString($"Client ID: {asiakasId}", defaultFont, textBrush, new XPoint(30, 240));

        // Draw invoice details
        gfx.DrawString("Invoice Details:", headingFont, headingBrush, new XPoint(30, 280));
        gfx.DrawString($"Invoice Number: INV-{timestamp}", defaultFont, textBrush, new XPoint(30, 310));
        gfx.DrawString($"Date: {DateTime.Now.ToShortDateString()}", defaultFont, textBrush, new XPoint(30, 330));

        // Draw booking information - replacing items with booking-specific details
        gfx.DrawString("Cabin Name:", headingFont, headingBrush, new XPoint(30, 370));
        gfx.DrawString(selectedMokki, defaultFont, textBrush, new XPoint(200, 370));

        gfx.DrawString("Area:", headingFont, headingBrush, new XPoint(30, 390));
        gfx.DrawString(selectedAlue, defaultFont, textBrush, new XPoint(200, 390));

        gfx.DrawString("Booking Dates:", headingFont, headingBrush, new XPoint(30, 410));
        gfx.DrawString($"{alkuPvmDate.ToShortDateString()} to {loppuPvmDate.ToShortDateString()}", defaultFont, textBrush, new XPoint(200, 410));

        // Draw price information
        gfx.DrawString("Price:", headingFont, headingBrush, new XPoint(30, 430)); // Adjust the Y position as needed
        gfx.DrawString($"{totalHinta.ToString("C")}", defaultFont, textBrush, new XPoint(200, 430)); // Adjust the X and Y position as needed

        // Additional Services header
        gfx.DrawString("Additional Services:", headingFont, headingBrush, new XPoint(30, 470)); // Adjust the Y position as needed
        int yPos = 490; // Initial Y position for additional services

        // Check if any additional services were selected
        if (lisapalveluLista != null && lisapalveluLista.Count > 0)
        {
            foreach (var service in lisapalveluLista)
            {
                gfx.DrawString(service, defaultFont, textBrush, new XPoint(50, yPos));
                yPos += 20; // Move down for next service
            }
        }
        else
        {
            gfx.DrawString("None", defaultFont, textBrush, new XPoint(50, yPos));
        }

        // Draw detailed costs for services including VAT
        // Draw price information
        //double totalPalveluHintaJaAlv = await TotalHintaPalvelut(lisapalveluLista.ToList());
        gfx.DrawString("Additional Services Total:", headingFont, headingBrush, new XPoint(300, 470));
        gfx.DrawString($"{totalPalveluHintaJaAlv.ToString("C")}", defaultFont, textBrush, new XPoint(300, 490));

        double grandTotal = totalHinta + totalPalveluHintaJaAlv;
        // Draw grand total
        int grandTotalYPos = yPos + (lisapalveluLista.Count > 0 ? 20 : 0); // Adjust position based on the number of services listed
        gfx.DrawString("Grand Total:", headingFont, headingBrush, new XPoint(30, grandTotalYPos));
        gfx.DrawString($"{grandTotal.ToString("C")}", defaultFont, textBrush, new XPoint(200, grandTotalYPos));

        // Save the document
        document.Save(filePath);

        // Store the file path to open the PDF later
        pdfFilePath = filePath;

        await InsertLaskuAsync(laskuId, varausId, totalHinta + totalPalveluHintaJaAlv, 0.24);
    }

    //inserting invoice details to database after the invoice is generated
    //used in GeneratePdf
    public async Task InsertLaskuAsync(long laskuId, int varausId, double summa, double alv)
    {
        string connectionString = $"SERVER=localhost;DATABASE=vn;UID=root;PASSWORD=VN_password;";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = @"
            INSERT INTO lasku (lasku_id, varaus_id, summa, alv)
            VALUES (@laskuId, @varausId, @summa, @alv);";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@laskuId", laskuId);
                    cmd.Parameters.AddWithValue("@varausId", varausId);
                    cmd.Parameters.AddWithValue("@summa", summa);
                    cmd.Parameters.AddWithValue("@alv", alv);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Error inserting invoice data: {ex.Message}");
        }
    }

    //handles the booking process, creates a new booking, adds additional services, generates an invoice
    //main function triggers when button "Suorita varaus" is clicked
    //calls other functions to handle the booking process
    private async void OnSuoritaVarausClicked(object sender, EventArgs e)
    {
        int alueId = await GetAlueIdByName(selectedAlue); // Obtain alueId
        string asiakasEmail = await GetEmailByName(selectedEtunimi, selectedSukunimi); // Get the client's email address
        int asiakasId = await GetIdByName(selectedEtunimi, selectedSukunimi); // Get the client ID from the selected client for testing its the first one
        var totalPalveluHintaJaAlv = await TotalHintaPalvelut(lisapalveluLista.ToList(), alueId); // Calculate the total price with VAT
        var (mokkiId, hinta) = await GetMokkiIdAnPriceByName(selectedMokki); // Get the cabin ID from the selected cabin
        int varausId = await LisaaVarausAsync(asiakasId, mokkiId, alkuPvmDate, loppuPvmDate);

        // Generate the PDF
        GeneratePdf(asiakasId, hinta, totalPalveluHintaJaAlv, varausId); //oikeastaan lasku luodaan jo t‰ss‰ vaiheessa vaikka ei viel‰ valittaisi laskutusvaihtoehtoa

        await InsertAdditionalServices(varausId, await FetchLisapalveluDataAsync(lisapalveluLista.ToList()));
        //hankitaan asiakasID ja mokkinID, insertataan varausID_________________________________

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
                DisplayAlert($"Varaus luotu. Lasku luotu Billing -kansioon.", "Lasku avattu s‰hkˆpostissa k‰sittely‰ varten.", "OK");
            }
            else if (selectedOption == "Print")
            {
                OpenPdf_Clicked(sender, e);
                DisplayAlert($"Varaus luotu. Lasku luotu Billing -kansioon.", $"Lasku avattu tulostusta varten.", "OK");
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
