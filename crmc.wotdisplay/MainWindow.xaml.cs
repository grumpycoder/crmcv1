using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            // configure and update display settings
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


            // Setup Hub Connection
            connection = new HubConnection(WebServer + "/signalr");

            //Make proxy to hub based on hub name on server
            myHub = connection.CreateHubProxy("crmcHub");

            connection.StateChanged += ConnectionOnStateChanged;
            connection.Closed += ConnectionOnClosed;
            await connection.Start();

            // Initialize Default Application Settings
            InitializeDefaultSettings();

            InitializeAudioSettings();
            manager.Play();

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
                    var displayLocalTask = Task.Factory.StartNew(() => DisplayWidgetLocalNamesAsync(widget1), cancelToken);
                    //DisplayWidgetLocalNamesAsync(widget1);
                }
            }, cancelToken);

            Log.Debug("Finished Startup");
        }


        private async Task DisplayWidgetLocalNamesAsync(Widget widget)
        {
            while (true)
            {
                var speed = ((Settings.Default.ScrollSpeed / (double)Settings.Default.MinFontSize) * ScreenSpeedModifier).ToInt() / 2;
                var delay = widget.IsPriorityList ? 30 : Settings.Default.AddNewItemSpeed;

                // Special loop scenario for locally added names from kiosks
                var localItemsToRemove = new List<LocalItem>();
                foreach (var localItem in widget.LocalList.LocalItems)
                {
                    if (localItem.LastTickTime >= speed)
                    {
                        Log.Debug("Display local name " + localItem.Person.FullName);
                        await AnimateDisplayNameToUI(localItem.Person, widget.Quadrant, cancelToken);
                        localItem.RotationCount += 1;
                        localItem.LastTickTime = 0;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(delay), cancelToken);
                    if (localItem.RotationCount > 3) localItemsToRemove.Add(localItem);
                    localItem.LastTickTime += Settings.Default.AddNewItemSpeed;
                }
                foreach (var localItem in localItemsToRemove)
                {
                    widget.LocalList.LocalItems.Remove(localItem);
                    Log.Debug("Removing " + localItem.Person.Firstname);
                }
            }
        }

        private async Task DisplayWidgetAsync(Widget widget)
        {
            while (true)
            {
                var speed = ((Settings.Default.ScrollSpeed / (double)Settings.Default.MinFontSize) * ScreenSpeedModifier).ToInt() / 2;
                var delay = widget.IsPriorityList ? 30 : Settings.Default.AddNewItemSpeed;

                foreach (var person in widget.PersonList)
                {
                    if (widget.IsPriorityList) Log.Debug("Displaying Priority: " + person.FullName);

                    await AnimateDisplayNameToUI(person, widget.Quadrant, cancelToken);
                    await Task.Delay(TimeSpan.FromSeconds(delay), cancelToken);
                }
                var temp = widget.PersonList.ToList();
                widget.PersonList = new List<Person>();
                widget.PersonList = await repository.Get(25, widget.IsPriorityList);
                if (!widget.PersonList.Any()) widget.PersonList = temp;

                //    .ContinueWith(task =>
                //{
                //    //If unable to get new list of people from repo set to last know list
                //    //return widget.PersonList = temp;
                //}, cancelToken);

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
            myHub.On<string, Person>("nameAddedToWall", (kiosk, person) => Dispatcher.Invoke(() => AddPersonToDisplayFromKiosk(kiosk, person)));

            Log.Info("Setting up listeners on hub for configuration changes.");
            myHub.On("configSettingsSaved", InitializeDefaultSettings);
        }

        private void ConnectionOnClosed()
        {
            Log.Info("Connection closed. Starting new hub and connecting");
            //Start new connection. Don't know why connection will not reconnect after disconnecting. 
            connection.Stop();
            connection = null;
            connection = new HubConnection(WebServer + "/signalr");
            myHub = connection.CreateHubProxy("crmcHub");
            connection.StateChanged += ConnectionOnStateChanged;
            connection.Closed += ConnectionOnClosed;

            connection.Start().ContinueWith(task =>
            {
                if (!task.IsFaulted) return;
                Log.Warn("Error occurred connecting to hub");
                Log.Warn(task.Exception);
            });
        }

        private void ConnectionOnStateChanged(StateChange stateChange)
        {
            Log.Info("ConnectionState Changed from : {0} to : {1}", stateChange.OldState, stateChange.NewState);

            if (stateChange.NewState == ConnectionState.Connected)
            {
                SetupHubListeners();
            }


        }

        public async void InitializeDefaultSettings()
        {
            var configUrl = WebServer + "/breeze/public/appconfigs";
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
            });
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

        //<< --OLD CODE BELOW HERE -->>

        public async Task AnimateDisplayNameToUI(Person person, int quadrant, CancellationToken token, bool isFromKiosk = false)
        {
            await Task.Delay(1, token);

            if (person == null) return;

            // Create a name scope for the page.
            Dispatcher.Invoke(() =>
            {
                NameScope.SetNameScope(this, new NameScope());

                // Set label properties and register

                var labelFontSize = CalculateFontSize(person.IsPriority);
                if (isFromKiosk) labelFontSize = Settings.Default.MaxFontSize * 2;

                var name = "label" + RandomNumber(1, 1000);
                var label = new Label
                {
                    Content = person.ToString(),
                    FontSize = labelFontSize,
                    FontFamily = new FontFamily(Settings.Default.FontFamily),
                    Name = name,
                    Tag = name,
                    Uid = name,
                    Foreground = new SolidColorBrush(RandomColor())
                };
                RegisterName(name, label);

                // Set label position
                var rightMargin = quadrant == 0 ? canvasWidth.ToInt() : (canvasWidth / 4 * quadrant).ToInt();
                var leftMargin = (rightMargin - quadSize).ToInt();
                if (quadrant == 0) leftMargin = 0;

                // Required to calculate actual size to determine overflow off viewable area
                label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                label.Arrange(new Rect(label.DesiredSize));
                var labelActualWidth = label.ActualWidth;


                var labelLeftPosition = RandomNumber(leftMargin, rightMargin);
                //                if (isFromKiosk)
                //                {
                //                    labelLeftPosition = (((canvasWidth.ToInt() / 4) - labelActualWidth) / 2).ToInt();
                //                    Log.Warn("FromKiosk label left " + labelLeftPosition);
                //                }
                //                else
                //                {
                if (labelLeftPosition + labelActualWidth > canvasWidth)
                {
                    labelLeftPosition = RandomNumber(leftMargin, (canvasWidth - labelActualWidth).ToInt());
                }
                //                    Log.Warn("Not FromKiosk label left " + labelLeftPosition);
                //                }

                // Set label animation
                var labelScrollSpeed = ((Settings.Default.ScrollSpeed / label.FontSize) * ScreenSpeedModifier).ToInt();
                var fallAnimation = new DoubleAnimation
                {
                    From = TopMargin,
                    To = canvasHeight,
                    BeginTime = TimeSpan.FromSeconds(0),
                    Duration = new Duration(TimeSpan.FromSeconds(labelScrollSpeed))
                };

                Storyboard.SetTargetName(fallAnimation, label.Name);
                Storyboard.SetTargetProperty(fallAnimation, new PropertyPath(TopProperty));

                var storyboard = new Storyboard();
                storyboard.Children.Add(fallAnimation);

                var e = new AnimationEventArgs { TagName = label.Uid };
                storyboard.Completed += (sender, args) => StoryboardOnCompleted(e);
                Canvas.SetLeft(label, labelLeftPosition);

                Canvas.SetTop(label, TopMargin);
                canvas.Children.Add(label);
                canvas.UpdateLayout();
                storyboard.Begin(this);

            });


        }




        public void AddNewNameToDisplay(Person person, int quadrant)
        {
            //var minFontSize = Settings.Default.MinFontSize + (Settings.Default.MinFontSize * .10).ToInt();
            var minFontSize = Settings.Default.MaxFontSize;
            var maxFontSize = Settings.Default.MaxFontSize * 2;
            var speed = ((Settings.Default.ScrollSpeed / (double)minFontSize) * ScreenSpeedModifier).ToInt();

            Dispatcher.Invoke(() =>
            {
                // Create a name scope for the page.
                NameScope.SetNameScope(this, new NameScope());
                var quadWidth = (canvasWidth / 4).ToInt();

                var rightMargin = (canvasWidth / 4 * quadrant).ToInt();
                var leftMargin = (rightMargin - quadSize).ToInt();
                //                var left = RandomNumber(leftMargin, rightMargin);

                var midPoint = canvasHeight / 4;
                var labelName = "label" + RandomNumber(1, 1000);

                //Set inital size of label to max for calculation of max label size
                //Reset to 1 after calculating size for grow animation
                var myLabel = new Label
                {
                    Content = person.ToString(),
                    FontSize = maxFontSize,
                    FontFamily = new FontFamily(Settings.Default.FontFamily),
                    Name = labelName,
                    Tag = labelName,
                    Uid = labelName,
                    Foreground = new SolidColorBrush(Colors.White)
                };
                RegisterName(myLabel.Name, myLabel);

                // Correct left position if name is too long to fit within canvas right margin
                myLabel.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                myLabel.Arrange(new Rect(myLabel.DesiredSize));
                var w = myLabel.ActualWidth;

                var offset = (quadWidth - w) / 2;
                var left = quadWidth * (quadrant - 1) + offset;

                if ((left + w) > quadWidth)
                {
                    if (quadrant == 1) left = 0;

                    if (quadrant > 1) left = rightMargin - w;
                }

                //Reset font size to begin growAnimation. 
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

                var fontSize = Settings.Default.MaxFontSize;

                var shrinkAnimation = new DoubleAnimation
                {
                    From = maxFontSize,
                    To = fontSize,
                    BeginTime = TimeSpan.FromSeconds(10),
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
                //myStoryboard.Children.Add(colorAnimation);
                //myStoryboard.Children.Add(upAnimation);
                myStoryboard.Children.Add(fallAnimation);

                Canvas.SetLeft(myLabel, left);

                Canvas.SetTop(myLabel, midPoint);
                Panel.SetZIndex(myLabel, 9999);
                canvas.Children.Add(myLabel);
                canvas.UpdateLayout();

                myStoryboard.Begin(this);
            });
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

        private async Task Animate(Person person, int quadrant, CancellationToken cancellationToken, bool random = true)
        {
            await Task.Delay(1, cancellationToken);

            Dispatcher.Invoke(() =>
            {
                NameScope.SetNameScope(this, new NameScope());

                var startTimer = 0;
                var growTime = 3;
                var shrinkTime = 5;

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
                    startTimer += growTime + 20;

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

                }

                // Set label animation
                var labelScrollSpeed = ((Settings.Default.ScrollSpeed / label.FontSize) * ScreenSpeedModifier).ToInt();

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
            });
        }

        //Invoked from SignalR event
        public async void AddPersonToDisplayFromKiosk(string location, Person person)
        {
            int quad;

            int.TryParse(location, out quad);
            await Animate(person, quad, cancelToken, false);

            var widget = Widgets.FirstOrDefault(x => x.Quadrant == quad);

            if (widget != null)
                widget.LocalList.LocalItems.Add(new LocalItem()
                {
                    Kiosk = quad,
                    Person = person,
                    RotationCount = 0
                });
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
