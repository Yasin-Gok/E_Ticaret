using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Timer = System.Windows.Forms.Timer;

namespace E_Ticaret
{
    public partial class Form1 : Form
    {
        // TextBox controls
        private TextBox txtSatisFiyati;
        private TextBox txtKargoUcreti;
        private TextBox txtVergiOrani;
        private TextBox txtKomisyonOrani;
        private TextBox txtKar;
        private TextBox txtKarOrani;
        private TextBox txtUrunMaliyeti;

        // Label controls
        private Label lblSatisFiyati;
        private Label lblKargoUcreti;
        private Label lblVergiOrani;
        private Label lblKomisyonOrani;
        private Label lblKar;
        private Label lblKarOrani;
        private Label lblConvertedAmount;
        private Label lblExchangeRate;
        private Label lblVergiMiktari;
        private Label lblKomisyonMiktari;
        private Label lblUrunMaliyeti;

        // Other controls
        private RadioButton radTL;
        private RadioButton radUSD;
        private CheckBox chkVergiOrani;
        private CheckBox chkKomisyonOrani;
        private readonly HttpClient httpClient = new HttpClient();
        private Timer exchangeRateTimer;
        private const string API_KEY = "YOUR_API_KEY";
        private decimal usdToTlRate = 0;

        public Form1()
        {
            InitializeComponent();

            // Set form background color
            this.BackColor = Color.FromArgb(35, 245, 241);  //Soft blue
            InitializeControls();
            FetchExchangeRate();  // Make sure exchange rate is fetched
            SetupExchangeRateTimer();
        }

