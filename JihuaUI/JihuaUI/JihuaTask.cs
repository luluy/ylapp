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
        IDictionary<string, string> rtu,utr;
        DateTime dtLastrtu;
        System.Timers.Timer timer1;
        CookieCollection cookies = new CookieCollection();
        static TimeSpan ts = new TimeSpan(1, 0, 0);
        static String user0 = "admin";
        static String pwd0 = "admin";
        static String DefaultUserAgent = "Jihua";
        static String host = "http://1.85.44.234/";
        static String url_login = host + "admin/ashx/bg_user_login.ashx";
        static String url_gettask = host + "irriplan/ashx/bg_irriplan.ashx";//?action=getFineIrriPlanList";
        static String url_getrtu = host + "bases/ashx/bg_stat.ashx";
        static String url_update = host + "irriplan/ashx/bg_irriplan.ashx";//"irriplan/ashx/bg_irrplan.ashx";
        int getrtutimeout = 6000;
        List<x1> start, end,outdate,ok,pending,pendinge;
        static object _lock = new object();

        volatile bool exit;
        Thread jihua;

        public bool init()
        {
            login();
            exit = false;
            rtu = new Dictionary<string, string>();
            utr = new Dictionary<string, string>();
            dtLastrtu = DateTime.Now.AddDays(-2);
            //getrtu();
            wss = null;
            start = new List<x1>(); // 等待到达预订开始时间后执行的等待列表。
            end = new List<x1>(); // 任务开始后已经发送开始命令，并接收到命令回复的列表(等待到达预订关闭时间后执行的等待列表。)
            outdate = new List<x1>(); // 过期任务列表
            ok = new List<x1>(); // 完成《开始-结束》计划流程的列表
            pending = new List<x1>(); // 到了任务开始时间，已经发送了开始命令，还没有收到回复的列表。
            pendinge = new List<x1>(); // 到了任务结束时间，已经发送了停止命令，还没有收到回复的列表。
            timer1 = new System.Timers.Timer();
            timer1.Interval = getrtutimeout;  //设置计时器事件间隔执行时间
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Elapsed);
            timer1.Enabled = true;
            connect();
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
            //if (login())
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
            wss.WaitTime = new TimeSpan(0, 0, 8);
            wss.OnMessage += (s, e1) => {
                Console.WriteLine(e1.Data);
                sockobj d = JsonConvert.DeserializeObject<sockobj>(e1.Data);
                if (d.op == "4D")
                {
                    update(d);
                }

            };
            wss.OnOpen += (s, e1) => {
                String a = @"{ctp:""0"",uid:""service"",utp:""1"",op:""0""}";
                lock (wss_lock)
                {
                    wss.Send(a);
                }
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

        public bool update(sockobj x)
        {
            String stm = "";
            int state = 4;
            String hcd = utr[x.rtu];
            String Guid = x.serial;

            if (x.uid != "service") return false;


            if (x.success)
            {
                if(x.value == "0101") //开机
                {
                    state = 2;
                    lock (_lock)
                    {
                        foreach(x1 o in pending)
                        {
                            if(o.GUID == Guid)
                            {
                                stm = o.STM;
                                end.Add(o);
                                pending.Remove(o);
                                break;
                            }
                        }
                    }
                }
                else if(x.value == "0100")//关机
                {
                    state = 3;
                    lock (_lock)
                    {
                        foreach (x1 o in pendinge)
                        {
                            if (o.GUID == Guid)
                            {
                                stm = o.STM;
                                ok.Add(o);
                                pendinge.Remove(o);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                lock (_lock)
                {
                    foreach (x1 o in pending)
                    {
                        if (o.GUID == Guid)
                        {
                            stm = o.STM;
                            outdate.Add(o);
                            pendinge.Remove(o);
                            break;
                        }
                    }
                }
            }
            
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("action", "updateIrriPlanState");
            parameters.Add("state", state.ToString());
            parameters.Add("stm", stm);
            parameters.Add("hcd", hcd);
            parameters.Add("acttm", DateTime.Now.ToString());
            parameters.Add("msg", x.message);
            parameters.Add("id", "1");
            parameters.Add("guid", Guid);
            loginstatus ret = new loginstatus();

            HttpWebResponse response = CreatePostHttpResponse(url_update, parameters, null, null, Encoding.UTF8, cookies);
            if (response != null)
            {

                //cookies = response.Cookies;
                StreamReader sr = new StreamReader(response.GetResponseStream());
                String txt = sr.ReadToEnd();
                //Console.WriteLine(txt);
                ret = JsonConvert.DeserializeObject<loginstatus>(txt);
                return ret.success;
            }
            return true;
        }

        public bool login()
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("action", "login");
            parameters.Add("remember", "sevenday");
            parameters.Add("loginName", "service");
            parameters.Add("loginPwd", "123456");
            loginstatus ret = new loginstatus();

            HttpWebResponse response = CreatePostHttpResponse(url_login, parameters, null, null, Encoding.UTF8, cookies);
            if (response != null)
            {
                cookies = response.Cookies;
                StreamReader sr = new StreamReader(response.GetResponseStream());
                String txt = sr.ReadToEnd();
                Console.WriteLine(txt);
                ret = JsonConvert.DeserializeObject<loginstatus>(txt);
                return ret.success;
            }
            return false;
        }

        public bool gettask()
        {
            
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("action", "getFineIrriPlanList");
            try
            {
                HttpWebResponse response = CreatePostHttpResponse(url_gettask, parameters, null, null, Encoding.UTF8, cookies);

                //cookies = response.Cookies;
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
                            if (x.RUNSTATE == "1")
                            {
                                //if((!start.Contains(x)) && (!end.Contains(x)) &&(!outdate.Contains(x)))
                                //    start.Add(x);
                                if (start.Contains(x))
                                    continue;
                                if (end.Contains(x))
                                    continue;
                                if (outdate.Contains(x))
                                    continue;
                                if (ok.Contains(x))
                                    continue;
                                if (pending.Contains(x))
                                    continue;
                                if (pendinge.Contains(x))
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
                Console.WriteLine(e1.Message);
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
                getrtu();
                x1 x = null;
                
                DateTime now = DateTime.Now;
                now = now.AddSeconds(0 - now.Second);
                now = now.AddMilliseconds(0 - now.Millisecond);
                lock (_lock)
                {
                    foreach(x1 a  in start)
                    {
                        DateTime s = Convert.ToDateTime(a.STM);
                        if (s >= now) continue;
                        x = a;
                        break;
                    }
                }
                if(x != null)
                {
                    DateTime s = Convert.ToDateTime(x.STM);
                    if ((s == (now)))
                    {
                        lock (_lock)
                        {
                            x.initretry();
                            pending.Add(x);
                            start.Remove(x);
                        }
                    }else if( s < now)
                    {
                        if (s > (now - ts))
                        {
                            lock (_lock)
                            {
                                x.initretry();
                                pending.Add(x);
                                start.Remove(x);
                            }
                        }
                        else
                        {
                            lock (_lock)
                            {
                                outdate.Add(x);
                                start.Remove(x);
                            }
                        }
                    }
                    else
                    {
                        //Console.WriteLine("任务等待启动...");
                    }
                }

                x = null;

                lock (_lock)
                {
                    foreach (x1 a in end)
                    {
                        DateTime s = Convert.ToDateTime(a.ETM);
                        if (s >= now) continue;
                        x = a;
                        break;
                    }
                }
                if (x != null)
                {
                    //Console.Write(x.STM);
                    DateTime s = Convert.ToDateTime(x.ETM);

                    lock (_lock)
                    {
                        x.initretry();
                        pendinge.Add(x);
                        end.Remove(x);
                    }
                }
                /*
                lock (wss_lock)
                {
                    if (!wss.IsAlive)
                        wss.Connect();
                }
                */
                lock (_lock)
                {
                    foreach (x1 o in pending)
                    {
                        if (o.retry())
                        {
                            openswitch(o);
                        }
                        else if(o.getretry() > 3)
                        {
                            pending.Remove(o);
                            outdate.Add(o);
                            break;
                        }
                    }

                    foreach(x1 o in pendinge)
                    {
                        if (o.retry())
                        {
                            closeswitch(o);
                        }
                        else if(o.getretry() > 3)
                        {
                            pendinge.Remove(o);
                            outdate.Add(o);
                            break;
                        }
                    }
                }


                Thread.Sleep(1);
            }
        }

        private void getrtu()
        {
            DateTime now = DateTime.Now;
            if (dtLastrtu.Day == now.Day) return;
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("action", "getSTCDRel");
            try
            {
                HttpWebResponse response = CreatePostHttpResponse(url_getrtu, parameters, null, null, Encoding.UTF8, cookies);

                //cookies = response.Cookies;
                StreamReader sr = new StreamReader(response.GetResponseStream());
                String txt = sr.ReadToEnd();
                //Console.WriteLine(txt);
                rtus ret = JsonConvert.DeserializeObject<rtus>(txt);
                if (ret.total > 0)
                {
                    foreach (rtu x in ret.rows)
                    {
                        rtu[x.CD] = x.STCD;
                        utr[x.STCD] = x.CD;
                    }
                }
                Console.WriteLine("getrtu ok...");
                dtLastrtu = now;
            }
            catch (Exception c1) { }
       
        }


        private void openswitch(x1 x)
        {
            lock (wss_lock)
            {
                String rtuid =  rtu[x.HCD];
                String cmd = "{\"ctp\":1,\"uid\":\"service\",\"utp\":1,\"rtu\": \"" + rtuid+ "\",\"op\":\"4D\",\"value\":\"0101\",\"serial\":\"" + x.GUID + "\"}"; 

                wss.Send(cmd);
                Console.WriteLine(x.STM + "发送开启阀门命令...");

            }

        }

        private void closeswitch(x1 x)
        {
            lock (wss_lock)
            {
                String rtuid = rtu[x.HCD];
                String cmd = "{\"ctp\":1,\"uid\":\"service\",\"utp\":1,\"rtu\": \"" + rtuid + "\",\"op\":\"4D\",\"value\":\"0000\",\"serial\":\"" + x.GUID + "\"}";

                wss.Send(cmd);
                Console.WriteLine(x.ETM + "发送关闭阀门命令...");
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
        public String GUID;
        private int iRetry;
        private DateTime last_iRetry;
        public int getretry() { return iRetry; }
        public void initretry() {
            iRetry = 0;
            last_iRetry = DateTime.Now.AddMinutes(-3);
        }
        public bool retry()
        {
            TimeSpan ts = DateTime.Now - last_iRetry;
            if (ts.TotalSeconds  >= (3 * 60))
            {
                iRetry++;
                last_iRetry = DateTime.Now;
                if (iRetry <=3)
                    return true;
            }
            return false;
        }
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
                if (this.GUID != obj.GUID) return false;
                return true;
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
        public String msg;
        public bool success;
        public int total;
        public x1[] rows;
    }

    public class rtu
    {
        public String STCD;
        public String CD;

    }

    public class rtus
    {
        public String msg;
        public bool success;
        public int total;
        public rtu[] rows;
    }

    public class sockobj
    {
        public String ctp;
        public String uid;
        public String utp;
        public String utp_name;
        public String rtu;
        public String op;
        public String op_desc;
        public String value;
        public String otp;
        public String broad;
        public bool success;
        public String message;
        public String serial;
        public String tm;
        public String data;
        public String guid;
    }
}
