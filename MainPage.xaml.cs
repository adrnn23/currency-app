using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text.Json;


namespace CurrencyApp
{
    class Conversion
    {
        public string inputCode { get; set; }
        public string outputCode { get; set; }
        public DateTime date { get; set; }
        public int num { get; set; }
        public float result { get; set; }

        public Conversion(string inputCode, string outputCode, DateTime date, int num, float result)
        {
            this.inputCode = inputCode;
            this.outputCode = outputCode;
            this.date = date;
            this.num = num;
            this.result = result;
        }
    }

    public partial class MainPage : ContentPage
    {
        ObservableCollection<Rate> currencyList = new ObservableCollection<Rate>();
        ObservableCollection<Conversion> conversionList = new ObservableCollection<Conversion>();
        List<Conversion> conversionsToSave = new List<Conversion>();

        public async void UpdateCurrencyList(object sender, EventArgs e)
        {
            string day = DatePick.Date.Day.ToString();
            string month = DatePick.Date.Month.ToString();
            string year = DatePick.Date.Year.ToString();

            if (int.Parse(day) < 10)
            {
                string buffor = day;
                day = "0" + buffor;
            }
            if (int.Parse(month) < 10)
            {
                string buffor = month;
                month = "0" + buffor;
            }
            string date = year + "-" + month + "-" + day;

            string url = "http://api.nbp.pl/api/exchangerates/tables/A/";
            url = url + date + "?format=json";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.nbp.pl/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string data = string.Empty;
                try
                {
                    var response = await client.GetAsync(url);
                    data = await response.Content.ReadAsStringAsync();
                    var currencyContent = JsonSerializer.Deserialize<Class1[]>(data)?.FirstOrDefault();

                    var rates = currencyContent.rates;

                    if (currencyList.Count != 0)
                    {
                        currencyList.Clear();
                    }

                    if (rates != null)
                    {
                        foreach (var rate in rates)
                        {
                            Rate currency = new Rate
                            {
                                code = rate.code + " ",
                                mid = rate.mid,
                                currency = rate.currency + " "
                            };
                            currencyList.Add(currency);
                        }

                        CurrencyList.IsVisible = true;
                        PickerStack.IsVisible = true;
                        CurrencyListLabel.IsVisible = true;
                        ConversionList.IsVisible = false;
                        ConversionListLabel.IsVisible = false;
                        await DisplayAlert("Udpate", "Currency list has been updated.", "OK");
                    }

                }
                catch (Exception ex)
                {
                    await DisplayAlert("Exception", ex.Message, "OK");
                }
            }
        }

        public async void CalculateCourse(object sender, EventArgs e)
        {
            try
            {
                int index1;
                int index2;
                if (Picker1.SelectedIndex != -1 && Picker1.SelectedIndex != -1)
                {
                    index1 = Picker1.SelectedIndex;
                    index2 = Picker2.SelectedIndex;
                    float mid1 = currencyList[index1].mid;
                    float mid2 = currencyList[index2].mid;

                    string num = await DisplayPromptAsync("How much?", $"How much {currencyList[index1].code.ToString()}?", "OK", "Cancel", keyboard: Keyboard.Numeric);
                    if (num != null)
                    {
                        int number = int.Parse(num);
                        float result = mid1 * number / mid2;

                        Conversion conversion = new Conversion(
                            currencyList[index1].code,
                            currencyList[index2].code,
                            DateTime.Now,
                            number,
                            result
                            );
                        conversionList.Add(conversion);
                        conversionsToSave.Add(conversion);
                        await DisplayAlert("Result", $"Result of converting {number} of {currencyList[index1].code} to {currencyList[index2].code} is equal to: {result}", "OK");
                        ShowBtn.IsVisible = true;
                        HistoryBtn.IsVisible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Exception", ex.Message, "OK");
            }
        }


        public async void SaveConversions(object sender, EventArgs e)
        {
            try
            {
                if (conversionsToSave.Count != 0)
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"\" + "conversions.txt");
                    using (StreamWriter sw = File.AppendText(filePath))
                    {
                        for (int i = 0; i < conversionsToSave.Count; i++)
                        {
                            string info = $"{conversionsToSave[i].num} " +
                                $"{conversionsToSave[i].inputCode} " +
                                $"{conversionsToSave[i].result} " +
                                $"{conversionsToSave[i].outputCode} " +
                                $"[{conversionsToSave[i].date.ToString()}]";
                            sw.WriteLine(info);
                        }
                        conversionsToSave.Clear();
                        await DisplayAlert("Info", "Conversion history has been saved successfully.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Exception", ex.Message, "OK");
            }
        }

        public async void ShowConversions(object sender, EventArgs e)
        {
            try
            {
                if (conversionList.Count != 0)
                {
                    CurrencyList.IsVisible = false;
                    PickerStack.IsVisible = false;
                    CurrencyListLabel.IsVisible = false;
                    ConversionList.IsVisible = true;
                    ConversionListLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Exception", ex.Message, "OK");
            }
        }

        public MainPage()
        {
            InitializeComponent();
            CurrencyList.ItemsSource = currencyList;
            ConversionList.ItemsSource = conversionList;
            Picker1.ItemsSource = currencyList;
            Picker2.ItemsSource = currencyList;

            DatePick.MaximumDate = DateTime.Today.AddDays(-1);
            DatePick.Date = DateTime.Today.AddDays(-1);
        }
    }
}