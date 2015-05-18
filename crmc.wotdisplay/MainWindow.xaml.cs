using crmc.wotdisplay.helpers;
using crmc.wotdisplay.Infrastructure;
using crmc.wotdisplay.Properties;
using Microsoft.AspNet.SignalR.Client;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AutoMapper;
using crmc.wotdisplay.models;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
//using Person = crmc.domain.Person;
using Size = System.Windows.Size;

namespace crmc.wotdisplay
{
    public partial class MainWindow
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
//        protected ApplicationDbContext db;

        #region Variables

        private static readonly Random Random = new Random();

        private const int TopMargin = 10;
        public static int CurrentTotal;
        public static int CurrentPriorityTotal;
        public static int CurrentSkipLimit;
        public static int CurrentPrioritySkipLimit;

        private const int ListSize = 25;
        private int skipCount;


        private readonly Canvas canvas;
        private readonly double canvasWidth;
        private readonly double canvasHeight;
        private readonly double quadSize;
        private const double SpeedModifier = 10;

        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly List<Widget> widgets = new List<Widget>();
        public AppConfig config;
        private MediaManager manager;

        private readonly List<Color> colors = new List<Color>()
        {
            Color.FromRgb(223, 239, 224),
            Color.FromRgb(247, 240, 246), 
            Color.FromRgb(229, 245, 255), 
            Color.FromRgb(255, 254, 230), 
            Color.FromRgb(255, 243, 234)
        }; 

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Mapper.CreateMap<Result, Person>();
            Mapper.CreateMap<Person, Result>();
            config = new AppConfig();
            // Connect to hub to listen for new names to display
            // Add other hub listeners here
            var webServer = "http://crmc-test";
            webServer = Settings.Default.WebServerUrl; 
            Debug.WriteLine(webServer);
            var connection = new HubConnection(webServer + "/signalr");

            //Make proxy to hub based on hub name on server
            var myHub = connection.CreateHubProxy("CRMCHub");

