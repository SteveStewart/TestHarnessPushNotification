using System;
using System.Collections.Generic;
using System.Linq;
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
using Newtonsoft.Json.Linq;
using PushSharp.Apple;

namespace TestHarnessPushNotification
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.txtResult.Text = "Starting...";

            var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, "TestPushNotification.p12", "Vicki1234");

            // Create a new broker
            var apnsBroker = new ApnsServiceBroker(config);

            // Wire up events
            apnsBroker.OnNotificationFailed += (notification, aggregateEx) => {

                aggregateEx.Handle(ex => {

                    // See what kind of exception it was to further diagnose
                    if (ex is ApnsNotificationException)
                    {
                        var notificationException = (ApnsNotificationException)ex;

                        // Deal with the failed notification
                        var apnsNotification = notificationException.Notification;
                        var statusCode = notificationException.ErrorStatusCode;

                        txtResult.Text +=
                            $"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}" + Environment.NewLine;

                        //Console.WriteLine($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");

                    }
                    else
                    {
                        // Inner exception might hold more useful information like an ApnsConnectionException           
                        txtResult.Text += $"Apple Notification Failed for some unknown reason : {ex.InnerException}" + Environment.NewLine;


                        //Console.WriteLine($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
                    }

                    // Mark it as handled
                    return true;
                });
            };

            apnsBroker.OnNotificationSucceeded += (notification) => {
                txtResult.Text += "Apple Notification Sent!" + Environment.NewLine;
                //Console.WriteLine("Apple Notification Sent!");
            };

            // Start the broker
            apnsBroker.Start();
            var testSubject = this.txtMessage.Text + " - " + DateTime.Now;

            string appleJsonFormat = "{\"aps\": {\"alert\":" + '"' + testSubject + '"' + ",\"sound\": \"default\", \"badge\":7}}";
            //foreach (var deviceToken in MY_DEVICE_TOKENS)
            //{
            // Queue a notification to send
            apnsBroker.QueueNotification(new ApnsNotification
                {  
                    //DeviceToken = "cfdb7e72b8a4be79f526014cf13be9301570d573265e5da2c1830374cd3aa358",
                    DeviceToken = "1c1dead5bc044bae9663642bf0c94ea4cf6018b6fe8f7a4e8cfe82a7d05ed139",
                //Payload = JObject.Parse("{\"aps\":{\"badge\":7}}")

                Payload = JObject.Parse(appleJsonFormat)

            });
            //}

            // Stop the broker, wait for it to finish   
            // This isn't done after every message, but after you're
            // done with the broker

            txtResult.Text += "Finished sending!" + Environment.NewLine;
            apnsBroker.Stop();

        }
    }
}
