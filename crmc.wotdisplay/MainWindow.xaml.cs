using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using crmc.wotdisplay.helpers;
using crmc.wotdisplay.Infrastructure;
using crmc.wotdisplay.models;
using crmc.wotdisplay.Properties;
using Microsoft.AspNet.SignalR.Client;
using NLog;
//TODO: Refactor out RestSharp. Use httpclient
using RestSharp;
using DataFormat = RestSharp.DataFormat;

namespace crmc.wotdisplay
{

    public partial class MainWindow
    {
        #region Display Variables

        private Canvas canvas;
        private double canvasWidth;
        private double canvasHeight;
        private double quadSize;
        private const int TopMargin = 10;

        #endregion

        #region Variables

        public string WebServer;
        private readonly MediaManager manager;
        private readonly List<Widget> Widgets;
        private readonly PersonRepository repository;
        private readonly CancellationToken cancelToken = new CancellationToken();
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public AppConfig appConfig;
        private readonly List<Color> colors;
        private const double ScreenSpeedModifier = 10;
        private HubConnection connection;
        private IHubProxy myHub;

        private int PriorityListItemDelay = 10;

        //TODO: Refactor
        private static readonly Random Random = new Random();

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Log.Info("Application Startup");
            WebServer = Settings.Default.WebServerUrl;
            appConfig = new AppConfig();
            manager = new MediaManager(MediaPlayer, Settings.Default.AudioFilePath);
            repository = new PersonRepository(WebServer);
            Widgets = new List<Widget>();

            colors = new List<Color>()
            {
                Color.FromRgb(205, 238, 207),
                Color.FromRgb(247, 231, 245),
                Color.FromRgb(213, 236, 250),
                Color.FromRgb(246, 244, 207),
                Color.FromRgb(246, 227, 213)
            };

            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            Init();

            //Create display widgets one for each quadrant
            for (var i = 1; i < 5; i++)
            {
                Widgets.Add(new Widget() { IsPriorityList = false, Quadrant = i });
            }
            //priority name widget
            Widgets.Add(new Widget() { IsPriorityList = true, Quadrant = 0 });

            //Begin recursive display of widget names
            await LoadWidgetsAsync().ContinueWith((t) =>
            {
                foreach (var widget in Widgets)
                {
                    var widget1 = widget;
                    var displayTask = Task.Factory.StartNew(() => DisplayWidgetAsync(widget1), cancelToken);
                    var displayPriorityTask = Task.Factory.StartNew(() => DisplayPriorityWidgetAsync(widget1), cancelToken);
                    var displayLocalTask = Task.Factory.StartNew(() => DisplayWidgetLocalNamesAsync(widget1), cancelToken);
                    //DisplayWidgetLocalNamesAsync(widget1);
                }
            }, cancelToken);

            Log.Debug("Finished Startup");
        }

        private void Init()
        {
            ConfigureDisplay();

            ConfigureConnectionManager();

            InitializeDefaultSettings();

            InitializeAudioSettings();

            manager.Play();
        }

        private async Task DisplayPriorityWidgetAsync(Widget widget)
        {
            while (true)
            {
                if (!widget.IsPriorityList) break;

                var speed = Settings.Default.ScrollSpeed;
                var delay = PriorityListItemDelay;

                if (widget.IsPriorityList)
                {
                    foreach (var person in widget.PersonList)
                    {
                        Log.Debug("Displaying Priority: " + person);
                        await Animate(person, widget.Quadrant, cancelToken);
                        await Task.Delay(TimeSpan.FromSeconds(delay), cancelToken);
                    }
                }

            }
        }

        private async Task DisplayWidgetLocalNamesAsync(Widget widget)
        {
            while (true)
            {
                var speed = ((Settings.Default.ScrollSpeed / (double)Settings.Default.MinFontSize) * ScreenSpeedModifier).ToInt() / 2;
                var delay = 20;

                foreach (var localItem in widget.LocalList.LocalItems.ToList())
                {
                    Log.Debug("Display local name " + localItem.Person.FullName);
                    await Animate(localItem.Person, widget.Quadrant, cancelToken);
                    localItem.RotationCount += 1;
                    await Task.Delay(TimeSpan.FromSeconds(delay), cancelToken);
                    if (localItem.RotationCount > 3) widget.LocalList.LocalItems.Remove(localItem);
                }
            }
        }