            connection.Start().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.WriteLine("There was an error opening the connection:{0}",
                                      task.Exception.GetBaseException());
                }
                else
                {
                    Debug.WriteLine("Connected");
                }

            }).Wait();

            myHub.On<string, string>("nameAddedToWall", (kiosk, name) => Dispatcher.Invoke(() => AddPersonToDisplayFromKiosk(kiosk, name)));

            myHub.On("configSettingsSaved", InitDefaultSettings); 

            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                            typeof(Timeline),
                            new FrameworkPropertyMetadata { DefaultValue = 80 }
                            );

            canvas = wallCanvas;

            canvas.Height = SystemParameters.PrimaryScreenHeight;
            canvas.Width = SystemParameters.PrimaryScreenWidth;
            expanderSettings.Width = SystemParameters.PrimaryScreenWidth;

            canvas.UpdateLayout();

            canvasWidth = canvas.Width;
            canvasHeight = canvas.Height;
            quadSize = canvasWidth / 4;

            InitDefaultSettings();
            InitAudio();

            //db.Persons.Take(400).Load();

            //Initialize widgets
            widgets.Add(new Widget() { ListSize = 25, IsPriorityList = false, SkipCount = 0, SectionSetting = new SectionSetting() { Quadrant = 1 } });
            widgets.Add(new Widget() { ListSize = 25, IsPriorityList = false, SkipCount = skipCount += ListSize, SectionSetting = new SectionSetting() { Quadrant = 2 } });
            widgets.Add(new Widget() { ListSize = 25, IsPriorityList = false, SkipCount = skipCount += ListSize * 2, SectionSetting = new SectionSetting() { Quadrant = 3 } });
            widgets.Add(new Widget() { ListSize = 25, IsPriorityList = false, SkipCount = skipCount += ListSize * 3, SectionSetting = new SectionSetting() { Quadrant = 4 } });
            widgets.Add(new Widget() { ListSize = 25, IsPriorityList = true, SkipCount = 0, SectionSetting = new SectionSetting() { Quadrant = 0 } });

            // Create and start new Timers
            foreach (var widget in widgets)
            {
                if (Settings.Default.UseLocalDataSource)
                {
                    PopulateListFromDb(widget);
                }
                else
                {
                    PopulateListAsync(widget);
                }

                var d = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(Settings.Default.AddNewItemSpeed) };
                var widget1 = widget;
                d.Tick += (s, args) => timer_Tick(d, widget1);
                d.Start();
            }
        }


        private void ConfigSettingsChanged(AppConfig vm)
        {
            Settings.Default.AddNewItemSpeed = vm.AddNewItemSpeed;
            Settings.Default.AudioFilePath = vm.AudioFilePath;
            Settings.Default.ScrollSpeed = vm.ScrollSpeed;
            Settings.Default.FontFamily = vm.FontFamily;
            Settings.Default.HubName = vm.HubName;
            Settings.Default.MaxFontSize = vm.MaxFontSize;
            Settings.Default.MinFontSize = vm.MinFontSize;
            Settings.Default.MaxFontSizeVIP = vm.MaxFontSizeVIP;
            Settings.Default.MinFontSizeVIP = vm.MinFontSizeVIP;
            Settings.Default.WebServerUrl = vm.WebServerURL;
            Settings.Default.UseLocalDataSource = vm.UseLocalDataSource;
            Settings.Default.Volume = vm.Volume;
            Settings.Default.Save();
        }

        async void InitDefaultSettings()
        {

            await Task.Run(() =>
            {
                var url = Settings.Default.WebServerUrl + "/breeze/public/appconfigs"; 
                
                var syncClient = new WebClient();
                var content = syncClient.DownloadString(url);

                // Create the Json serializer and parse the response
                var serializer = new DataContractJsonSerializer(typeof(AppConfig));
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(content)))
                {
                    // deserialize the JSON object using the WeatherData type.
                    config = (AppConfig)serializer.ReadObject(ms);
                }

                if (config == null) return;

                Settings.Default.AddNewItemSpeed = config.AddNewItemSpeed;
                Settings.Default.AudioFilePath = config.AudioFilePath;
                Settings.Default.ScrollSpeed = config.ScrollSpeed;
                Settings.Default.FontFamily = config.FontFamily;
                Settings.Default.HubName = config.HubName;
                Settings.Default.MaxFontSize = config.MaxFontSize;
                Settings.Default.MinFontSize = config.MinFontSize;
                Settings.Default.MaxFontSizeVIP = config.MaxFontSizeVIP;
                Settings.Default.MinFontSizeVIP = config.MinFontSizeVIP;
                Settings.Default.WebServerUrl = config.WebServerURL;
                Settings.Default.Volume = config.Volume;
                Settings.Default.UseLocalDataSource = config.UseLocalDataSource;
                Settings.Default.Save();
            });

        }

        void InitAudio()
        {
            //Check if path to audio exists and has audio files
            if (!Directory.GetFiles(Settings.Default.AudioFilePath).Any(f => f.EndsWith(".mp3"))) return;

            manager = new MediaManager(myMediaElement, Settings.Default.AudioFilePath);

            manager.Play();
            PlayButton.Source = manager.Paused
                ? new BitmapImage(new Uri(@"images\pause.ico", UriKind.Relative))
                : new BitmapImage(new Uri(@"images\play.ico", UriKind.Relative));
            CurrentSongTextBlock.Text = string.Format("{0}: {1}", manager.PlayStatus, manager.CurrentSong.Title);
            manager.ChangeVolume(0.75);


        }

        private async void timer_Tick(object sender, Widget widget)
        {
            var d = (DispatcherTimer)(sender);
            // Reset timer interval in the chance options has changed
            d.Interval = TimeSpan.FromSeconds(Settings.Default.AddNewItemSpeed);

            if (!widget.PersonList.Any())
            {
                if (Settings.Default.UseLocalDataSource)
                {
                    //await Task.FromResult(PopulateListFromDb(widget));
                    PopulateListFromDb(widget);
                }
                else
                {
                    //await Task.FromResult(PopulateListAsync(widget));
                    PopulateListAsync(widget);
                }
                return;
            }


            AddNameToQuadDisplay(widget.CurrentPerson, widget.SectionSetting.Quadrant);
            widget.SetNextPerson();

            if (widget.CurrentPerson != widget.LastPerson) return;

            switch (widget.IsPriorityList)
            {
                case false:
                    widget.SkipCount = CurrentSkipLimit >= CurrentTotal ? 0 : widget.SkipCount += ListSize;
                    CurrentSkipLimit = widget.SkipCount;
                    break;
                case true:
                    widget.SkipCount = CurrentPrioritySkipLimit >= CurrentPriorityTotal
                        ? 0
                        : widget.SkipCount += ListSize * widget.SectionSetting.Quadrant;
                    CurrentPrioritySkipLimit = widget.SkipCount;
                    break;
            }
            //await Task.FromResult(Settings.Default.UseLocalDataSource ? PopulateListFromDb(widget) : PopulateListAsync(widget));

            if (Settings.Default.UseLocalDataSource)
            {
                //await Task.FromResult(PopulateListFromDb(widget));
                PopulateListFromDb(widget);
            }
            else
            {
                //await Task.FromResult(PopulateListAsync(widget));
                PopulateListAsync(widget);
            }
        }

        async void PopulateListAsync(Widget widget)
        {
               await Task.Run(() =>
               {
                   var baseUrl = Settings.Default.WebServerUrl + "/breeze/public/People?$filter=IsPriority%20eq%20{0}&$orderby=Id&$skip={1}&$top={2}&$inlinecount=allpages";

                   var url = string.Format(baseUrl, widget.IsPriorityList.ToString().ToLower(), widget.SkipCount, widget.ListSize);

                   var syncClient = new WebClient();
                   var content = syncClient.DownloadString(url);

                   // Create the Json serializer and parse the response
                   PersonData personData;
                   var serializer = new DataContractJsonSerializer(typeof (PersonData));
                   using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(content)))
                   {
                       // deserialize the JSON object using the PersonData type.
                       personData = (PersonData) serializer.ReadObject(ms);
                   }
                   widget.PersonList = Mapper.Map<List<Result>, List<Person>>(personData.Results);
                   if (widget.IsPriorityList)
                   {
                       CurrentPriorityTotal = personData.InlineCount;
                   }
                   else
                   {
                       CurrentTotal = personData.InlineCount;
                   }
               });
        }

        void PopulateListFromDb(Widget widget)
        {
            if (db.Database.Exists() && db.Persons.Any())
            {
                widget.PersonList =
                    db.Persons.Local.Where(p => p.IsPriority == widget.IsPriorityList)
                        .OrderBy(x => x.Id).Skip(widget.SkipCount).Take(widget.ListSize).OrderBy(o => o.Id);

                //Update count totals while making request
                CurrentTotal = db.Persons.Local.Count();
                CurrentPriorityTotal = db.Persons.Local.Count(x => x.IsPriority == true);
            }
        }

        public void AddNewNameToDisplay(string name, int quadrant)
        {
            var minFontSize = Settings.Default.MinFontSize + (Settings.Default.MinFontSize * .10).ToInt();
            var maxFontSize = Settings.Default.MaxFontSize + (Settings.Default.MaxFontSize * .10).ToInt();
            var speed = ((Settings.Default.ScrollSpeed / (double)minFontSize) * SpeedModifier).ToInt();

            var rightMargin = (canvasWidth / 4 * quadrant).ToInt();
            var leftMargin = (rightMargin - quadSize).ToInt();
            var left = RandomNumber(leftMargin, rightMargin);
            var midPoint = canvasHeight / 2;

            // Create a name scope for the page.
            NameScope.SetNameScope(this, new NameScope());
            var myLabel = new Label { Content = name, FontSize = maxFontSize, Name = "label1", Foreground = new SolidColorBrush(Colors.White) };
            RegisterName(myLabel.Name, myLabel);

            // Correct left position if name is too long to fit within canvas right margin
            myLabel.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            myLabel.Arrange(new Rect(myLabel.DesiredSize));
            var w = myLabel.ActualWidth;
            if ((left + w) > canvasWidth)
            {
                left = left > (canvasWidth - w).ToInt() ? (canvasWidth - w).ToInt() : RandomNumber(leftMargin, (canvasWidth - w).ToInt());
            }

            myLabel.FontSize = 1;

            var growAnimation = new DoubleAnimation
            {
                From = 1,
                To = maxFontSize,
                Duration = new Duration(TimeSpan.FromSeconds(3)),
                BeginTime = TimeSpan.FromSeconds(1)
            };
            Storyboard.SetTargetName(growAnimation, myLabel.Name);
            Storyboard.SetTargetProperty(growAnimation, new PropertyPath(FontSizeProperty));

            var shrinkAnimation = new DoubleAnimation
            {
                From = maxFontSize,
                To = maxFontSize - maxFontSize * .20,
                BeginTime = TimeSpan.FromSeconds(5),
                Duration = new Duration(TimeSpan.FromSeconds(5))
            };
            Storyboard.SetTargetName(shrinkAnimation, myLabel.Name);
            Storyboard.SetTargetProperty(shrinkAnimation, new PropertyPath(FontSizeProperty));

            var upAnimation = new DoubleAnimation
            {
                From = midPoint,
                To = TopMargin,
                BeginTime = TimeSpan.FromSeconds(5),
                Duration = new Duration(TimeSpan.FromSeconds(5))
            };
            Storyboard.SetTargetName(upAnimation, myLabel.Name);
            Storyboard.SetTargetProperty(upAnimation, new PropertyPath(TopProperty));

            var mySolidColorBrush = new SolidColorBrush { Color = Colors.White };
            RegisterName("mySolidColorBrush", mySolidColorBrush);

            myLabel.Foreground = mySolidColorBrush;

            var colorAnimation = new ColorAnimation
            {
                From = RandomColor(),
                To = RandomColor(),
                BeginTime = TimeSpan.FromSeconds(5),
                Duration = new Duration(TimeSpan.FromSeconds(5))
            };
            Storyboard.SetTargetName(colorAnimation, "mySolidColorBrush");
            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath(SolidColorBrush.ColorProperty));


            var fallAnimation = new DoubleAnimation
            {
                From = midPoint,
                To = canvasHeight,
                BeginTime = TimeSpan.FromSeconds(15),
                Duration = new Duration(TimeSpan.FromSeconds(speed))
            };
            Storyboard.SetTargetName(fallAnimation, myLabel.Name);
            Storyboard.SetTargetProperty(fallAnimation, new PropertyPath(TopProperty));

            var myStoryboard = new Storyboard();
            myStoryboard.Children.Add(growAnimation);
            myStoryboard.Children.Add(shrinkAnimation);
            myStoryboard.Children.Add(colorAnimation);
            //myStoryboard.Children.Add(upAnimation);
            myStoryboard.Children.Add(fallAnimation);

            Canvas.SetLeft(myLabel, left);

            Canvas.SetTop(myLabel, midPoint);
            Panel.SetZIndex(myLabel, 9999);
            canvas.Children.Add(myLabel);
            canvas.UpdateLayout();

            myStoryboard.Begin(this);

        }

        async void AddNameToQuadDisplay(Person person, int quadrant)
        {
            await Task.Run(() =>
            {
                if (person == null) return;

                var fontSize = CalculateFontSize(person.IsPriority);
                var name = "label" + RandomNumber(1, 1000);

                // Create a name scope for the page.
                Dispatcher.Invoke(() =>
                {
                    NameScope.SetNameScope(this, new NameScope());

                    var label = new Label
                    {
                        Content = person.ToString(),
                        FontSize = fontSize,
                        FontFamily = new FontFamily(Settings.Default.FontFamily),
                        Name = name,
                        Tag = name,
                        Uid = name,
                        Foreground = new SolidColorBrush(RandomColor())
                    };
                    var speed = ((Settings.Default.ScrollSpeed / label.FontSize) * SpeedModifier).ToInt();
                    RegisterName(label.Name, label);

                    var rightMargin = quadrant == 0 ? canvasWidth.ToInt() : (canvasWidth / 4 * quadrant).ToInt();
                    var leftMargin = (rightMargin - quadSize).ToInt();
                    if (quadrant == 0)
                    {
                        leftMargin = 0;
                    }

                    // Required to calculate actual size to determine overflow off viewable area
                    label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    label.Arrange(new Rect(label.DesiredSize));

                    var width = label.ActualWidth;
                    var leftPos = RandomNumber(leftMargin, rightMargin);
                    if (leftPos + width > canvasWidth)
                    {
                        leftPos = RandomNumber(leftMargin, (canvasWidth - width).ToInt());
                    }

                    var fallAnimation = new DoubleAnimation
                    {
                        From = TopMargin,
                        To = canvasHeight,
                        BeginTime = TimeSpan.FromSeconds(0),
                        Duration = new Duration(TimeSpan.FromSeconds(speed))
                    };

                    Storyboard.SetTargetName(fallAnimation, label.Name);
                    Storyboard.SetTargetProperty(fallAnimation, new PropertyPath(TopProperty));

                    var storyboard = new Storyboard();
                    storyboard.Children.Add(fallAnimation);

                    var e = new AnimationEventArgs { TagName = label.Uid };
                    storyboard.Completed += (sender, args) => StoryboardOnCompleted(e);
                    Canvas.SetLeft(label, leftPos);

                    Canvas.SetTop(label, TopMargin);
                    canvas.Children.Add(label);
                    canvas.UpdateLayout();

                    storyboard.Begin(this);
                });
            });

        }

        private int CalculateFontSize(bool? isPriority)
        {
            var maxFontSize = isPriority.GetValueOrDefault() ? Settings.Default.MaxFontSizeVIP : Settings.Default.MaxFontSize;
            var minFontSize = isPriority.GetValueOrDefault() ? Settings.Default.MinFontSizeVIP : Settings.Default.MinFontSize;
            return RandomNumber(minFontSize, maxFontSize);
        }


        public void AddPersonToDisplayFromKiosk(string location, string name)
        {
            int quad;
            int.TryParse(location, out quad);
            AddNewNameToDisplay(name, quad);
        }

        public int RandomNumber(int min, int max)
        {
            if (max <= min) min = max - 1;
            return Random.Next(min, max);
        }

        private Color RandomColor()
        {
//            var r = Convert.ToByte(RandomNumber(0, 128) + 127);
//            var g = Convert.ToByte(RandomNumber(0, 128) + 127);
//            var b = Convert.ToByte(RandomNumber(0, 128) + 127);
//            var color = Color.FromRgb(r, g, b);
//            return color;

            var color = colors[RandomNumber(0, 5)];
            return color;
        }

        #region Events

        private void StoryboardOnCompleted(AnimationEventArgs eventArgs)
        {
            var tagName = eventArgs.TagName;

            foreach (UIElement child in canvas.Children)
            {
                if (tagName != child.Uid) continue;
                child.BeginAnimation(TopProperty, null);
                canvas.Children.Remove(child);
                return;
            }
        }

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            expanderSettings.IsExpanded = false;

            config.AddNewItemSpeed = Settings.Default.AddNewItemSpeed;
            config.AudioFilePath = Settings.Default.AudioFilePath;
            config.ScrollSpeed = Settings.Default.ScrollSpeed;
            config.FontFamily = Settings.Default.FontFamily;
            config.HubName = Settings.Default.HubName;
            config.MaxFontSize = Settings.Default.MaxFontSize;
            config.MinFontSize = Settings.Default.MinFontSize;
            config.MaxFontSizeVIP = Settings.Default.MaxFontSizeVIP;
            config.MinFontSizeVIP = Settings.Default.MinFontSizeVIP;
            config.WebServerURL = Settings.Default.WebServerUrl;
            config.Volume = Settings.Default.Volume;
            config.UseLocalDataSource = Settings.Default.UseLocalDataSource;


