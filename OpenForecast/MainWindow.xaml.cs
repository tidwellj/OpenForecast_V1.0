using ExtentionMethods;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Drawing;
using System.IO;
using System.Net.Mail;
using System.Security;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace OpenForecast
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///

    public partial class MainWindow : Window
    {
        //Context menu for notifiicon
        private System.Windows.Forms.ContextMenu contextMenu1;

        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem2;
        public static string zip;
        public static string currentLat;
        public static string currentLon;
        public static List<string> emailList = new List<string>(); 
        public static int emailAlerts;
        public static string currentlat2;
        public static string currentlon2;
        public static string city;
        //PLace email information here
        public static string emailAddress = "User email address here";
        public static string emailUser = "Username here";
        public static string emailPass = "User password here";

        //Place Openweathermap api key here.
        public static string apiKey = "API key here";

        //Atempt to make key more secure by using SecureString for encryption.
        public static SecureString SecureKey = apiKey.Secure();
        public static SecureString SecureEmailAddress = emailAddress.Secure();
        public static SecureString SecureEmailUser = emailUser.Secure();
        public static SecureString SecureEmailPass = emailPass.Secure();
        public static string zipCode;

        private GeoCoordinateWatcher Watcher = null;

        // Create JSON variables
        private static string json;

        private static string json2;
        private static string json3;
        private static string json4;
        private bool run = true;
        private DispatcherTimer timer = new DispatcherTimer();
        private DispatcherTimer timer2 = new DispatcherTimer();

        private NotifyIcon notify;
        private static string temperature2;
        public static List<int> alertNumber = new List<int>();
        private static string unitLetter = "°F";
        private static string units = "imperial";
        private static string pressureLetters = " inHg";
        private static string currentPressure;
        private static string windSpeed = " mph";

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            //Create Timers for WIndows Location Services
            //Create Timers for syncing JSON data

            timer = new DispatcherTimer();

            timer.Interval = TimeSpan.FromMinutes(10);
            timer.Tick += Timer_Tick;
            timer.Start();

            //Create GeoCoordinateWatcher for Windows Location Services

            Window window = new Window();

            window.Closed += new EventHandler(MainWindow_Closed);

            Watcher = new GeoCoordinateWatcher();

            //Checks movement location in 10 mile increments as not to sync too often.
            Watcher.MovementThreshold = 16093.4;
            // Start the watcher.

            Watcher.Start();
            // Catch the StatusChanged event.
            Watcher.StatusChanged += Watcher_StatusChanged;

            //On window close remove notifyicon completely.
            Closed += new EventHandler(MainWindow_Closed);
            //store email data in binary file
            if (File.Exists("mydata"))
            {
                BinaryReader br;

                br = new BinaryReader(new FileStream("mydata", FileMode.Open));

                string json = br.ReadString();

                string[] items = JsonConvert.DeserializeObject<string[]>(json);
                //int pos = 0;
                for (int i = 0; i < items.Length; i++)
                {
                    emailList.Add(items[i]);
                    emails.Items.Add(items[i]);
                }

                br.Close();
            }

            //Store units in binary file

            if (File.Exists("mydata2"))
            {
                BinaryReader br;

                br = new BinaryReader(new FileStream("mydata2", FileMode.Open));

                units = br.ReadString();
                unitLetter = br.ReadString();
                pressureLetters = br.ReadString();

                br.Close();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            //Start Weather data syncing every 10 minutes

            SyncWeather();
            //Refresh tray icon
            TrayIco(temperature2);
            //Refresh city name
            GetCity(currentlat2, currentlon2);
        }
        //Main function for syncing data
        private string SyncWeather()
        {
            try
            {
                //Retrieve JSON data

                var client = new RestClient("https://api.openweathermap.org/data/2.5/onecall?lat=" + currentLat + "&lon="
                     + currentLon + "&units=" + units + "&appid=" + SecureKey.Unsecure());

                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);

                //Convert JSON Data to readable array data
                json = response.Content;

                dynamic obj = JsonConvert.DeserializeObject(json);
                //Set current day variables
                string feelsLike = obj.current.feels_like;
                string sunrise = obj.current.sunrise + obj.timezone_offset;
                string sunset = obj.current.sunset + obj.timezone_offset;
                string humidity = obj.current.humidity;
                string visibility = obj.current.visibility;
                string pressure = obj.current.pressure;
                string dewPoint = obj.current.dew_point;
                string clouds = obj.current.weather[0].description;
                string windspeed = obj.current.wind_speed;

                string rain;
                if (obj.current.rain == null)
                {
                    rain = "0.0";
                }
                else
                {
                    rain = obj.SelectToken("current.rain.1h".ToString());
                }

                //Seven day tab
                //Convert from unix epoch time
                DateTimeOffset dateTimeOffsetTimeDay1 = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(obj.daily[0].dt));
                DateTime dateTimeDay1 = dateTimeOffsetTimeDay1.DateTime.ToLocalTime();

                DateTimeOffset dateTimeOffsetTimeDay2 = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(obj.daily[1].dt));
                DateTime dateTimeDay2 = dateTimeOffsetTimeDay2.DateTime.ToLocalTime();

                DateTimeOffset dateTimeOffsetTimeDay3 = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(obj.daily[2].dt));
                DateTime dateTimeDay3 = dateTimeOffsetTimeDay3.DateTime.ToLocalTime();

                DateTimeOffset dateTimeOffsetTimeDay4 = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(obj.daily[3].dt));
                DateTime dateTimeDay4 = dateTimeOffsetTimeDay4.DateTime.ToLocalTime();

                DateTimeOffset dateTimeOffsetTimeDay5 = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(obj.daily[4].dt));
                DateTime dateTimeDay5 = dateTimeOffsetTimeDay5.DateTime.ToLocalTime();

                DateTimeOffset dateTimeOffsetTimeDay6 = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(obj.daily[5].dt));
                DateTime dateTimeDay6 = dateTimeOffsetTimeDay6.DateTime.ToLocalTime();

                DateTimeOffset dateTimeOffsetTimeDay7 = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(obj.daily[6].dt));
                DateTime dateTimeDay7 = dateTimeOffsetTimeDay7.DateTime.ToLocalTime();

                //Convert from unix epoch time hourly

                DateTimeOffset dateTimeOffsetHour1 = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(obj.hourly[0].dt));

                DateTime dateHour1 = dateTimeOffsetTimeDay1.DateTime.ToLocalTime();

                DateTimeOffset dateTimeOffSet = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(sunrise));
                DateTime dateTime = dateTimeOffSet.DateTime;
                DateTimeOffset dateTimeOffSet2 = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(sunset));
                DateTime dateTime2 = dateTimeOffSet2.DateTime;

                //Check if units are imperial or metric
                if (units == "imperial")
                {
                    Pressure1.Content = "Pressure: " + Math.Round((float.Parse(pressure) / 33.864), 2) + " inHg";
                    Rain_label.Content = "precip: " + rain + " in/h";
                    Windspeed_Label.Content = "Wind: " + windspeed + " mph";
                }
                else if (units == "metric")
                {
                    Pressure1.Content = "Pressure: " + Math.Round(float.Parse(pressure), 2) + " hPa";
                    Rain_label.Content = "precip: " + rain + " mm/h";
                    Windspeed_Label.Content = "Wind: " + windspeed + " m/s";
                }

                DewPoint.Content = "Dew Point: " + Math.Round(float.Parse(dewPoint)) + unitLetter;
                SunRise.Content = "Sunrise: " + dateTime.ToShortTimeString();
                SunSet1.Content = "Sunset: " + dateTime2.ToShortTimeString();
                Humidity1.Content = "Humidity: " + humidity + "%";

                CurrentConditionsLablel.Content = clouds;

                //Current Temperature
                string temperature = obj.current.temp;

                string CurrentDay(string x)
                {
                    if (x == null)
                    {
                        x = "0.000";
                    }
                    else
                    {
                        x = temperature2 = Math.Round(float.Parse(temperature)).ToString();
                    }
                    return x;
                }
                Current_Temp_Text.Content = CurrentDay(temperature);

                //Assign cloud icon on current weather page
                string cloudIcon = obj.current.weather[0].icon;

                //Change icon from Default provided by API
                CurrentWeatherImage.Source = new BitmapImage(new Uri(@"pack://application:,,,/images/" + cloudIcon + ".png"));

                //Determine seven day icons
                string day1 = obj.daily[0].weather[0].icon;
                string day2 = obj.daily[1].weather[0].icon;
                string day3 = obj.daily[2].weather[0].icon;
                string day4 = obj.daily[3].weather[0].icon;
                string day5 = obj.daily[4].weather[0].icon;
                string day6 = obj.daily[5].weather[0].icon;
                string day7 = obj.daily[6].weather[0].icon;

                //Replace seven day icons provided with API

                Day1.Source = new BitmapImage(new Uri(@"pack://application:,,,/images/" + day1 + ".png"));
                Day2.Source = new BitmapImage(new Uri(@"pack://application:,,,/images/" + day2 + ".png"));
                Day3.Source = new BitmapImage(new Uri(@"pack://application:,,,/images/" + day3 + ".png"));
                Day4.Source = new BitmapImage(new Uri(@"pack://application:,,,/images/" + day4 + ".png"));
                Day5.Source = new BitmapImage(new Uri(@"pack://application:,,,/images/" + day5 + ".png"));
                Day6.Source = new BitmapImage(new Uri(@"pack://application:,,,/images/" + day6 + ".png"));
                Day7.Source = new BitmapImage(new Uri(@"pack://application:,,,/images/" + day7 + ".png"));

                //First label row in 7 day forecast tab

                Day1Label.Content = dateTimeDay1.ToString("dddd");
                Day2Label.Content = dateTimeDay2.ToString("dddd");
                Day3Label.Content = dateTimeDay3.ToString("dddd");
                Day4Label.Content = dateTimeDay4.ToString("dddd");
                Day5Label.Content = dateTimeDay5.ToString("dddd");
                Day6Label.Content = dateTimeDay6.ToString("dddd");
                Day7Label.Content = dateTimeDay7.ToString("dddd");
                //Second label row in 7 day forecast

                string day1max = obj.daily[0].temp.max;
                string day1maxb = Math.Round(float.Parse(day1max)).ToString();
                string day1min = obj.daily[0].temp.min;
                string day1minb = Math.Round(float.Parse(day1min)).ToString();

                string day2max = obj.daily[1].temp.max;
                string day2maxb = Math.Round(float.Parse(day2max)).ToString();
                string day2min = obj.daily[1].temp.min;
                string day2minb = Math.Round(float.Parse(day2min)).ToString();

                string day3max = obj.daily[2].temp.max;
                string day3maxb = Math.Round(float.Parse(day3max)).ToString();
                string day3min = obj.daily[2].temp.min;
                string day3minb = Math.Round(float.Parse(day3min)).ToString();

                string day4max = obj.daily[3].temp.max;
                string day4maxb = Math.Round(float.Parse(day4max)).ToString();
                string day4min = obj.daily[3].temp.min;
                string day4minb = Math.Round(float.Parse(day4min)).ToString();

                string day5max = obj.daily[4].temp.max;
                string day5maxb = Math.Round(float.Parse(day5max)).ToString();
                string day5min = obj.daily[4].temp.min;
                string day5minb = Math.Round(float.Parse(day5min)).ToString();

                string day6max = obj.daily[5].temp.max;
                string day6maxb = Math.Round(float.Parse(day6max)).ToString();
                string day6min = obj.daily[5].temp.min;
                string day6minb = Math.Round(float.Parse(day6min)).ToString();

                string day7max = obj.daily[6].temp.max;
                string day7maxb = Math.Round(float.Parse(day7max)).ToString();
                string day7min = obj.daily[6].temp.min;
                string day7minb = Math.Round(float.Parse(day7min)).ToString();

                string day1per = obj.daily[0].rain;
                string day2per = obj.daily[1].rain;
                string day3per = obj.daily[2].rain;
                string day4per = obj.daily[3].rain;
                string day5per = obj.daily[4].rain;
                string day6per = obj.daily[5].rain;
                string day7per = obj.daily[6].rain;

                string day1hum = obj.daily[0].humidity;
                string day2hum = obj.daily[1].humidity;
                string day3hum = obj.daily[2].humidity;
                string day4hum = obj.daily[3].humidity;
                string day5hum = obj.daily[4].humidity;
                string day6hum = obj.daily[5].humidity;
                string day7hum = obj.daily[6].humidity;

                //Current day min and max
                CurrentMax.Content = "Max: " + day1maxb + unitLetter;

                CurrentMin.Content = "Min: " + day1minb + unitLetter;

                //Check to see if JSON element is null which happens occasionally and causes crashes

                string Days(string x)
                {
                    if (x == null)
                    {
                        x = "0.000";
                    }
                    else
                    {
                        x = Math.Round(float.Parse(x) / 25, 3).ToString();
                    }
                    if (units == "imperial")
                    {
                        x = x + " in";
                    }
                    else if (units == "metric")
                    {
                        x = x + " mm";
                    }

                    return x;
                }
                day1per = Days(day1per);
                day2per = Days(day2per);
                day3per = Days(day3per);
                day4per = Days(day4per);
                day5per = Days(day5per);
                day6per = Days(day6per);
                day7per = Days(day7per);

                //Seven day day 1 sunrise/sunset
                string sunrise7day1 = obj.daily[0].sunrise + obj.timezone_offset;

                string sunset7day1 = obj.daily[0].sunset + obj.timezone_offset;
                DateTimeOffset sevendayoffset1a = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(sunrise7day1));
                DateTime day1a = sevendayoffset1a.DateTime;

                DateTimeOffset sevendayoffset1b = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(sunset7day1));
                DateTime day1b = sevendayoffset1b.DateTime;

                //seven day day 2 sunrise/sunset
                string sunrise7day2 = obj.daily[1].sunrise + obj.timezone_offset;

                string sunset7day2 = obj.daily[1].sunset + obj.timezone_offset;
                DateTimeOffset sevendayoffset2a = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(sunrise7day2));
                DateTime day2a = sevendayoffset2a.DateTime;

                DateTimeOffset sevendayoffset2b = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(sunset7day2));
                DateTime day2b = sevendayoffset2b.DateTime;

                //seven day day 3 sunrise/sunset
                string sunrise7day3 = obj.daily[2].sunrise + obj.timezone_offset;

                string sunset7day3 = obj.daily[2].sunset + obj.timezone_offset;
                DateTimeOffset sevendayoffset3a = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(sunrise7day3));
                DateTime day3a = sevendayoffset3a.DateTime;

                DateTimeOffset sevendayoffset3b = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(sunset7day3));
                DateTime day3b = sevendayoffset3b.DateTime;

                //seven day day 4 sunrise/sunset
                string sunrise7day4 = obj.daily[3].sunrise + obj.timezone_offset;

                string sunset7day4 = obj.daily[3].sunset + obj.timezone_offset;
                DateTimeOffset sevendayoffset4a = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(sunrise7day4));
                DateTime day4a = sevendayoffset4a.DateTime;

                DateTimeOffset sevendayoffset4b = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(sunset7day4));
                DateTime day4b = sevendayoffset4b.DateTime;

                //seven day day 5 sunrise/sunset
                string sunrise7day5 = obj.daily[4].sunrise + obj.timezone_offset;

                string sunset7day5 = obj.daily[4].sunset + obj.timezone_offset;
                DateTimeOffset sevendayoffset5a = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(sunrise7day5));
                DateTime day5a = sevendayoffset5a.DateTime;

                DateTimeOffset sevendayoffset5b = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(sunset7day5));
                DateTime day5b = sevendayoffset5b.DateTime;

                //seven day day 6 sunrise/sunset
                string sunrise7day6 = obj.daily[5].sunrise + obj.timezone_offset;

                string sunset7day6 = obj.daily[5].sunset + obj.timezone_offset;
                DateTimeOffset sevendayoffset6a = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(sunrise7day6));
                DateTime day6a = sevendayoffset6a.DateTime;

                DateTimeOffset sevendayoffset6b = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(sunset7day6));
                DateTime day6b = sevendayoffset6b.DateTime;

                //seven day day 7 sunrise/sunset
                string sunrise7day7 = obj.daily[6].sunrise + obj.timezone_offset;

                string sunset7day7 = obj.daily[6].sunset + obj.timezone_offset;
                DateTimeOffset sevendayoffset7a = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(sunrise7day7));
                DateTime day7a = sevendayoffset7a.DateTime;

                DateTimeOffset sevendayoffset7b = DateTimeOffset.FromUnixTimeSeconds(Int32.Parse(sunset7day7));
                DateTime day7b = sevendayoffset7b.DateTime;

                // Day1Labelb.Content = day1maxb + "/" + day1minb + unitLetter + " Precipitation: " + day1per + " in";
                Info_Label.Content = string.Format("{0,0} {1,55} {2,30} {3,30} {4, 20 }", "Day", "Temp", "Precip", "Humidity", "Sunrise/Sunset");

                Day1Labelb.Content = string.Format("{0,0} {1,15} {2,10} {3, 20}", day1maxb + "°/" + day1minb + unitLetter, day1per, day1hum + "%", day1a.ToShortTimeString() + "/" + day1b.ToShortTimeString());
                Day2Labelb.Content = string.Format("{0,0} {1,15} {2,10} {3, 20}", day2maxb + "°/" + day2minb + unitLetter, day2per, day2hum + "%", day2a.ToShortTimeString() + "/" + day2b.ToShortTimeString());
                Day3Labelb.Content = string.Format("{0,0} {1,15} {2,10} {3, 20}", day3maxb + "°/" + day3minb + unitLetter, day3per, day3hum + "%", day3a.ToShortTimeString() + "/" + day3b.ToShortTimeString());
                Day4Labelb.Content = string.Format("{0,0} {1,15} {2,10} {3, 20}", day4maxb + "°/" + day4minb + unitLetter, day4per, day4hum + "%", day4a.ToShortTimeString() + "/" + day4b.ToShortTimeString());
                Day5Labelb.Content = string.Format("{0,0} {1,15} {2,10} {3, 20}", day5maxb + "°/" + day5minb + unitLetter, day5per, day5hum + "%", day5a.ToShortTimeString() + "/" + day5b.ToShortTimeString());
                Day6Labelb.Content = string.Format("{0,0} {1,15} {2,10} {3, 20}", day6maxb + "°/" + day6minb + unitLetter, day6per, day6hum + "%", day6a.ToShortTimeString() + "/" + day6b.ToShortTimeString());
                Day7Labelb.Content = string.Format("{0,0} {1,15} {2,10} {3, 20}", day7maxb + "°/" + day7minb + unitLetter, day7per, day7hum + "%", day7a.ToShortTimeString() + "/" + day7b.ToShortTimeString());

                //Hourly tab
                int HourTemp(int x)
                {
                    string y = obj.hourly[x].temp;
                    y = Math.Round(float.Parse(y)).ToString();
                    return int.Parse(y);
                }

                int HourHumidity(int x)
                {
                    //int y = 0.0

                    int y = obj.hourly[x].humidity;

                    return y;
                }
                int WindSpeed(int x)
                {
                    int y = obj.hourly[x].wind_speed;

                    return y;
                }
                int HourPressure(int x)
                {
                    string y = "0.00";
                    float z = 0;
                    if (obj.hourly == null)
                    {
                        y = "0.00";
                    }
                    else
                    {
                        y = obj.hourly[x].pressure;

                        if (units == "imperial")
                        {
                            y = Math.Round(float.Parse(y) / 33.864).ToString();
                            //y = y;
                        }
                        else if (units == "metric")
                        {
                            y = Math.Round(float.Parse(y)).ToString();
                        }

                        // System.Windows.MessageBox.Show(y);
                        x = int.Parse(y);
                    }
                    return x;
                }

                Paragraph textTitle2 = new Paragraph();
                richTextBox2.Document.Blocks.Clear();
                //Check to see if JSON contains hourly which can cause a crash if not present
                Hourlykey.Content = string.Format("{0,15} {1,15} {2,35} {3,33} {4, 25}", "Hour", "Temp", "Pressure", "Humidity", "Wind speed");

                if (json.Contains("hourly"))
                {
                    int c = obj.hourly.Count;

                    StreamWriter sw = new StreamWriter(@"alerts2.txt");
                    if (units == "imperial")
                    {
                        windspeed = " mph";
                    }
                    else if (units == "metric")
                    {
                        windspeed = " m/s";
                    }

                    for (int i = 0; i < c; i++)

                    {
                        sw.WriteLine(string.Format("{0,1}{1,7} {2,7} {3, 15} {4,10} {5,15}", " ", DateTime.Now.AddHours(i).ToString("h tt"), HourTemp(i).ToString() + unitLetter, HourPressure(i) + pressureLetters, HourHumidity(i) + "%", WindSpeed(i) + windspeed + Environment.NewLine));
                    }
                    sw.Close();

                    string text2 = File.ReadAllText(@"alerts2.txt");
                    textTitle2.Inlines.Add(text2);
                    richTextBox2.Document.Blocks.Add(textTitle2);
                }
                else
                {
                    textTitle2.Inlines.Add("No alerts");
                    richTextBox2.Document.Blocks.Add(textTitle2);
                }

                //Alerts tab
                Paragraph textTitle = new Paragraph();
                richTextBox1.Document.Blocks.Clear();
                if (json.Contains("alerts"))
                {
                    // Alert_Icon.Visibility = Visibility.Visible;
                    int b = obj.alerts.Count;

                    StreamWriter sw = new StreamWriter(@"alerts.txt");

                    for (int i = 0; i < b; i++)

                    {
                        sw.WriteLine(obj.alerts[i].sender_name + Environment.NewLine + Environment.NewLine);

                        //Event is reserved keyword. Use @ to acess JSON
                        sw.WriteLine(obj.alerts[i].@event + Environment.NewLine + Environment.NewLine);

                        sw.WriteLine(obj.alerts[i].description + Environment.NewLine);
                        //Alert Emailer
                        emailAlerts = obj.alerts[i].start;
                        //Check if email alerts are present and prevent duplicate emails being sent
                        if (alertNumber.Contains(emailAlerts))
                        {
                            Console.WriteLine("Alert already sent");
                        }
                        else if (emailList.Count >= 1)
                        {
                            alertNumber.Add(emailAlerts);
                            //Send emails to each email address in list
                            for (int j = 0; j < emailList.Count; j++)
                            {
                                string text4 = obj.alerts[i].description;
                                MailMessage mail = new MailMessage();
                                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                                mail.From = new MailAddress(SecureEmailAddress.Unsecure());
                                
                                mail.To.Add(emailList[j]);
                                mail.Subject = "Weather Alert";
                                mail.Body = text4;

                                SmtpServer.Port = 587;
                                SmtpServer.Credentials = new System.Net.NetworkCredential(SecureEmailUser.Unsecure(), SecureEmailPass.Unsecure());
                                SmtpServer.EnableSsl = true;

                                SmtpServer.Send(mail);
                            }
                        }
                        else
                        {
                            Console.WriteLine("No Alerts");
                        }
                    }
                    sw.Close();
                    //Output alerts to alert tab
                    string text = File.ReadAllText(@"alerts.txt");
                    textTitle.Inlines.Add(text);
                    richTextBox1.Document.Blocks.Add(textTitle);
                }
                else
                {
                    StreamWriter sw = new StreamWriter(@"alerts.txt");
                    sw.WriteLine("No alerts");
                    sw.Close();
                    //  Alert_Icon.Visibility = Visibility.Hidden;

                    textTitle.Inlines.Add("No alerts");
                    richTextBox1.Document.Blocks.Add(textTitle);
                }
                //Clear list when alerts are not present
                if (!json.Contains("alerts"))
                {
                    alertNumber.Clear();
                }
            }
            catch (Exception)
            {
                // Prevent crash and notify user that network is not present

                Location.Content = "No network";
                city = "none";
            }
            return temperature2;
        }

        //Create Notifyicon in system tray
        private void TrayIco(string trayTemp)
        {
            trayTemp = temperature2 + "°";
            string firstText = trayTemp;

            PointF firstLocation = new PointF(-1.5f, 2f);

            string path = System.IO.Path.GetTempPath();

            Bitmap bitmap = new Bitmap(System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/images/tray.png", UriKind.RelativeOrAbsolute)).Stream);

            //Render current temperature on notify tray icon by using overlay
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                using (Font arialFont = new Font("Arial", 325))
                {
                    graphics.DrawString(firstText, arialFont, System.Drawing.Brushes.Black, firstLocation);
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                }
            }

            IntPtr Hicon = bitmap.GetHicon();

            // Create a new icon from the handle.
            Icon newIcon = System.Drawing.Icon.FromHandle(Hicon);

            if (run)
            {
                notify = new NotifyIcon();

                notify.Icon = newIcon;

                notify.Text = temperature2 + unitLetter;
                notify.Visible = true;

                notify.MouseDoubleClick += OnNotifyIconDoubleClick;

                contextMenu1 = new System.Windows.Forms.ContextMenu();

                menuItem1 = new System.Windows.Forms.MenuItem();
                menuItem2 = new System.Windows.Forms.MenuItem();

                contextMenu1.MenuItems.AddRange(
                            new System.Windows.Forms.MenuItem[] { menuItem1 });
                contextMenu1.MenuItems.AddRange(
                           new System.Windows.Forms.MenuItem[] { menuItem2 });

                menuItem1.Index = 0;

                menuItem1.Text = "Exit";
                menuItem1.Click += new System.EventHandler(menuItem1_Click);

                menuItem2.Index = 1;
                menuItem2.Text = "Show";
                menuItem2.Click += new System.EventHandler(menuItem2_Click);

                notify.ContextMenu = contextMenu1;
            }
            else
            {
                notify.Dispose();

                notify = new NotifyIcon();

                notify.Icon = newIcon;

                notify.Text = "OpenForecast";
                notify.Visible = true;

                notify.MouseDoubleClick += OnNotifyIconDoubleClick;

                contextMenu1 = new System.Windows.Forms.ContextMenu();

                menuItem1 = new System.Windows.Forms.MenuItem();
                menuItem2 = new System.Windows.Forms.MenuItem();

                // Initialize contextMenu1
                contextMenu1.MenuItems.AddRange(
                            new System.Windows.Forms.MenuItem[] { menuItem1 });
                contextMenu1.MenuItems.AddRange(
                           new System.Windows.Forms.MenuItem[] { menuItem2 });

                // Initialize menuItem1
                menuItem1.Index = 0;

                menuItem1.Text = "E&xit";
                menuItem1.Click += new System.EventHandler(menuItem1_Click);

                menuItem2.Index = 1;
                menuItem2.Text = "Show";
                menuItem2.Click += new System.EventHandler(menuItem2_Click);

                notify.ContextMenu = contextMenu1;
            }
            run = false;
        }

        //Doubleclick Notifyicon to make window visible
        private void OnNotifyIconDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            WindowState = WindowState.Normal;
            //Make window appear on top of other windows on doubleclick
            Topmost = true;
            Show();
        }

        // Close the window, which closes the application.

        private void menuItem1_Click(object Sender, EventArgs e)
        {
            Close();
        }

        // Restore window to normal state

        private void menuItem2_Click(object Sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        //Set tab index on button click to currentday
        private void CurrentDay_Click(object sender, RoutedEventArgs e)
        {
            myTabControl.SelectedIndex = 0;
            System.Windows.Media.Color color1 = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF3E3E42");

            System.Windows.Media.Brush SpeedColor = new SolidColorBrush(color1);
            CurrentDayButton.Background = SpeedColor;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            myTabControl.SelectedIndex = 1;
        }

        // The geocoordinate watcher's status has change. See if it is ready.
        private void Watcher_StatusChanged(object sender,
            GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Ready)
            {
                if (Watcher.Position.Location.IsUnknown)
                {
                    Console.WriteLine("Cannot find location data");
                }
                else
                {
                    GeoCoordinate location =
                        Watcher.Position.Location;
                    string currentLat2 = location.Latitude.ToString();
                    string currentLon2 = location.Longitude.ToString();

                    GetCity(currentLat2, currentLon2);

                    SyncWeather();
                    TrayIco(temperature2);
                    Watcher.Stop();

                    Watcher.Dispose();
                }
            }
        }

        //Get the city name by reverse geocode from latitude and longitude
        private void GetCity(string x, string y)
        {
            try
            {
                var client2 = new RestClient("http://api.openweathermap.org/geo/1.0/reverse?lat=" + x + "&lon=" + y + "&limit=1&appid=" + SecureKey.Unsecure());

                var request2 = new RestRequest(Method.GET);
                IRestResponse response = client2.Execute(request2);

                json2 = response.Content;
                dynamic obj2 = JsonConvert.DeserializeObject(json2);

                if (obj2[0].state == null)
                {
                    city = obj2[0].name + ", " + obj2[0].country;
                }
                else
                {
                    city = obj2[0].name + ", " + obj2[0].state + ", " + obj2[0].country;
                }
                UnitLabel.Content = unitLetter;
                currentLat = obj2[0].lat;
                currentLon = obj2[0].lon;
                Location.Content = city;
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to get location");
            }
        }

        //Shut down application completely so it does not hang in debugger
        //and dispose of notifyicon so it does not remain in system tray after
        //application close
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
            notify.Visible = false;
            // notify.Dispose();
            // Watcher.Stop();
        }

        //Select hourly tab on button click
        private void hourlyButton_Click(object sender, RoutedEventArgs e)
        {
            myTabControl.SelectedIndex = 3;
        }

        //Select alerts tab on button click
        private void alertsButton_Click(object sender, RoutedEventArgs e)
        {
            myTabControl.SelectedIndex = 2;
        }

        //Enter location though zip code or city, state, and country codes
        private void ZipCodeButton_Click(object sender, RoutedEventArgs e)
        {
            string locationNames = ZipCodeEntry.Text;

            //Determine if location is input as zip or location codes
            if (locationNames.Length == 5 && !locationNames.Contains(","))
            {
                try
                {
                    var client3 = new RestClient("http://api.openweathermap.org/data/2.5/weather?zip=" + locationNames + "&appid=" + SecureKey.Unsecure());

                    var request3 = new RestRequest(Method.GET);
                    IRestResponse response3 = client3.Execute(request3);

                    json3 = response3.Content;
                    dynamic obj3 = JsonConvert.DeserializeObject(json3);
                    currentLat = obj3.coord.lat;
                    currentLon = obj3.coord.lon;

                    city = obj3.name + ", " + obj3.sys.country;
                    UnitLabel.Content = unitLetter;
                    Location.Content = city;
                    SyncWeather();
                    TrayIco(temperature2);
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("Location not found!");
                }
            }
            else
            {
                try
                {
                    var client4 = new RestClient("https://api.openweathermap.org/data/2.5/weather?q=" + locationNames + "&appid=" + SecureKey.Unsecure());

                    var request4 = new RestRequest(Method.GET);
                    IRestResponse response4 = client4.Execute(request4);

                    json4 = response4.Content;
                    dynamic obj4 = JsonConvert.DeserializeObject(json4);

                    currentLat = obj4.coord.lat;
                    currentLon = obj4.coord.lon;


                    city = obj4.name + ", " + obj4.sys.country;

                    UnitLabel.Content = unitLetter;

                    Location.Content = city;
                    SyncWeather();
                    TrayIco(temperature2);
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("Location not found!");
                }
            }
        }

        //Settings icon click
        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            myTabControl.SelectedIndex = 4;
        }

        //Add email to list
        private void emailSubmit_Click(object sender, RoutedEventArgs e)
        {
            string email = emailEntry.Text;
            if (email != null)
            {
                emailList.Add(email);

                emails.Items.Add(email);

                Stream stream = new FileStream("mydata", FileMode.Create);

                BinaryWriter binWriter = new BinaryWriter(stream);
                string listAdd = Newtonsoft.Json.JsonConvert.SerializeObject(emailList);
                binWriter.Write(listAdd);

                binWriter.Close();
                stream.Close();
            }
        }

        //Remove email from list
        private void remove_Click(object sender, RoutedEventArgs e)
        {
            if (emails.SelectedItem != null)
            {
                string selected = emails.SelectedItem.ToString();

                emails.Items.RemoveAt(emails.Items.IndexOf(emails.SelectedItem));
                int indexed = emails.SelectedIndex;
                emailList.Remove(selected);

                Stream stream = new FileStream("mydata", FileMode.Create);
                BinaryWriter binWriter = new BinaryWriter(stream);
                BinaryWriter bw = new BinaryWriter(stream);

                string listAdd = Newtonsoft.Json.JsonConvert.SerializeObject(emailList);
                bw.Write(listAdd);

                bw.Close();
                stream.Close();
            }
        }

        //Set units to imperial or metric
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if ((bool)Fahrenheit.IsChecked)
            {
                units = "imperial";
                unitLetter = "°F";
                UnitLabel.Content = unitLetter;
                pressureLetters = " inHg";
                SyncWeather();
                TrayIco(temperature2);
            }
            else if ((bool)Celsius.IsChecked)
            {
                units = "metric";
                unitLetter = "°C";
                UnitLabel.Content = unitLetter;
                pressureLetters = " hPa";
                SyncWeather();
                TrayIco(temperature2);
            }
            //store units in binary file
            Stream stream = new FileStream("mydata2", FileMode.Create);

            BinaryWriter binWriter = new BinaryWriter(stream);

            binWriter.Write(units);
            binWriter.Write(unitLetter);
            binWriter.Write(pressureLetters);

            binWriter.Close();
            stream.Close();
        }
    }
}