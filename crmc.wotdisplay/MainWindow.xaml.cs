using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AutoMapper;
using crmc.wotdisplay.helpers;
using crmc.wotdisplay.Infrastructure;
using crmc.wotdisplay.models;
using crmc.wotdisplay.Properties;
using Microsoft.AspNet.SignalR.Client;
using NLog;

namespace crmc.wotdisplay
{

    public partial class MainWindow
    {
        #region Display Variables

        private Canvas canvas;
        private double canvasWidth;
        private double canvasHeight;
        private double quadSize;

        #endregion

        #region Variables

        private readonly MediaManager manager;
        private PersonRepository repository;
        private readonly CancellationToken cancelToken = new CancellationToken();
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        //TODO: Refactor
        private static readonly Random Random = new Random();

        #endregion

        readonly List<DisplayQuadrantViewModel> quads = new List<DisplayQuadrantViewModel>();

        public MainWindow()
        {
            InitializeComponent();
            Log.Info("Application Startup");

            manager = new MediaManager(MediaPlayer, @"C:\audio");

            Loaded += MainWindow_Loaded;

        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            await Init();
            repository = new PersonRepository(SettingsManager.Configuration.Webserver);

            for (var i = 1; i < 5; i++)
            {

                var quad = new DisplayQuadrantViewModel(QuadrantType.Normal, i);
                quads.Add(quad);
                await quad.LoadPeopleAsync();
            }

            var priorityQuad = new DisplayQuadrantViewModel(QuadrantType.Priority);
            await priorityQuad.LoadPeopleAsync();
            quads.Add(priorityQuad);

            var localQuad = new DisplayQuadrantViewModel(QuadrantType.Local);
            quads.Add(localQuad);


            foreach (var vm in quads)
            {
                await Task.Factory.StartNew(() => DisplayQuadrantViewModelAsync(vm), cancelToken);
            }

            manager.Play();

            //Log.Debug("Finished Startup");
        }

        private async Task DisplayQuadrantViewModelAsync(DisplayQuadrantViewModel vm)
        {
            while (true)
            {
                var delay = SettingsManager.Configuration.DefaultItemDelay;
                var localItemDelay = SettingsManager.Configuration.DefaultLocalItemDelay + delay;
                var priorityItemDely = SettingsManager.Configuration.DefaultPriorityItemDelay;

                //TODO: Set delay based on quad type??
                foreach (var person in vm.People.ToList())
                {
                    switch (vm.QuadrantType)
                    {
                        case QuadrantType.Local:
                            {
                                if (DateTime.Now >= person.NextDisplayTime)
                                {
                                    //Log.Debug("Displaying: {0} in quad {1}", person, person.QuadrantIndex);
                                    await
                                        Animate(person, person.QuadrantIndex, cancelToken,
                                            SettingsManager.Configuration.DefaultMaxFontSize);
                                    //Log.Debug("Displaying {0} in {1} for {2}", vm.QuadrantType, vm.QuadrantIndex, person);
                                    person.NextDisplayTime = DateTime.Now.AddSeconds(localItemDelay);
                                    person.LastDisplayTime = DateTime.Now;

                                    //Log.Debug("Last display time {0}", person.LastDisplayTime);
                                    //Log.Debug("Next display time {0}", person.NextDisplayTime);
                                    person.RotationCount += 1;
                                }
                                if (person.RotationCount > 3) vm.People.Remove(person);
                                break;
                            }
                        case QuadrantType.Priority:
                            {
                                if (DateTime.Now >= person.NextDisplayTime)
                                {
                                    //Log.Debug("Displaying: {0} in quad {1}", person, person.QuadrantIndex);
                                    await Animate(person, person.QuadrantIndex, cancelToken);
                                    //Log.Debug("Displaying {0} in {1} for {2}", vm.QuadrantType, vm.QuadrantIndex, person);
                                    await Task.Delay(TimeSpan.FromSeconds(priorityItemDely), cancelToken);
                                }
                                break;
                            }
                        case QuadrantType.Normal:
                            {
                                await Animate(person, vm.QuadrantIndex, cancelToken);
                                //Log.Debug("Displaying {0} in {1} for {2}", vm.QuadrantType, vm.QuadrantIndex, person);
                                await Task.Delay(TimeSpan.FromSeconds(delay), cancelToken);
                                break;
                            }
                    }

                }
                if (vm.QuadrantType != QuadrantType.Local)
                {
                    await vm.LoadPeopleAsync();
                }

            }
        }