//            const string url =
//               "http://localhost/crmc/breeze/Breeze/savechanges";
//
//            var syncClient = new WebClient();
//            var content = syncClient.UploadValues(url, config);
//
//            // Create the Json serializer and parse the response
//            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AppConfig));
//            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(content)))
//            {
//                // deserialize the JSON object using the WeatherData type.
//                config = (AppConfig)serializer.ReadObject(ms);
//            }

            var client = new RestClient(Settings.Default.WebServerUrl);
            var request = new RestRequest("api/configuration/SaveConfiguration", Method.POST) { RequestFormat = RestSharp.DataFormat.Json };
            request.AddBody(config);

            client.ExecuteAsync(request, response =>
            {
                if (response.StatusCode != HttpStatusCode.NoContent)
                {

                }
            });


//            var connection = new HubConnection("http://localhost/crmc/signalr");
            var connection = new HubConnection(Settings.Default.WebServerUrl +  "/signalr");

            //Make proxy to hub based on hub name on server
            var myHub = connection.CreateHubProxy("CRMCHub");

            connection.Start().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.WriteLine("There was an error opening the connection:{0}",
                                      task.Exception.GetBaseException());
                }
                else
                {
                    Debug.WriteLine("Connected");
                }

            }).Wait();
            
            myHub.Invoke<AppConfig>("SaveConfigSettings", config);
