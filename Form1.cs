
using MarketTracker.Model;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using ScottPlot;
using ScottPlot.Plottable;
using MarketTracker.Functions;

namespace MarketTracker
{
    public partial class frmMain : Form
    {
        SignalPlot mySignalPlot;
        SignalPlot mySignalPlot2;
        SignalPlot mySignalPlot3;
        SignalPlot mySignalPlot4;
        SignalPlot mySignalPlot5;

        public frmMain()
        {
            InitializeComponent();

            mySignalPlot = formsPlot1.Plot.AddSignal(new double[] { });
            mySignalPlot2 = formsPlot2.Plot.AddSignal(new double[] { }, label: "Lower Band");
            mySignalPlot3 = formsPlot2.Plot.AddSignal(new double[] { }, label: "Upper Band");
            mySignalPlot4 = formsPlot2.Plot.AddSignal(new double[] { }, label: "Mid Band");
            mySignalPlot5 = formsPlot2.Plot.AddSignal(new double[] { }, label: "Son Fiyat");

            mySignalPlot2.Color = Color.Red;
            mySignalPlot3.Color = Color.Blue;
            mySignalPlot4.Color = Color.Green;
            mySignalPlot5.Color = Color.Orange;

            formsPlot2.Plot.Legend(true);
        }

        bool streamStatus = false;
        string contractName;


        List<clsLastData> contracts = new List<clsLastData>();
        List<double> positiveAverage = new List<double>();
        List<double> negativeAverage = new List<double>();
        ObservableCollection<double> rsiIndicatorSource = new ObservableCollection<double>();
        boolingerItems boolingerBands = new boolingerItems();

        List<double> upperBand = new List<double>();
        List<double> lowerBand = new List<double>();
        List<double> midBand = new List<double>();



        DateTime selectedDate;
        System.Timers.Timer timer = new System.Timers.Timer();
        System.Timers.Timer timer2 = new System.Timers.Timer();


