using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JihuaUI
{
    class JihuaTask
    {
        WebSocketSharp.WebSocket wss;
        static object wss_lock = new object();
        System.Timers.Timer timer1;
        CookieCollection cookies = new CookieCollection();
        static TimeSpan ts = new TimeSpan(1, 0, 0);
        static String user0 = "admin";
        static String pwd0 = "admin";
        static String DefaultUserAgent = "Jihua";
        static String host = "http://1.85.44.234/";
        static String url_login = host + "admin/ashx/bg_user_login.ashx";
        static String url_gettask = host + "irriplan/ashx/bg_irriplan.ashx";//?action=getFineIrriPlanList";
        List<x1> start, end,outdate;
        static object _lock = new object();

        volatile bool exit;
        Thread jihua;

        public bool init()
        {
            exit = false;
            wss = null;
            start = new List<x1>();
            end = new List<x1>();
            outdate = new List<x1>();
            timer1 = new System.Timers.Timer();
            timer1.Interval = 6000;  //设置计时器事件间隔执行时间
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Elapsed);
            timer1.Enabled = true;
            jihua = new Thread(new ThreadStart(this.JihuaThread));
            jihua.Start();
            doo();
            return true;
        }

        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //执行SQL语句或其他操作
            //doo();
            doo();
        }



        async void doo()
        {
            if (login())
            {
                gettask();
            }
        }

        private void reconnect()
        {
            int timeout = 6000;

        }

        private void connect()
        {
            if (wss != null)
            {
                wss.Close();

            }
            wss = new WebSocketSharp.WebSocket("ws://1.85.44.234:9612");
            wss.OnMessage += (s, e1) => { Console.WriteLine(e1.Data); };
            wss.OnOpen += (s, e1) => {
                String a = @"{ctp:""0"",uid:""admin"",utp:""1"",op:""0""}";
                wss.Send(a);
                Console.WriteLine(" websocket open!");
            };
            wss.OnClose += (s, e1) =>
            {
                Console.WriteLine("close!");
            };
            wss.OnError += (s, e1) =>
            {
                Console.WriteLine("error1");
            };
            wss.Connect();
        }

        public bool login()
        {
            // IDictionary<string, string> parameters = { "action": 'login', 'city': '', 'remember': 'sevenday', 'loginName': 'admin', 'loginPwd': 'admin' };
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("action", "login");
            parameters.Add("remember", "sevenday");
            parameters.Add("loginName", "admin");
            parameters.Add("loginPwd", "admin");
            loginstatus ret = new loginstatus();

            HttpWebResponse response = CreatePostHttpResponse(url_login, parameters, null, null, Encoding.UTF8, cookies);
            if (response != null)
            {

                cookies = response.Cookies;
                StreamReader sr = new StreamReader(response.GetResponseStream());
                String txt = sr.ReadToEnd();
                //Console.WriteLine(txt);
                ret = JsonConvert.DeserializeObject<loginstatus>(txt);
            }
            return ret.success;
        }

        public bool gettask()
        {
            
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("action", "getFineIrriPlanList");
            //parameters.Add("stm", "2017-02-01");
            //parameters.Add("etm", "admin");
            //parameters.Add("loginPwd", "admin");
            try
            {
                HttpWebResponse response = CreatePostHttpResponse(url_gettask, parameters, null, null, Encoding.UTF8, cookies);

                cookies = response.Cookies;
                StreamReader sr = new StreamReader(response.GetResponseStream());
                String txt = sr.ReadToEnd();
                //Console.WriteLine(txt);
                tasks ret = JsonConvert.DeserializeObject<tasks>(txt);
                if (ret.total > 0)
                {
                    lock (_lock)
                    {
                        foreach (x1 x in ret.rows)
                        {
                            if (x.RUNMODE == "1")
                            {
                                //if((!start.Contains(x)) && (!end.Contains(x)) &&(!outdate.Contains(x)))
                                //    start.Add(x);
                                if (start.Contains(x))
                                    continue;
                                if (end.Contains(x))
                                    continue;
                                if (outdate.Contains(x))
                                    continue;
                                start.Add(x);
                                Console.WriteLine(x.STM + " 新任务...");
                            }
                        }
                    }
                }
            }
            catch (Exception c1) { }
            return true;
        }

        public HttpWebResponse CreatePostHttpResponse(string url, IDictionary<string, string> parameters, int? timeout, string userAgent, Encoding requestEncoding, CookieCollection cookies)
        {
            try
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
            catch(Exception e1)
            {

            }
            return null;
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受  
        }

        private void JihuaThread()
        {
            while (!exit)
            {
                x1 x = null;
                lock (_lock)
                {
                    if(start.Count >= 1)
                    {
                        x = start.First();
                        //start.Remove(x);
                    }
                }
                if(x != null)
                {
                    //Console.Write(x.STM);
                    DateTime s = Convert.ToDateTime(x.STM);
                    DateTime now = DateTime.Now;
                    now = now.AddSeconds(0-now.Second);
                    now = now.AddMilliseconds(0-now.Millisecond);
                    if((s == (now)))
                    {
                        lock (_lock)
                        {
                            end.Add(x);
                            start.Remove(x);
                        }
                        Console.WriteLine(x.STM + "启动任务...");

                    }else if( s < now)
                    {
                        if (s > (now - ts))
                        {
                            lock (_lock)
                            {
                                end.Add(x);
                                start.Remove(x);
                            }
                            Console.WriteLine(x.STM + "启动任务(晚)...");
                        }
                        else
                        {
                            lock (_lock)
                            {
                                outdate.Add(x);
                                start.Remove(x);
                            }
                            Console.WriteLine(x.STM + "启动已经过期...");
                        }
                    }
                    else
                    {
                        //Console.WriteLine("任务等待启动...");
                    }
                }
                Thread.Sleep(1);
            }
        }
    }


    public class loginstatus
    {
        public String msg { get; set; }
        public bool success { get; set; }
    }


    public class x1
    {
       //public  enum OType{
       //     waitforstart,
       //     waitforend
       // }
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
        //public OType type;

        //public x1()
        //{
        //    type = OType.waitforstart;
        //}

        public override bool Equals(object o)
        {
            if (o is x1)
            {
                x1 obj = o as x1;
                if (this == obj)
                    return true;
                if (obj == null)
                    return false;
                if (this.ID != obj.ID)
                    return false;
                if (this.TITLE != obj.TITLE) return false;
                if (this.SGNM != obj.SGNM) return false;
                if (this.BGNM != obj.BGNM) return false;
                if (this.PID != obj.PID) return false;
                if (this.SID != obj.SID) return false;
                if (this.CCD != obj.CCD) return false;
                if (this.TLNG != obj.TLNG) return false;
                if (this.DAYS != obj.DAYS) return false;
                if (this.GTP != obj.GTP) return false;
                if (this.STM != obj.STM) return false;
                if (this.ETM != obj.ETM) return false;
                if (this.RUNMODE != obj.RUNMODE) return false;
                if (this.RUNSTATE != obj.RUNSTATE) return false;
                if (this.HCD != obj.HCD) return false;
                if (this.ACTSTM != obj.ACTSTM) return false;
                if (this.ACTETM != obj.ACTETM) return false;
                if (this.MSG != obj.MSG) return false;
                return true;
            }
            return false;
        }

    }

    public class tasks
    {
        public int total;
        public x1[] rows;
    }
}