//            ConnectionManager.HubProxy.Invoke("SaveConfigSettings", config);
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();
            Settings.Default.Reload();
        }

        private void BtnCloseApp_OnClick(object sender, RoutedEventArgs e)
        {
            //Logger.Trace("Wall application closing");
            //Conn.Dispose();
            if (manager != null)
            {
                manager.Stop();
            }
            mainWindow.Close();
        }

        #endregion

        #region Media Events

        private void Element_MediaEnded(object sender, RoutedEventArgs e)
        {
            manager.Next();
            CurrentSongTextBlock.Text = string.Format("{0}: {1}", manager.PlayStatus, manager.CurrentSong.Title);
            manager.Play();
        }

        private void OnMouseDownPlayMedia(object sender, MouseButtonEventArgs e)
        {
            if (manager == null)
            {
                CurrentSongTextBlock.Text = "Directory does not exists or contain audio files";
                return;
            }
            manager.Play();

            PlayButton.Source = manager.Paused
                ? new BitmapImage(new Uri(@"images\pause.ico", UriKind.Relative))
                : new BitmapImage(new Uri(@"images\play.ico", UriKind.Relative));
            CurrentSongTextBlock.Text = string.Format("{0}: {1}", manager.PlayStatus, manager.CurrentSong.Title);
        }

        private void OnMouseDownStopMedia(object sender, MouseButtonEventArgs e)
        {
            if (manager == null)
            {
                CurrentSongTextBlock.Text = "Directory does not exists or contain audio files";
                return;
            }

            manager.Stop();
            CurrentSongTextBlock.Text = string.Format("{0}: {1}", manager.PlayStatus, manager.CurrentSong.Title);
            PlayButton.Source = new BitmapImage(new Uri(@"images\play.ico", UriKind.Relative));
        }

        private void OnMouseDownForwardMedia(object sender, MouseButtonEventArgs e)
        {
            if (manager == null)
            {
                CurrentSongTextBlock.Text = "Directory does not exists or contain audio files";
                return;
            }

            manager.Next();
            manager.Play();
            CurrentSongTextBlock.Text = string.Format("{0}: {1}", manager.PlayStatus, manager.CurrentSong.Title);
        }

        private void OnMouseDownBackMedia(object sender, MouseButtonEventArgs e)
        {
            if (manager == null)
            {
                CurrentSongTextBlock.Text = "Directory does not exists or contain audio files";
                return;
            }

            manager.Prev();
            manager.Play();
            CurrentSongTextBlock.Text = string.Format("{0}: {1}", manager.PlayStatus, manager.CurrentSong.Title);
        }

        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (manager != null)
            {
                manager.ChangeVolume(VolumeSlider.Value);
            }
        }

        private void OnMouseDownRefreshMedia(object sender, MouseButtonEventArgs e)
        {
            if (!Directory.GetFiles(Settings.Default.AudioFilePath).Any(f => f.EndsWith(".mp3")))
            {
                CurrentSongTextBlock.Text = "Directory does not exists or contain audio files";
                return;
            }

            manager.RefreshPlaylist();
        }

        #endregion

    }

}
