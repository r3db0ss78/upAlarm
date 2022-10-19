using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace upAlarm
{
    public class APing : Ping
    {
        public APing()
        {
            Buffer = new byte[3]; //by default, ping em with 3 bytes to ease the network load
            FrequencyMs = 500;
            TimeoutMs = 1000;
            Ttl = 128;
            DontFragment = true;
            IsRunning = false;
            IsWebRunning = false;
            IsUp = false;
            IsWebUp = false;
            Proc=null;
            Buffer.Initialize();
        }

        public APing(string address)
        {
            Ip = address;
            Buffer = new byte[3];
            FrequencyMs = 1000;
            TimeoutMs = 1000;
            Ttl = 128;
            DontFragment = true;
            IsRunning = false;
            IsWebRunning = false;
            IsUp = false;
            IsWebUp = false;
            Proc = null;
           
        }

        public string Ip { get; set; }
        public byte[] Buffer { get; set; }
        public int FrequencyMs { get; set; }
        public int Ttl { get; set; }
        public int TimeoutMs { get; set; }
        public bool DontFragment { get; set; }
        public bool IsRunning { get; set; }
        public bool IsWebRunning{ get; set;}
        public bool IsUp { get; set; }
        public bool IsWebUp { get; set; }
        public int ThreadId { get; set; }
        public long ReplyMs { get; set; }
        public Task Proc { get; set; }

        public string Header()
        {
            return $"Pinging {Ip} with {Buffer.Count()} bytes {Ttl} ttl";
        }

        public static string ReplyText(PingReply reply)
        {
            string buff = "";
            string bytes = "";
            string ms = "";
            string replyAddress = "";

            if (reply.RoundtripTime > 0)
            {
                ms = $"{reply.RoundtripTime} ms";
            }

            if (reply.Address != null)
            {
                replyAddress = $"from {reply.Address}";
            }

            if (reply.Buffer != null && reply.Buffer.Length > 0)
            {

                foreach (byte b in reply.Buffer)
                {
                    bytes += b.ToString();
                }
                bytes += " bytes";
            }


            return buff = $"{reply.Status} {bytes} {replyAddress} {ms} ";

        }

        public static long ReplyMsTime(PingReply reply)
        {
            return reply.RoundtripTime;
        }

            public static List<string> ReplyList(PingReply reply)
        {
            string buff = "";
            string bytes = "";
            string ms = "";
            string replyAddress = "";
            List<string> ret = new List<string>();

            if (reply.RoundtripTime > 0)
            {
                ms = $"{reply.RoundtripTime} ms";
            }

            if (reply.Address != null)
            {
                replyAddress = $"from {reply.Address}";
            }

            if (reply.Buffer != null && reply.Buffer.Length > 0)
            {

                foreach (byte b in reply.Buffer)
                {
                    bytes += b.ToString();
                }
                bytes += " bytes";
            }


            buff = $"{reply.Status} {bytes} {replyAddress} {ms} \n";
            ret.Add(buff);

            return ret;
        }

        public static bool ValidateReply(PingReply reply, string senderAddress)
        {
            if (reply.Status.ToString() != "Success" && (reply.Address == null || reply.Address.ToString() != senderAddress))
            {
                return false;
            }
            return true;
        }
       
        public static string GetHostName(string input)
        {
            if(input.IndexOf("://")!=0)
            {
                try
                {
                    Uri uri = new Uri(input);
                    return uri.Host;
                }
                catch
                {
                    return input;
                }
               
            }
            else
            {
                return input;
            }
           
        }

    }
}