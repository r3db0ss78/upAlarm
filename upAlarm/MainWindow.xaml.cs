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
    /// </summary>
    public partial class MainWindow : Window
    {
       
        List<APing> ips = new List<APing>();
        List<ComboBoxItem> list = new List<ComboBoxItem>();
        List<ComboBoxItem> listWeb = new List<ComboBoxItem>();
        StackPanel stackPanel1 = new StackPanel();

        public MainWindow()
        {
            InitializeComponent();
            
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
                if (UserInput.Text.ToString().Length > 0)
                {
                    StartPings(APing.GetHostName(UserInput.Text.ToString()));
                }
            }
        }

        private void Ping_Click(object sender, RoutedEventArgs e)
        {
            if (UserInput.Text.ToString().Length > 0)
            {
                StartPings(APing.GetHostName(UserInput.Text.ToString()));
            }
        }

        public async void StartPings(string hostname)
        {
            UserMsg("");
            try
            {
                IPHostEntry hostEntry = await Dns.GetHostEntryAsync(hostname);
               
                if (hostEntry.AddressList.Length > 0)
                {
                    TextBlock output = new TextBlock();
                    output.TextAlignment = TextAlignment.Center;
                    output.Text = " " + hostname + " : ...";


                    Rectangle rect1 = new Rectangle();
                    rect1.Width = 20;
                    rect1.Height = 20;
                    rect1.Fill = Brushes.ForestGreen;


                    StackPanel panel = new StackPanel();
                    panel.Orientation = Orientation.Horizontal;
                    panel.Children.Add(rect1);
                    panel.Children.Add(output);


                    listBox1.Items.Add(panel);

                    ips.Add(new APing(hostname));
                    DoPings();
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

        public async void DoPings()
        {
            for (int i=0;i<ips.Count();i++)
            {
                if (!ips[i].IsRunning)
                {
                    try
                    {
                        await Task.Run(delegate
                        {

                            Thread th = new Thread(() => DoPing(ips[i], i));
                            th.SetApartmentState(ApartmentState.STA);
                            th.IsBackground = true;
                            th.Start();
                            ips[i].ThreadId = th.ManagedThreadId;

                        });
                        ips[i].IsRunning = true;
                    }
                   catch(Exception e)
                    {
                        UserMsg(e.Message);
                        GC.Collect();
                    }
                }
                
            }
        }

        public void  DoPing(APing aping, int number)
        {
                   
            Ping ping = new Ping();
                try
                {
                    PingOptions options = new PingOptions(aping.Ttl, aping.DontFragment);
                    System.Timers.Timer timer = new System.Timers.Timer(aping.FrequencyMs);
                    timer.Elapsed +=  new ElapsedEventHandler(async delegate{

                    PingReply pong = await ping.SendPingAsync(aping.Ip, aping.TimeoutMs, aping.Buffer, options);
                    SetValue(pong, aping, number);

                        
                    });
                    timer.Start();

                }
                catch (Exception e)
                {
                   UserMsg(e.Message);
                   GC.Collect();
                }
        }

        public void UserMsg(string msg)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                errorMsg.Text = msg;
            });   
        }

        


        public void SetValue(PingReply pong, APing ip, int number)
        {

            if (pong != null)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate {

                    StackPanel panel = (StackPanel)listBox1.Items[number];
                    TextBlock block = (TextBlock)panel.Children[1];
                    block.Text = " " + ip.Ip + " : " + APing.ReplyText(pong)+"\t";
                    Rectangle rect= (Rectangle)panel.Children[0];
                    if (rect.Fill == Brushes.ForestGreen)
                    {
                        rect.Fill = Brushes.Yellow;
                        rect.RenderSize=new Size(20, 20);
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