        private async Task DisplayWidgetAsync(Widget widget)
        {
            while (true)
            {
                var speed = ((Settings.Default.ScrollSpeed / (double)Settings.Default.MinFontSize) * ScreenSpeedModifier).ToInt() / 2;
                var delay = Settings.Default.AddNewItemSpeed;

                if (widget.IsPriorityList) break;
                foreach (var person in widget.PersonList)
                {
                    await Animate(person, widget.Quadrant, cancelToken);
                    await Task.Delay(TimeSpan.FromSeconds(delay), cancelToken);
                }
                var temp = widget.PersonList.ToList();
                widget.PersonList = new List<Person>();
                widget.PersonList = await repository.Get(25, widget.IsPriorityList);
                if (!widget.PersonList.Any()) widget.PersonList = temp;

            }
        }

        public async Task LoadWidgetsAsync()
        {
            foreach (var widget in Widgets)
            {
                widget.PersonList = await repository.Get(25, widget.IsPriorityList);
            }
        }

        private static async Task<AppConfig> DownloadDefaultSettings(string url)
        {
            var config = await Downloader.DownloadConfigDataAsync(url);
            return config;
        }

        private void SetupHubListeners()
        {
            Log.Info("Setting up listeners on hub for kiosk names added.");
            ConnectionManager.HubProxy.On<string, Person>("nameAddedToWall", (kiosk, person) => Dispatcher.Invoke(() => AddPersonToDisplayFromKiosk(kiosk, person)));

            Log.Info("Setting up listeners on hub for configuration changes.");
            ConnectionManager.HubProxy.On("configSettingsSaved", InitializeDefaultSettings);
        }

        public async void InitializeDefaultSettings()
        {
            var configApiUrl = "http://localhost/crmc/breeze/public/configurations";
            var configUrl = WebServer + "/breeze/public/appconfigs";


            await SettingsManager.LoadAsync(configApiUrl);
            Log.Debug("WallConfig {0}", SettingsManager.WallConfiguration);
            Log.Debug(SettingsManager.WallConfiguration);

            await DownloadDefaultSettings(configUrl).ContinueWith(async (r) =>
            {
                var result = await r;
                appConfig = result;
                Settings.Default.AddNewItemSpeed = appConfig.AddNewItemSpeed;
                Settings.Default.AudioFilePath = appConfig.AudioFilePath;
                Settings.Default.ScrollSpeed = appConfig.ScrollSpeed;
                Settings.Default.FontFamily = appConfig.FontFamily;
                Settings.Default.HubName = appConfig.HubName;
                Settings.Default.MaxFontSize = appConfig.MaxFontSize;
                Settings.Default.MinFontSize = appConfig.MinFontSize;
                Settings.Default.MaxFontSizeVIP = appConfig.MaxFontSizeVIP;
                Settings.Default.MinFontSizeVIP = appConfig.MinFontSizeVIP;
                Settings.Default.WebServerUrl = appConfig.WebServerURL;
                Settings.Default.Volume = appConfig.Volume;
                Settings.Default.UseLocalDataSource = appConfig.UseLocalDataSource;
                Settings.Default.Save();
            }, cancelToken);
        }

        void InitializeAudioSettings()
        {
            Log.Info("Initializing Audio settings.");
            //Check if path to audio exists and has audio files
            if (!Directory.GetFiles(Settings.Default.AudioFilePath).Any(f => f.EndsWith(".mp3"))) return;



            PlayButton.Source = manager.Paused
                ? new BitmapImage(new Uri(@"images\pause.ico", UriKind.Relative))
                : new BitmapImage(new Uri(@"images\play.ico", UriKind.Relative));
            CurrentSongTextBlock.Text = string.Format("{0}: {1}", manager.PlayStatus, manager.CurrentSong.Title);
            manager.ChangeVolume(Settings.Default.Volume);
            Log.Info("Audio initialization complete.");
        }