        private void InitializeControls()
        {
            // Define consistent spacing
            int startX = 20;
            int startY = 20;
            int verticalSpacing = 40;  // Space between control groups
            int labelX = startX;
            int controlX = 120;
            int controlWidth = 100;
            int rightAlignX = 230;

            // Create and position Satış Fiyatı controls (first)
            lblSatisFiyati = new Label { 
                Text = "Satış Fiyatı:", 
                Location = new Point(labelX, startY) 
            };
            txtSatisFiyati = new TextBox { 
                Location = new Point(controlX, startY), 
                Width = controlWidth 
            };

            // Radio buttons and exchange rate
            radTL = new RadioButton { 
                Text = "TL", 
                Location = new Point(rightAlignX, startY), 
                Checked = true 
            };
            radUSD = new RadioButton { 
                Text = "USD", 
                Location = new Point(rightAlignX + 50, startY) 
            };
            lblExchangeRate = new Label { 
                Location = new Point(rightAlignX + 100, startY), 
                AutoSize = true, 
                Text = "Kur yükleniyor..." 
            };

            // Converted amount label
            lblConvertedAmount = new Label { 
                Location = new Point(controlX, startY + 25),
                AutoSize = true,
                ForeColor = Color.DarkGray,
                Font = new Font(DefaultFont.FontFamily, 8)
            };

            // Kargo Ücreti controls (second)
            lblKargoUcreti = new Label { 
                Text = "Kargo Ücreti:", 
                Location = new Point(labelX, startY + verticalSpacing) 
            };
            txtKargoUcreti = new TextBox { 
                Location = new Point(controlX, startY + verticalSpacing), 
                Width = controlWidth 
            };

            // Ürün Maliyeti controls (third)
            lblUrunMaliyeti = new Label { 
                Text = "Ürün Maliyeti:", 
                Location = new Point(labelX, startY + verticalSpacing * 2) 
            };
            txtUrunMaliyeti = new TextBox { 
                Location = new Point(controlX, startY + verticalSpacing * 2), 
                Width = controlWidth 
            };

            // Create all controls first
            lblVergiOrani = new Label { Text = "Vergi Oranı:", Location = new Point(20, 140) };
            txtVergiOrani = new TextBox { Location = new Point(120, 140), Width = 100 };

            lblKomisyonOrani = new Label { Text = "Komisyon Oranı:", Location = new Point(20, 200) };
            txtKomisyonOrani = new TextBox { Location = new Point(120, 200), Width = 100 };

            // Create radio buttons and labels
            radTL = new RadioButton { Text = "TL", Location = new Point(230, 20), Checked = true };
            radUSD = new RadioButton { Text = "USD", Location = new Point(230, 40) };
            lblConvertedAmount = new Label { Location = new Point(120, 45), AutoSize = true };
            lblExchangeRate = new Label { Location = new Point(400, 20), AutoSize = true, Text = "1 USD = 35.14 TL" };

            // Create checkboxes and their labels
            chkVergiOrani = new CheckBox { Text = "Vergi Dahil", Location = new Point(230, 140) };
            chkKomisyonOrani = new CheckBox { Text = "Komisyon Dahil", Location = new Point(230, 200) };
            lblVergiMiktari = new Label { Location = new Point(120, 165), AutoSize = true, Text = "Vergi miktarı: 0 TL" };
            lblKomisyonMiktari = new Label { Location = new Point(120, 225), AutoSize = true, Text = "Komisyon miktarı: 0 TL" };

            // Get form width for center calculation
            int formWidth = this.ClientSize.Width;
            int resultControlWidth = 120;
            int spaceBetweenResults = 60;  // Space between Kar and Kar Oranı
            
            // Calculate start X to center both controls
            int totalWidth = (resultControlWidth * 2) + spaceBetweenResults;
            int resultStartX = (formWidth - totalWidth) / 2;

            // Kar controls (left side of center)
            lblKar = new Label
            {
                Text = "Kar:",
                Location = new Point(resultStartX, startY + verticalSpacing * 6),
                Font = new Font(DefaultFont.FontFamily, 9, FontStyle.Bold),
                BackColor = Color.FromArgb(200, 230, 200),
                AutoSize = false,
                Width = resultControlWidth,
                Height = 20,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            txtKar = new TextBox
            {
                Location = new Point(resultStartX, startY + verticalSpacing * 6 + 20),
                Width = resultControlWidth,
                Height = 25,
                ReadOnly = true,
                BackColor = Color.FromArgb(230, 255, 230),
                Font = new Font("Arial", 11, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Right,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Kar Oranı controls (right side of center)
            lblKarOrani = new Label
            {
                Text = "Kar Oranı:",
                Location = new Point(resultStartX + resultControlWidth + spaceBetweenResults, startY + verticalSpacing * 6),
                Font = new Font(DefaultFont.FontFamily, 9, FontStyle.Bold),
                BackColor = Color.FromArgb(200, 230, 200),
                AutoSize = false,
                Width = resultControlWidth,
                Height = 20,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            txtKarOrani = new TextBox
            {
                Location = new Point(resultStartX + resultControlWidth + spaceBetweenResults, startY + verticalSpacing * 6 + 20),
                Width = resultControlWidth,
                Height = 25,
                ReadOnly = true,
                BackColor = Color.FromArgb(230, 255, 230),
                Font = new Font("Arial", 11, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Right,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Add all controls to form first
            Controls.AddRange(new Control[] {
                lblSatisFiyati, lblKargoUcreti, lblUrunMaliyeti, lblVergiOrani, lblKomisyonOrani, lblKar, lblKarOrani,
                txtSatisFiyati, txtKargoUcreti, txtUrunMaliyeti, txtVergiOrani, txtKomisyonOrani, txtKar, txtKarOrani,
                radTL, radUSD, lblConvertedAmount, lblExchangeRate, 
                chkVergiOrani, lblVergiMiktari,
                chkKomisyonOrani, lblKomisyonMiktari
            });

            // Then add event handlers for live calculation
            txtSatisFiyati.TextChanged += UpdateKarCalculation;
            txtKargoUcreti.TextChanged += UpdateKarCalculation;
            txtUrunMaliyeti.TextChanged += UpdateKarCalculation;
            txtVergiOrani.TextChanged += UpdateKarCalculation;
            txtKomisyonOrani.TextChanged += UpdateKarCalculation;
            chkVergiOrani.CheckedChanged += UpdateKarCalculation;
            chkKomisyonOrani.CheckedChanged += UpdateKarCalculation;
            radTL.CheckedChanged += UpdateKarCalculation;
            radUSD.CheckedChanged += UpdateKarCalculation;

            // Trigger initial calculation
            UpdateKarCalculation(null, EventArgs.Empty);

            // Add KeyPress event handlers to restrict input to numbers and decimal
            txtSatisFiyati.KeyPress += NumberAndDecimalOnly;
            txtKargoUcreti.KeyPress += NumberAndDecimalOnly;
            txtUrunMaliyeti.KeyPress += NumberAndDecimalOnly;
            txtVergiOrani.KeyPress += PercentageOnly;
            txtKomisyonOrani.KeyPress += PercentageOnly;

            // Add TextChanged handlers for percentage validation
            txtVergiOrani.TextChanged += ValidatePercentage;
            txtKomisyonOrani.TextChanged += ValidatePercentage;

            // Add event handlers for currency conversion
            txtSatisFiyati.TextChanged += UpdateConvertedAmount;
            radTL.CheckedChanged += UpdateConvertedAmount;
            radUSD.CheckedChanged += UpdateConvertedAmount;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await FetchExchangeRate();
        }

        private async Task FetchExchangeRate()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync("https://api.exchangerate-api.com/v4/latest/USD");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        dynamic data = JsonConvert.DeserializeObject(json);
                        usdToTlRate = data.rates.TRY;
                        
                        // Update the label instead of showing MessageBox
                        lblExchangeRate.Text = $"1 USD = {usdToTlRate:N2} TL";
                    }
                    else
                    {
                        lblExchangeRate.Text = "Kur bilgisi alınamadı";
                        usdToTlRate = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                lblExchangeRate.Text = "Kur bilgisi alınamadı";
                usdToTlRate = 0;
            }
        }

        private void CurrencyRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCurrencyDisplay();
        }

        private void TxtSatisFiyati_TextChanged(object sender, EventArgs e)
        {
            UpdateCurrencyDisplay();
        }

        private void UpdateCurrencyDisplay()
        {
            if (decimal.TryParse(txtSatisFiyati.Text, out decimal amount))
            {
                if (radUSD.Checked && usdToTlRate > 0)
                {
                    // Convert USD to TL using live rate
                    decimal tlAmount = amount * usdToTlRate;
                    lblConvertedAmount.Text = $"({tlAmount:N2} TL)";
                }
                else if (radTL.Checked && usdToTlRate > 0)
                {
                    // Convert TL to USD using live rate
                    decimal usdAmount = amount / usdToTlRate;
                    lblConvertedAmount.Text = $"({usdAmount:N2} USD)";
                }
            }
            else
            {
                lblConvertedAmount.Text = "";
            }
        }

        private void NumberOnlyTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow digits, decimal point, and control characters (like backspace)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
                return;
            }

            // Allow only one decimal point
            TextBox textBox = (TextBox)sender;
            if (e.KeyChar == '.' && textBox.Text.Contains('.'))
            {
                e.Handled = true;
                return;
            }
        }

        private void PercentageOnly(object sender, KeyPressEventArgs e)
        {
            // Allow numbers, decimal point, and control characters (backspace, etc.)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
                return;
            }

            TextBox textBox = (TextBox)sender;
            
            // Allow only one decimal point
            if (e.KeyChar == '.' && textBox.Text.Contains('.'))
            {
                e.Handled = true;
                return;
            }

            // If there's already a number and it's 100, only allow backspace
            if (textBox.Text == "100" && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // If there's a number greater than 100, don't allow more digits
            if (textBox.Text.Length > 0 && !char.IsControl(e.KeyChar))
            {
                if (decimal.TryParse(textBox.Text, out decimal value))
                {
                    if (value > 100)
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
        }

        private void ValidatePercentage(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            
            if (decimal.TryParse(textBox.Text, out decimal value))
            {
                if (value > 100)
                {
                    textBox.Text = "100";
                    textBox.SelectionStart = textBox.Text.Length;
                }
                else if (value < 0)
                {
                    textBox.Text = "0";
                    textBox.SelectionStart = textBox.Text.Length;
                }
            }
            else if (!string.IsNullOrEmpty(textBox.Text) && textBox.Text != ".")
            {
                textBox.Text = "0";
                textBox.SelectionStart = textBox.Text.Length;
            }
        }

        private void ChkVergiOrani_CheckedChanged(object sender, EventArgs e)
        {
            txtVergiOrani.Enabled = chkVergiOrani.Checked;
            
            if (!chkVergiOrani.Checked)
            {
                txtVergiOrani.Text = "0";
            }
            
            UpdateVergiMiktari(sender, e);
        }

        private void UpdateVergiMiktari(object sender, EventArgs e)
        {
            try
            {
                if (!chkVergiOrani.Checked)
                {
                    lblVergiMiktari.Text = "Vergi miktarı: 0 TL";
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtSatisFiyati.Text) || string.IsNullOrWhiteSpace(txtVergiOrani.Text))
                {
                    lblVergiMiktari.Text = "Vergi miktarı: Geçersiz değer";
                    return;
                }

                if (decimal.TryParse(txtSatisFiyati.Text, out decimal satisFiyati) && 
                    decimal.TryParse(txtVergiOrani.Text, out decimal vergiOrani))
                {
                    if (vergiOrani < 0 || vergiOrani > 100)
                    {
                        lblVergiMiktari.Text = "Vergi miktarı: Vergi oranı 0-100 arasında olmalı";
                        return;
                    }

                    if (satisFiyati < 0)
                    {
                        lblVergiMiktari.Text = "Vergi miktarı: Satış fiyatı negatif olamaz";
                        return;
                    }

                    vergiOrani = vergiOrani / 100;
                    
                    if ((1 + vergiOrani) == 0)
                    {
                        lblVergiMiktari.Text = "Vergi miktarı: Hesaplama hatası";
                        return;
                    }

                    decimal vergiMiktari = satisFiyati - (satisFiyati / (1 + vergiOrani));

                    if (radUSD.Checked && usdToTlRate > 0)
                    {
                        vergiMiktari *= usdToTlRate;
                        lblVergiMiktari.Text = $"Vergi miktarı: {vergiMiktari:N2} TL ({(vergiMiktari/usdToTlRate):N2} USD)";
                    }
                    else
                    {
                        lblVergiMiktari.Text = $"Vergi miktarı: {vergiMiktari:N2} TL";
                    }
                }
                else
                {
                    lblVergiMiktari.Text = "Vergi miktarı: Geçersiz sayı formatı";
                }
            }
            catch (Exception ex)
            {
                lblVergiMiktari.Text = "Vergi miktarı: Hesaplama hatası";
            }
        }

        private void ChkKomisyonOrani_CheckedChanged(object sender, EventArgs e)
        {
            txtKomisyonOrani.Enabled = chkKomisyonOrani.Checked;
            
            if (!chkKomisyonOrani.Checked)
            {
                txtKomisyonOrani.Text = "0";
            }
            
            UpdateKomisyonMiktari(sender, e);
        }

        private void UpdateKomisyonMiktari(object sender, EventArgs e)
        {
            try
            {
                if (!chkKomisyonOrani.Checked)
                {
                    lblKomisyonMiktari.Text = "Komisyon miktarı: 0 TL";
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtSatisFiyati.Text) || string.IsNullOrWhiteSpace(txtKomisyonOrani.Text))
                {
                    lblKomisyonMiktari.Text = "Komisyon miktarı: Geçersiz değer";
                    return;
                }

                if (decimal.TryParse(txtSatisFiyati.Text, out decimal satisFiyati) && 
                    decimal.TryParse(txtKomisyonOrani.Text, out decimal komisyonOrani))
                {
                    if (komisyonOrani < 0 || komisyonOrani > 100)
                    {
                        lblKomisyonMiktari.Text = "Komisyon miktarı: Komisyon oranı 0-100 arasında olmalı";
                        return;
                    }

                    if (satisFiyati < 0)
                    {
                        lblKomisyonMiktari.Text = "Komisyon miktarı: Satış fiyatı negatif olamaz";
                        return;
                    }

                    decimal komisyonMiktari = (satisFiyati * komisyonOrani) / 100;

                    if (radUSD.Checked && usdToTlRate > 0)
                    {
                        komisyonMiktari *= usdToTlRate;
                        lblKomisyonMiktari.Text = $"Komisyon miktarı: {komisyonMiktari:N2} TL ({(komisyonMiktari/usdToTlRate):N2} USD)";
                    }
                    else
                    {
                        lblKomisyonMiktari.Text = $"Komisyon miktarı: {komisyonMiktari:N2} TL";
                    }
                }
                else
                {
                    lblKomisyonMiktari.Text = "Komisyon miktarı: Geçersiz sayı formatı";
                }
            }
            catch (Exception ex)
            {
                lblKomisyonMiktari.Text = "Komisyon miktarı: Hesaplama hatası";
            }
        }


        private void UpdateKarCalculation(object sender, EventArgs e)
        {
            try
            {
                // Default values
                decimal satisFiyati = 0;
                decimal kargoUcreti = 0;
                decimal urunMaliyeti = 0;
                decimal vergiMiktari = 0;
                decimal komisyonMiktari = 0;

                // Get sales price
                if (decimal.TryParse(txtSatisFiyati.Text, out decimal parsedSatisFiyati))
                {
                    satisFiyati = parsedSatisFiyati;
                }

                // Get shipping cost
                if (!string.IsNullOrWhiteSpace(txtKargoUcreti.Text))
                {
                    decimal.TryParse(txtKargoUcreti.Text, out kargoUcreti);
                }

                // Get product cost
                if (!string.IsNullOrWhiteSpace(txtUrunMaliyeti.Text))
                {
                    decimal.TryParse(txtUrunMaliyeti.Text, out urunMaliyeti);
                }

                // Calculate tax amount
                if (chkVergiOrani.Checked && decimal.TryParse(txtVergiOrani.Text, out decimal vergiOrani))
                {
                    vergiOrani = vergiOrani / 100;
                    vergiMiktari = satisFiyati - (satisFiyati / (1 + vergiOrani));
                    lblVergiMiktari.Text = $"Vergi miktarı: {vergiMiktari:N2} TL";
                }
                else
                {
                    lblVergiMiktari.Text = "Vergi miktarı: 0 TL";
                }

                // Calculate commission amount
                if (chkKomisyonOrani.Checked && decimal.TryParse(txtKomisyonOrani.Text, out decimal komisyonOrani))
                {
                    komisyonMiktari = (satisFiyati * komisyonOrani) / 100;
                    lblKomisyonMiktari.Text = $"Komisyon miktarı: {komisyonMiktari:N2} TL";
                }
                else
                {
                    lblKomisyonMiktari.Text = "Komisyon miktarı: 0 TL";
                }

                // Calculate profit
                decimal kar = satisFiyati - kargoUcreti - vergiMiktari - komisyonMiktari - urunMaliyeti;
                
                // Calculate profit rate
                decimal karOrani = 0;
                if (satisFiyati != 0)
                {
                    karOrani = (kar / satisFiyati) * 100;
                }

                // Format and display results
                string karText;
                if (radUSD.Checked && usdToTlRate > 0)
                {
                    decimal karUSD = kar / usdToTlRate;
                    karText = $"{kar:N2} TL ({karUSD:N2} USD)";
                }
                else
                {
                    karText = $"{kar:N2} TL";
                }

                // Update display
                txtKar.Text = karText;
                txtKarOrani.Text = $"%{karOrani:N2}";

                // Update colors based on profit/loss
                Color resultColor = kar > 0 ? Color.Green : (kar < 0 ? Color.Red : Color.Black);
                txtKar.ForeColor = resultColor;
                txtKarOrani.ForeColor = resultColor;
            }
            catch
            {
                // If any error occurs, just show 0
                txtKar.Text = "0 TL";
                txtKarOrani.Text = "%0.00";
                txtKar.ForeColor = Color.Black;
                txtKarOrani.ForeColor = Color.Black;
                lblVergiMiktari.Text = "Vergi miktarı: 0 TL";
                lblKomisyonMiktari.Text = "Komisyon miktarı: 0 TL";
            }
        }

        private void NumberAndDecimalOnly(object sender, KeyPressEventArgs e)
        {
            // Allow numbers, decimal point, and control characters (backspace, etc.)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
                return;
            }

            // Allow only one decimal point
            TextBox textBox = (TextBox)sender;
            if (e.KeyChar == '.' && textBox.Text.Contains('.'))
            {
                e.Handled = true;
                return;
            }

            // Prevent multiple zeros at start
            if (e.KeyChar == '0' && textBox.Text.Length == 1 && textBox.Text[0] == '0' && textBox.SelectionStart == 1)
            {
                e.Handled = true;
                return;
            }

            // Handle first character being decimal point (add leading zero)
            if (e.KeyChar == '.' && textBox.Text.Length == 0)
            {
                textBox.Text = "0";
                textBox.SelectionStart = textBox.Text.Length;
            }
        }

