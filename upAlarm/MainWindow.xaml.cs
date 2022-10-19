using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Timers;
using System.Threading;
using System.Media;

namespace upAlarm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// //todo layout
    /// </summary>
    public partial class MainWindow : Window
    {

        List<APing> ips = new List<APing>();
        List<ComboBoxItem> list = new List<ComboBoxItem>();
        List<ComboBoxItem> listWeb = new List<ComboBoxItem>();
        StackPanel panel = new StackPanel();
       

        public MainWindow()
        {
            InitializeComponent();
            APing ping = new APing();
            InitializeInputs(ping);
        }

        public void InitializeInputs(APing ping)
        {
            FrequencyMs.Text = ping.FrequencyMs.ToString();
            Buffer.Text = ping.Buffer.Count().ToString();
            panel.Orientation = Orientation.Horizontal;
            listBox1.Items.Add(panel);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
            {

                this.ShowInTaskbar = true;
                this.Hide();

            }


            base.OnStateChanged(e);
        }

        private void UserInput_GotFocus(object sender, RoutedEventArgs e)
        {
            UserInput.Text = "";
        }

        private void Key_Down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                int frequency = Convert.ToInt32(FrequencyMs.Text);
                int size = Convert.ToInt32(Buffer.Text);
                AddIp(APing.GetHostName(UserInput.Text.ToString()), frequency, size);

                UserInput.Text = "";
                e.KeyboardDevice.Focus(UserInput);
                DoPings();
            }
        }

        private void Ping_Click(object sender, RoutedEventArgs e)
        {
            if (UserInput.Text.ToString().Length > 0)
            {
                int frequency = Convert.ToInt32(FrequencyMs.Text);
                int size = Convert.ToInt32(Buffer.Text);
                AddIp(APing.GetHostName(UserInput.Text.ToString()), frequency, size);
                DoPings();
            }
        }

        private void Run()
        {
            DoPings();
        }

        public void AddIp(string hostname, int frequencyMs, int sizeBytes)
        {
            UserMsg("");
            try
            {
                IPHostEntry hostEntry =  Dns.GetHostEntry(hostname);

                if (hostEntry.AddressList.Length > 0)
                {
                  
                    byte[] buffer = new byte[sizeBytes];
                    buffer.Initialize();
                    APing ping = new APing();
                    ping.Ip = hostEntry.HostName;
                    ping.FrequencyMs = frequencyMs;

                    ping.ThreadId= (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                    ips.Add(ping);
                    AddBox(ping);
                  
                }
                else
                {
                    UserMsg($"Invalid Host/IP: {hostname}");
                }
            }
            catch
            {
                UserMsg($"Invalid Host/IP: {hostname}");
            }

        }

        public void DoPings()
        {
          
            foreach (APing ip in this.ips.AsEnumerable().Where(t=>t.IsRunning==false))
            {
                ips.Find(a => a.ThreadId == ip.ThreadId).IsRunning = true;
              
                try
                {
                    Task t = new Task(() =>
                    {
                        System.Timers.Timer timer = new System.Timers.Timer(1000);
                        timer.Elapsed += new ElapsedEventHandler(async delegate
                                    {
                                        SetValue(await DoPing(ip), ip);
                                    });

                        timer.Start();

                    });
                    t.RunSynchronously();
                }
                catch (Exception e)
                {
                    UserMsg(e.Message);
                    GC.Collect();
                }//end catch
            
            }

            foreach (APing ip in this.ips.AsEnumerable().Where(t => t.IsRunning == true))
            {
                System.Timers.Timer timer = new System.Timers.Timer(100);
                timer.Elapsed += new ElapsedEventHandler(async delegate
                {
                    SetValue(await DoPing(ip), ip);
                });

                timer.Start();
            }


            }


        //todo implement task for async return
        public async Task<PingReply> DoPing(APing aping)
        {
            PingReply pong = null;
            try
            {
                       
                            Ping ping = new Ping();
                            PingOptions options = new PingOptions(aping.Ttl, aping.DontFragment);
                            try //dirty fix
                            {
                               
                                pong = await ping.SendPingAsync(aping.Ip, aping.TimeoutMs, aping.Buffer, options);
                           
                            }
                            catch
                            {
                                
                            }


                
            }
            catch (Exception e)
            {
                UserMsg(e.Message);
                GC.Collect();
            }

            return pong;
        }



        public void UserMsg(string msg)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                errorMsg.Text = msg;
            });
        }


        public void AddBox(APing ip)
        {
          
            TextBlock output = new TextBlock();
            output.TextAlignment = TextAlignment.Center;
            output.Text = " " + ip.Ip + " : ...";
            output.Name = $"box_{ip.ThreadId}";

            Rectangle rect1 = new Rectangle();
            rect1.Width = 20;
            rect1.Height = 20;
            rect1.Fill = Brushes.ForestGreen;


            panel.Children.Add(rect1);
            panel.Children.Add(output);
            panel.Children.Add(new StackPanel());
               
        }

 
        public void SetValue(PingReply pong, APing ip)
        {

            if (pong != null)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate {

                    StackPanel panel = (StackPanel)listBox1.Items[listBox1.Items.Count - 1];
                    TextBlock block = new List<TextBlock>(panel.Children.OfType<TextBlock>()).FirstOrDefault(s => s.Name == $"box_{ip.ThreadId.ToString()}");
                  
                    block.Text = $" {ip.Ip} : {APing.ReplyText(pong)} \t\t";
                    Rectangle rect = (Rectangle)panel.Children[0];
                    if (rect.Fill == Brushes.ForestGreen)
                    {
                        rect.Fill = Brushes.Yellow;
                        rect.RenderSize = new Size(20, 20);
                    }
                    else
                    {
                        rect.Fill = Brushes.ForestGreen;
                        rect.RenderSize = new Size(40, 40);
                    }

                });

            }
        }
    }
}