        private int CalculateFontSize(bool? isPriority)
        {
            var maxFontSize = isPriority.GetValueOrDefault() ? Settings.Default.MaxFontSizeVIP : Settings.Default.MaxFontSize;
            var minFontSize = isPriority.GetValueOrDefault() ? Settings.Default.MinFontSizeVIP : Settings.Default.MinFontSize;
            return RandomNumber(minFontSize, maxFontSize);
        }

        private Label CreateLabel(Person person)
        {
            var labelFontSize = CalculateFontSize(person.IsPriority);
            var name = "label" + Guid.NewGuid().ToString("N").Substring(0, 10);
            var label = new Label()
            {
                Content = person.ToString(),
                FontSize = labelFontSize,
                FontFamily = new FontFamily(appConfig.FontFamily),
                HorizontalAlignment = HorizontalAlignment.Center,
                Name = name,
                Tag = name,
                Uid = name,
                Foreground = new SolidColorBrush(RandomColor())
            };

            return label;
        }

        private async Task<double> Animate(Person person, int quadrant, CancellationToken cancellationToken, bool random = true)
        {
            var totalTime = 0.0;
            await Dispatcher.InvokeAsync(() =>
             {
                 NameScope.SetNameScope(this, new NameScope());

                 var startTimer = 5;
                 var growTime = 3;
                 var shrinkTime = 3;
                 var pauseTime = 1;
                 var fallAnimationOffset = -2;

                 var label = CreateLabel(person);
                 label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                 label.Arrange(new Rect(label.DesiredSize));
                 var labelActualWidth = random ? label.ActualWidth : quadSize;

                 RegisterName(label.Name, label);

                 var borderName = "border" + Guid.NewGuid().ToString("N").Substring(0, 10);

                 var border = new Border()
                 {
                     Name = borderName,
                     Uid = borderName,
                     Child = label,
                     Width = labelActualWidth,
                     HorizontalAlignment = HorizontalAlignment.Center
                 };
                 RegisterName(borderName, border);


                 // Set label position
                 var rightMargin = quadrant == 0 ? canvasWidth.ToInt() : (canvasWidth.ToInt().Quarter() * quadrant);
                 var leftMargin = (rightMargin - quadSize).ToInt();
                 if (quadrant == 0) leftMargin = 0;

                 var borderLeftMargin = RandomNumber(leftMargin, rightMargin);
                 if (borderLeftMargin + labelActualWidth > canvasWidth)
                 {
                     borderLeftMargin = RandomNumber(leftMargin, (canvasWidth - labelActualWidth).ToInt());
                 }
                 var storyboard = new Storyboard();
                 if (!random)
                 {
                     label.FontSize = 0.1;
                     borderLeftMargin = leftMargin;
                     border.Width = quadSize;

                     var maxFontSize = appConfig.MaxFontSize * 2;


                     var growAnimation = new DoubleAnimation
                     {
                         From = 0,
                         To = maxFontSize,
                         BeginTime = TimeSpan.FromSeconds(startTimer),
                         Duration = new Duration(TimeSpan.FromSeconds(growTime)),
                     };
                     startTimer += growTime + pauseTime;

                     var fontSize = appConfig.MaxFontSize;

                     var shrinkAnimation = new DoubleAnimation
                     {
                         From = maxFontSize,
                         To = fontSize,
                         BeginTime = TimeSpan.FromSeconds(startTimer),
                         Duration = new Duration(TimeSpan.FromSeconds(shrinkTime))
                     };
                     startTimer += shrinkTime;

                     Storyboard.SetTargetName(growAnimation, label.Name);
                     Storyboard.SetTargetProperty(growAnimation, new PropertyPath(FontSizeProperty));
                     Storyboard.SetTargetName(shrinkAnimation, label.Name);
                     Storyboard.SetTargetProperty(shrinkAnimation, new PropertyPath(FontSizeProperty));

                     storyboard.Children.Add(growAnimation);
                     storyboard.Children.Add(shrinkAnimation);
                     startTimer = startTimer + fallAnimationOffset;
                 }
                 else
                 {
                     startTimer = 0;
                 }

                 // Set label animation
                 var size = random ? label.FontSize : appConfig.MaxFontSize;
                 var labelScrollSpeed = ((Settings.Default.ScrollSpeed / (double)size) * ScreenSpeedModifier).ToInt();
                 var midPoint = canvasHeight.ToInt().Quarter() * 2;

                 var fallAnimation = new DoubleAnimation
                 {
                     From = random ? TopMargin : midPoint,
                     To = canvasHeight,
                     BeginTime = TimeSpan.FromSeconds(startTimer),
                     Duration = new Duration(TimeSpan.FromSeconds(labelScrollSpeed))
                 };

                 Storyboard.SetTargetName(fallAnimation, border.Name);
                 Storyboard.SetTargetProperty(fallAnimation, new PropertyPath(TopProperty));
                 storyboard.Children.Add(fallAnimation);

                 var e = new AnimationEventArgs { TagName = border.Uid };
                 storyboard.Completed += (sender, args) => StoryboardOnCompleted(e);

                 Canvas.SetLeft(border, borderLeftMargin);
                 Canvas.SetTop(border, random ? TopMargin : midPoint);
                 canvas.Children.Add(border);
                 canvas.UpdateLayout();
                 storyboard.Begin(this);

                 totalTime = startTimer + fallAnimation.Duration.TimeSpan.TotalSeconds;
             });
            return totalTime;
        }