        private void UpdateConvertedAmount(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtSatisFiyati.Text))
                {
                    lblConvertedAmount.Text = "";
                    return;
                }

                if (decimal.TryParse(txtSatisFiyati.Text, out decimal amount))
                {
                    if (radTL.Checked)
                    {
                        if (usdToTlRate > 0)
                        {
                            decimal usdAmount = amount / usdToTlRate;
                            lblConvertedAmount.Text = $"(${usdAmount:N2})";
                        }
                    }
                    else // USD is checked
                    {
                        decimal tlAmount = amount * usdToTlRate;
                        lblConvertedAmount.Text = $"(₺{tlAmount:N2})";
                    }
                }
                else
                {
                    lblConvertedAmount.Text = "";
                }
            }
            catch
            {
                lblConvertedAmount.Text = "";
            }
        }

        private async Task UpdateExchangeRate()
        {
            try
            {
                // Using exchangerate-api.com (free tier)
                string url = $"https://v6.exchangerate-api.com/v6/{API_KEY}/latest/USD";
                
                var response = await httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);
                
                // Get TRY rate
                usdToTlRate = json["conversion_rates"]["TRY"].Value<decimal>();
            }
            catch (Exception ex)
            {
                lblExchangeRate.Text = "Kur bilgisi alınamadı";
                usdToTlRate = 0;
            }
        }

        private void SetupExchangeRateTimer()
        {
            // Update exchange rate every 5 minutes
            exchangeRateTimer = new Timer();
            exchangeRateTimer.Interval = 5 * 60 * 1000; // 5 minutes
            exchangeRateTimer.Tick += async (s, e) => await UpdateExchangeRate();
            exchangeRateTimer.Start();

            // Initial update
            _ = UpdateExchangeRate();
        }

        private void ClearResults(string message = null)
        {
            // Clear all result fields
            txtKar.Text = "0 TL";
            txtKarOrani.Text = "%0.00";
            txtKar.ForeColor = Color.Black;
            txtKarOrani.ForeColor = Color.Black;
            lblVergiMiktari.Text = "Vergi miktarı: 0 TL";
            lblKomisyonMiktari.Text = "Komisyon miktarı: 0 TL";

            // Show error message if provided
            if (!string.IsNullOrEmpty(message))
            {
                MessageBox.Show(message, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
