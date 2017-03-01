using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace JihuaUI
{
    public partial class Form1 : Form
    {
        WebSocketSharp.WebSocket wss;
        System.Timers.Timer timer1;
        CookieCollection cookies = new CookieCollection();
        static String DefaultUserAgent = "Jihua";
        static String host = "http://1.85.44.234/";
        static String url_login = host + "admin/ashx/bg_user_login.ashx";
        static String url_gettask = host + "irriplan/ashx/bg_irriplan.ashx";//?action=getFineIrriPlanList";
        List<x1> start, end;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            wss = new WebSocketSharp.WebSocket("ws://1.85.44.234:9612");
            wss.OnMessage += new
            timer1 = new System.Timers.Timer();
            timer1.Interval = 6000;  //设置计时器事件间隔执行时间
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Elapsed);
            timer1.Enabled = true;
            start = new List<x1>();
            end = new List<x1>();
            doo();
        }

        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //执行SQL语句或其他操作
            //doo();
        }

        async void doo()
        {
            if (login())
            {
                gettask();
            }
        }

        public bool login()
        {
            // IDictionary<string, string> parameters = { "action": 'login', 'city': '', 'remember': 'sevenday', 'loginName': 'admin', 'loginPwd': 'admin' };
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("action", "login");
            parameters.Add("remember", "sevenday");
            parameters.Add("loginName", "admin");
            parameters.Add("loginPwd", "admin");

            HttpWebResponse response = CreatePostHttpResponse(url_login, parameters, null, null, Encoding.UTF8, cookies);

            cookies = response.Cookies;
            StreamReader sr = new StreamReader(response.GetResponseStream());
            String txt = sr.ReadToEnd();
            Console.WriteLine(txt);
            loginstatus ret = JsonConvert.DeserializeObject<loginstatus>(txt);
            return ret.success;
        }

        public bool gettask()
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("action", "getFineIrriPlanList");
            parameters.Add("stm", "2017-02-01");
            //parameters.Add("etm", "admin");
            //parameters.Add("loginPwd", "admin");

            HttpWebResponse response = CreatePostHttpResponse(url_gettask, parameters, null, null, Encoding.UTF8, cookies);

            cookies = response.Cookies;
            StreamReader sr = new StreamReader(response.GetResponseStream());
            String txt = sr.ReadToEnd();
            Console.WriteLine(txt);
            tasks ret = JsonConvert.DeserializeObject<tasks>(txt);
            if(ret.total > 0)
            {
                foreach(x1 x in ret.rows)
                {
                    if(x.RUNMODE == "1")
                    {
                        start.Add(x);
                    }
                }
            }
            return true;
        }

        public HttpWebResponse CreatePostHttpResponse(string url, IDictionary<string, string> parameters, int? timeout, string userAgent, Encoding requestEncoding, CookieCollection cookies)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            if (requestEncoding == null)
            {
                throw new ArgumentNullException("requestEncoding");
            }
            HttpWebRequest request = null;
            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            else
            {
                request.UserAgent = DefaultUserAgent;
            }

            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            //如果需要POST数据  
            if (!(parameters == null || parameters.Count == 0))
            {
                StringBuilder buffer = new StringBuilder();
                int i = 0;
                foreach (string key in parameters.Keys)
                {
                    if (i > 0)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, parameters[key]);
                    }
                    i++;
                }
                byte[] data = requestEncoding.GetBytes(buffer.ToString());
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            return request.GetResponse() as HttpWebResponse;
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受  
        }
    }

    public class loginstatus
    {
        public String msg { get; set; }
        public bool success { get; set; }
    }


    public class x1
    {
        public String ID { get; set; }
        public String TITLE;
        public String SGNM;
        public String BGNM;
        public String PID;
        public String SID;
        public String CCD;
        public String TLNG;
        public String DAYS;
        public String GTP;
        public String STM;
        public String ETM;
        public String RUNMODE;
        public String RUNSTATE;
        public String HCD;
        public String ACTSTM;
        public String ACTETM;
        public String MSG;

    }

    public class tasks{
        public int total;
        public x1[] rows;
    }
}