        private async Task Init()
        {
            ConfigureDisplay();

            ConfigureConnectionManager();

            await InitializeDefaultSettings();

            await InitializeAudioSettings();
        }

        private void SetupHubListeners()
        {
            Log.Info("Setting up listeners on hub for kiosk names added.");
            ConnectionManager.HubProxy.On<string, Person>("nameAddedToWall", (kiosk, person) => Dispatcher.Invoke(() => AddPersonToDisplayFromKiosk(kiosk, person)));

            //TODO: Refactor saving configuration watch proxy
            Log.Info("Setting up listeners on hub for configuration changes.");
            ConnectionManager.HubProxy.On("configSettingsSaved", ReInitSettings);
        }

        private async void ReInitSettings()
        {
            //Log.Debug("Settings Changed... Reinitailizing");
            //TODO: Store in app config??
            const string configApiUrl = "http://localhost/crmc/breeze/public/configurations";

            await SettingsManager.LoadAsync(configApiUrl);

            var cfg = SettingsManager.Configuration;

            //await InitializeDefaultSettings();
            //await InitializeAudioSettings();
        }


        public async Task InitializeDefaultSettings()
        {
            //TODO: Store in app config??
            const string configApiUrl = "http://localhost/crmc/breeze/public/configurations";

            await SettingsManager.LoadAsync(configApiUrl);
            //Log.Debug("WallConfig {0}", SettingsManager.Configuration);
            //Log.Debug(SettingsManager.Configuration);
        }

        public async Task InitializeAudioSettings()
        {
            Log.Info("Initializing Audio settings.");
            //Check if path to audio exists and has audio files

            if (!Directory.GetFiles(SettingsManager.Configuration.DefaultAudioFilePath).Any(f => f.EndsWith(".mp3"))) return;

            PlayButton.Source = manager.Paused ? new BitmapImage(new Uri(@"images\pause.ico", UriKind.Relative)) : new BitmapImage(new Uri(@"images\play.ico", UriKind.Relative));
            CurrentSongTextBlock.Text = string.Format("{0}: {1}", manager.PlayStatus, manager.CurrentSong.Title);
            //Log.Debug("Volume: {0}", SettingsManager.Configuration.Volume);
            manager.ChangeVolume(SettingsManager.Configuration.Volume);

            Log.Info("Audio initialization complete.");
        }

        private Label CreateLabel(Person person)
        {
            var color = SettingsManager.RandomColor();
            var labelFontSize = CalculateFontSize(person.IsPriority);
            var name = "label" + Guid.NewGuid().ToString("N").Substring(0, 10);
            var label = new Label()
            {
                Content = person.ToString(),
                FontSize = labelFontSize,
                FontFamily = new FontFamily(SettingsManager.Configuration.FontFamily),
                HorizontalAlignment = HorizontalAlignment.Center,
                Name = name,
                Tag = name,
                Uid = name,
                Foreground = new SolidColorBrush(color)
            };

            return label;
        }