        //Invoked from SignalR event
        public async void AddPersonToDisplayFromKiosk(string location, Person person)
        {
            int quad;

            int.TryParse(location, out quad);
            Log.Debug("Sending from kiosk: " + person);

            //await Animate(person, quad, cancelToken, false);
            var time = await Animate(person, quad, cancelToken, false);
            Log.Debug("TotalTime: " + time);
            await Task.Delay(TimeSpan.FromSeconds(time), cancelToken);
            Log.Debug("Should be " + time + " seconds later");


            var widget = Widgets.FirstOrDefault(x => x.Quadrant == quad);
            Log.Debug("Into continue with");
            if (widget != null)
                widget.LocalList.LocalItems.Add(new LocalItem()
                {
                    Kiosk = quad,
                    Person = person,
                    RotationCount = 0
                });

            //await Animate(person, quad, cancelToken, false).ContinueWith((t) =>
            //{
            //    //Wait for first display animation before adding to list to cycle.
            //    var widget = Widgets.FirstOrDefault(x => x.Quadrant == quad);
            //    Log.Debug("Into continue with");
            //    if (widget != null)
            //        widget.LocalList.LocalItems.Add(new LocalItem()
            //        {
            //            Kiosk = quad,
            //            Person = person,
            //            RotationCount = 0
            //        });
            //}, cancelToken);


        }


        //TODO: Refactor
        public int RandomNumber(int min, int max)
        {
            if (max <= min) min = max - 1;
            return Random.Next(min, max);
        }

        private Color RandomColor()
        {
            var color = colors[RandomNumber(0, 5)];
            return color;
        }

        #region Configuration and Init