        private async void button1_Click(object sender, EventArgs e)
        {


            var upperLine = formsPlot1.Plot.AddHorizontalLine(80);
            upperLine.LineStyle = LineStyle.Solid;
            upperLine.Color = Color.Red;

            var lowerLine = formsPlot1.Plot.AddHorizontalLine(20);
            lowerLine.LineStyle = LineStyle.Solid;
            lowerLine.Color = Color.Red;

            formsPlot1.Plot.SetAxisLimitsY(0, 100);

            formsPlot1.Render();

            formsPlot2.Render();

            timer.Interval = 10000;
            timer.Elapsed += Timer_Tick;

            timer2.Interval = 5000;
            timer2.Elapsed += timer2_Elapsed;


            streamStatus = true;
            label1.Text = "Ýlk Akýþ Bekleniyor.";
            label1.ForeColor = Color.Green;
            timer.Start();
        }
        private async void Timer_Tick(object sender, EventArgs e)
        {
            if (positiveAverage.Count > 10)
            {
                positiveAverage.RemoveAt(0);
            }
            if (negativeAverage.Count > 10)
            {
                negativeAverage.RemoveAt(0);
            }

            if (positiveAverage.Count == 10 && negativeAverage.Count == 10)
            {
                checkBox1.Invoke((MethodInvoker)delegate
                {
                    checkBox1.CheckState = CheckState.Checked;
                });
            }

            string stream = " ";
            streamStatus = true;

            List<clsLastData> dataList = new List<clsLastData>();

            label1.Invoke((MethodInvoker)delegate
            {
                label1.Text = "Akýþ Baþladý";
                label1.ForeColor = Color.Green;
            });

            selectedDate = dateKontratGun.Value;
            string date = selectedDate.ToShortDateString();
            contractName = "PH" + date.Substring(8, 2) + date.Substring(3, 2) + date.Substring(0, 2) + numericUpDown1.Value.ToString().PadLeft(2, '0');

            string pattern = @"{""kontratAd"":""" + Regex.Escape(contractName) + @""".*?}";

            string Url = "https://gip.epias.com.tr/gunici/SseServlet?event=SaatlikTabela";

            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(200000);
            var request = new HttpRequestMessage(HttpMethod.Get, Url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            var reader = new StreamReader(await response.Content.ReadAsStreamAsync());

            timer2.Start();
            while (streamStatus == true)
            {
                var line = await reader.ReadLineAsync();
                stream += line + " ";
            }
            timer2.Stop();

            MatchCollection matches = Regex.Matches(stream, pattern);

            string selectedData = string.Join("-", matches.Select(m => m.Value));

            string[] dataArray = selectedData.Split("-");

            foreach (string data in dataArray)
            {
                var obj = JsonConvert.DeserializeObject<clsLastData>(data);
                if (!(obj is null))
                {
                    dataList.Add(obj);
                }
            }
            if (!(dataList.Count == 0))
            {
                contracts.Add(dataList[dataList.Count - 1]);
            }
            if (contracts.Count >= 2)
            {
                var average = (contracts.LastOrDefault().sonFiyat - (contracts[contracts.Count - 2].sonFiyat));

                if (average > 0)
                {
                    positiveAverage.Add(average);
                }
                else if (average < 0)
                {
                    average = average * -1;
                    negativeAverage.Add(average);
                }
            }
            if (positiveAverage.Count > 0 && negativeAverage.Count > 0)
            {
                var rs = positiveAverage.Average() / negativeAverage.Average();
                if (rs > 0)
                {
                    var rsi = 100 - (100 / (1 + rs));
                    rsiIndicatorSource.Add(rsi);
                }
            }
            formsPlot1.Invoke((MethodInvoker)delegate
            {
                mySignalPlot = formsPlot1.Plot.AddSignal(rsiIndicatorSource.ToArray());
                if (rsiIndicatorSource.Count > 0)
                {
                    formsPlot1.Plot.AddText($"{contracts.Last().sonFiyat}", rsiIndicatorSource.Count - 1, rsiIndicatorSource.Last());
                }
                formsPlot1.Plot.AxisAuto();
                formsPlot1.Refresh();
            });

            formsPlot2.Invoke((MethodInvoker)delegate
            {
                var boolingerDatas = boolingerBands.boolinger(contracts);

                lowerBand.Add(boolingerDatas.lowerBand);
                upperBand.Add(boolingerDatas.upperBand);
                midBand.Add(boolingerDatas.averagePrice);

                mySignalPlot2 = formsPlot2.Plot.AddSignal(lowerBand.ToArray());
                mySignalPlot3 = formsPlot2.Plot.AddSignal(upperBand.ToArray());
                mySignalPlot4 = formsPlot2.Plot.AddSignal(midBand.ToArray());
                mySignalPlot5 = formsPlot2.Plot.AddSignal(contracts.Select(x => x.sonFiyat).ToArray());

                mySignalPlot2.Color = Color.Red;
                mySignalPlot3.Color = Color.Blue;
                mySignalPlot4.Color = Color.Green;
                mySignalPlot5.Color = Color.Orange;

                formsPlot2.Plot.AddText($"{Math.Round(lowerBand.Last(), 2)}", lowerBand.Count - 1, lowerBand.Last());
                formsPlot2.Plot.AddText($"{Math.Round(upperBand.Last(), 2)}", upperBand.Count - 1, upperBand.Last());
                formsPlot2.Plot.AddText($"{Math.Round(midBand.Last(), 2)}", midBand.Count - 1, midBand.Last());
                formsPlot2.Plot.AddText($"{contracts.Select(x => x.sonFiyat).Last()}", contracts.Count - 1, contracts.Select(x => x.sonFiyat).Last());

                formsPlot2.Plot.AxisAuto();
                formsPlot2.Refresh();
            });

        }
        private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            streamStatus = !streamStatus;

            label1.Invoke((MethodInvoker)delegate
            {
                label1.Text = "Veri Ýþlendi. Bir Sonraki Akýþ Bekleniyor..";
                label1.ForeColor = Color.DarkSlateBlue;
            });
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            label1.Text = "Stream Off";
            label1.ForeColor = Color.Red;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            streamStatus = false;
            label1.Text = "Stream Off";
            label1.ForeColor = Color.Red;
            timer.Stop();
            negativeAverage.Clear();
            positiveAverage.Clear();
            contracts.Clear();
            rsiIndicatorSource.Clear();
            formsPlot1.Plot.Clear();
            formsPlot2.Plot.Clear();
            lowerBand.Clear();
            midBand.Clear();
            upperBand.Clear();
        }
    }
}
