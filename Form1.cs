using MarketTracker.Model;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace MarketTracker
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }
        bool streamStatus = false;
        string contractName;

        DateTime selectedDate;
        System.Timers.Timer timer = new System.Timers.Timer();
        System.Timers.Timer timer2 = new System.Timers.Timer();

        private async void button1_Click(object sender, EventArgs e)
        {
            timer.Interval = 60000;
            timer.Elapsed += Timer_Tick;

            timer2.Interval = 2000;
            timer2.Elapsed += timer2_Elapsed;


            streamStatus = true;
            label1.Text = "Stream On.";
            label1.ForeColor = Color.Green;
            timer.Start();


        }
        private async void Timer_Tick(object sender, EventArgs e)
        {
            List<clsLastData> dataList = new List<clsLastData>();
            string stream = " ";
            streamStatus = true;

            label1.Invoke((MethodInvoker)delegate
            {

                label1.Text = "Stream On";
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
            richTextBox1.Invoke((MethodInvoker)delegate
            {
                richTextBox1.Text = stream;
            });

            MatchCollection matches = Regex.Matches(stream, pattern);

            string selectedData = string.Join("-", matches.Select(m => m.Value));

            string[] dataArray = selectedData.Split("-");

            foreach (string data in dataArray)
            {
                var obj = JsonConvert.DeserializeObject<clsLastData>(data);
                dataList.Add(obj);
            }


        }

        private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            streamStatus = !streamStatus;

            label1.Invoke((MethodInvoker)delegate
            {

                label1.Text = "Awaiting..";
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
        }
    }
}