        private void ConfigureDisplay()
        {
            canvas = wallCanvas;
            canvas.Height = SystemParameters.PrimaryScreenHeight;
            canvas.Width = SystemParameters.PrimaryScreenWidth;
            expanderSettings.Width = SystemParameters.PrimaryScreenWidth;

            canvas.UpdateLayout();

            canvasWidth = canvas.Width;
            canvasHeight = canvas.Height;
            quadSize = canvasWidth / 4;

            //HACK: Test if needed. 
            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 80 }
                );
        }

        private void ConfigureConnectionManager()
        {
            ConnectionManager.Connection.StateChanged += ConnectionOnStateChanged;
            ConnectionManager.Connection.Closed += ConnectionOnClosed;
            ConnectionManager.ConnectAsync();
        }

        #endregion

        #region ConnectionManager Events

        private void ConnectionOnClosed()
        {
            Log.Info("Connection closed. Starting new hub and connecting");
            //Start new connection. Don't know why connection will not reconnect after disconnecting. 
            ConnectionManager.Connection.Stop();
            ConnectionManager.Connection.Start().ContinueWith(task =>
            {
                if (!task.IsFaulted) return;
                Log.Warn("Error occurred connecting to hub");
                Log.Warn(task.Exception);
            }, cancelToken);
        }

        private void ConnectionOnStateChanged(StateChange stateChange)
        {
            Log.Info("ConnectionState Changed from : {0} to : {1}", stateChange.OldState, stateChange.NewState);

            if (stateChange.NewState == ConnectionState.Connected)
            {
                SetupHubListeners();
            }
        }


        #endregion

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

            appConfig.AddNewItemSpeed = Settings.Default.AddNewItemSpeed;
            appConfig.AudioFilePath = Settings.Default.AudioFilePath;
            appConfig.ScrollSpeed = Settings.Default.ScrollSpeed;
            appConfig.FontFamily = Settings.Default.FontFamily;
            appConfig.HubName = Settings.Default.HubName;
            appConfig.MaxFontSize = Settings.Default.MaxFontSize;
            appConfig.MinFontSize = Settings.Default.MinFontSize;
            appConfig.MaxFontSizeVIP = Settings.Default.MaxFontSizeVIP;
            appConfig.MinFontSizeVIP = Settings.Default.MinFontSizeVIP;
            appConfig.WebServerURL = Settings.Default.WebServerUrl;
            appConfig.Volume = Settings.Default.Volume;
            appConfig.UseLocalDataSource = Settings.Default.UseLocalDataSource;
            //TODO: Refactor this
            var client = new RestClient(Settings.Default.WebServerUrl);
            var request = new RestRequest("api/configuration/SaveConfiguration", Method.POST) { RequestFormat = DataFormat.Json };
            request.AddBody(appConfig);

            Log.Info("Saving configuration changes.");
            client.ExecuteAsync(request, response =>
            {
                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    Log.Warn("Unable to save configuration");
                    Log.Warn(response.StatusCode);
                }
            });

            Log.Info("Sending notification to hub of configuration settings updated. ");
            myHub.Invoke<AppConfig>("SaveConfigSettings", appConfig);
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();
            Settings.Default.Reload();
        }

        private void BtnCloseApp_OnClick(object sender, RoutedEventArgs e)
        {
            if (manager != null)
            {
                manager.Stop();
            }
            Log.Info("Application Exited by user control.");
            mainWindow.Close();
        }

        #endregion

        #region Media Events


        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {

        }

        private void Element_MediaEnded(object sender, RoutedEventArgs e)
        {
            manager.Next();
            //CurrentSongTextBlock.Text = string.Format("{0}: {1}", manager.PlayStatus, manager.CurrentSong.Title);
            manager.Play();
        }

        private void OnMouseDownPlayMedia(object sender, MouseButtonEventArgs e)
        {
            if (manager == null)
            {
                //CurrentSongTextBlock.Text = "Directory does not exists or contain audio files";
                return;
            }
            manager.Play();

            //PlayButton.Source = manager.Paused
            //    ? new BitmapImage(new Uri(@"images\pause.ico", UriKind.Relative))
            //    : new BitmapImage(new Uri(@"images\play.ico", UriKind.Relative));
            //CurrentSongTextBlock.Text = string.Format("{0}: {1}", manager.PlayStatus, manager.CurrentSong.Title);
        }

        private void OnMouseDownStopMedia(object sender, MouseButtonEventArgs e)
        {
            if (manager == null)
            {
                //CurrentSongTextBlock.Text = "Directory does not exists or contain audio files";
                return;
            }

            manager.Stop();
            //CurrentSongTextBlock.Text = string.Format("{0}: {1}", manager.PlayStatus, manager.CurrentSong.Title);
            //PlayButton.Source = new BitmapImage(new Uri(@"images\play.ico", UriKind.Relative));
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