        private async Task<double> Animate(Person person, int quadrant, CancellationToken cancellationToken, int? overrideFontSize = null, bool random = true)
        {
            var totalTime = 0.0;
            await Dispatcher.InvokeAsync(() =>
            {
                NameScope.SetNameScope(this, new NameScope());

                var screenSpeedModifier = SettingsManager.Configuration.DefaultSpeedModifier; //10;
                var startTimer = SettingsManager.Configuration.NewItemOnScreenDelay; //5
                var growTime = SettingsManager.Configuration.NewItemOnScreenGrowTime; //3
                var shrinkTime = SettingsManager.Configuration.NewItemOnScreenShrinkTime;  //3
                var pauseTime = SettingsManager.Configuration.NewItemFallAnimationDelay; //1
                var fallAnimationOffset = SettingsManager.Configuration.NewItemFallAnimationDelayOffset; //-2
                var topMarginOffset = SettingsManager.Configuration.TopMarginOffset;
                var newItemTopMargin = canvasHeight.AmountFromPercent(SettingsManager.Configuration.NewItemTopMargin);
                var topMargin = random ? topMarginOffset : newItemTopMargin;


                var label = CreateLabel(person);
                label.FontSize = overrideFontSize ?? label.FontSize;

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


                //TODO: Set margins on quadrant view model??
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

                    var maxFontSize = SettingsManager.Configuration.DefaultMaxFontSize * 2;

                    var growAnimation = new DoubleAnimation
                    {
                        From = 0,
                        To = maxFontSize,
                        BeginTime = TimeSpan.FromSeconds(startTimer),
                        Duration = new Duration(TimeSpan.FromSeconds(growTime)),
                    };
                    startTimer += growTime + pauseTime;

                    var fontSize = SettingsManager.Configuration.DefaultMaxFontSize;

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
                var size = random ? label.FontSize : (double)SettingsManager.Configuration.DefaultMaxFontSize;

                //TODO: Refactor using settings.default.scrollspeed to use settings manager
                var labelScrollSpeed = ((Settings.Default.ScrollSpeed / size) * screenSpeedModifier).ToInt();
                //labelScrollSpeed = 15;

                var fallAnimation = new DoubleAnimation
                {
                    From = topMargin,
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
                Canvas.SetTop(border, topMargin);
                canvas.Children.Add(border);
                canvas.UpdateLayout();
                storyboard.Begin(this);

                totalTime = startTimer + fallAnimation.Duration.TimeSpan.TotalSeconds;
            }, DispatcherPriority.Normal, cancellationToken);
            return totalTime;
        }

        //Invoked from SignalR event
        public async void AddPersonToDisplayFromKiosk(string location, Person person)
        {
            int quad;

            int.TryParse(location, out quad);
            //Log.Debug("Sending from kiosk: " + person);

            var time = await Animate(person, quad, cancelToken, null, false);
            //Log.Debug("TotalTime: " + time);
            await Task.Delay(TimeSpan.FromSeconds(time), cancelToken);
            //Log.Debug("Should be " + time + " seconds later");
            //Log.Debug("Into continue with");

            var vm = quads.FirstOrDefault(x => x.QuadrantType == QuadrantType.Local);
            var p = Mapper.Map<Person, PersonViewModel>(person);
            p.RotationCount = 0;
            p.QuadrantIndex = quad;
            p.LastDisplayTime = DateTime.Now.AddSeconds(-time);
            if (vm != null) vm.People.Add(p);
        }

        private int CalculateFontSize(bool? isPriority)
        {
            //var maxFontSize = isPriority.GetValueOrDefault() ? Settings.Default.MaxFontSizeVIP : Settings.Default.MaxFontSize;
            //var minFontSize = isPriority.GetValueOrDefault() ? Settings.Default.MinFontSizeVIP : Settings.Default.MinFontSize;
            var maxFontSize = isPriority.GetValueOrDefault() ? SettingsManager.Configuration.DefaultPriorityMaxFontSize : SettingsManager.Configuration.DefaultMaxFontSize;
            var minFontSize = isPriority.GetValueOrDefault() ? SettingsManager.Configuration.DefaultPriorityMinFontSize : SettingsManager.Configuration.DefaultMinFontSize;
            return RandomNumber(minFontSize.GetValueOrDefault(), maxFontSize.GetValueOrDefault());
        }

        //TODO: Refactor
        public int RandomNumber(int min, int max)
        {
            if (max <= min) min = max - 1;
            return Random.Next(min, max);
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
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 80 });
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
            //TODO: Save Configuration settings to database and update
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

        private void OnMouseDownPlayMedia(object sender, MouseButtonEventArgs e)
        {
            if (manager == null)
            {
                return;
            }
            manager.Play();
        }

        private void OnMouseDownStopMedia(object sender, MouseButtonEventArgs e)
        {
            if (manager == null)
            {
                return;
            }

            manager.Stop();
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
