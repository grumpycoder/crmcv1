using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace crmc.wotdisplay
{
    public class MediaManager
    {
        private readonly MediaElement player;
        private readonly string filePath;


        public string FilePath { get; set; }
        public int ActiveIndex { get; set; }
        public bool Paused { get; set; }

        public bool IsPlaying { get; set; }

        public PlayStatus PlayStatus { get; set; }

        public ObservableCollection<Song> Songs { get; private set; }

        public Song CurrentSong { get; set; }

        public MediaManager(MediaElement mediaElement, string path)
        {
            Paused = false;
            IsPlaying = false;
            FilePath = path;
            player = mediaElement;

            PlayStatus = PlayStatus.NotLoaded;
            Songs = new ObservableCollection<Song>();
            player.MediaEnded += player_MediaEnded;
            RefreshPlaylist();
        }


        private void LoadPlaylist()
        {
            foreach (var file in Directory.GetFiles(@FilePath).Where(s => s.EndsWith(".mp3")))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var path = file;
                Songs.Add(new Song(name, path));
            }
            if (Songs.Count == 0) return;

            CurrentSong = Songs[0];
        }

        public void Play()
        {
            if (!Paused && IsPlaying)
            {
                Pause();
            }
            else
            {
                if (!Paused && CurrentSong != null)
                {
                    player.Source = CurrentSong.Path;
                }
                player.Play();
                PlayStatus = PlayStatus.Playing;
                Paused = false;
                IsPlaying = true;
            }
        }

        public void Pause()
        {
            Paused = true;
            IsPlaying = false;
            PlayStatus = PlayStatus.Paused;
            player.Pause();
        }

        public void Stop()
        {
            player.Stop();
            player.Position = new TimeSpan(0);
            PlayStatus = PlayStatus.Stopped;
            IsPlaying = false;
            Paused = false;
        }

        public void Next()
        {
            Stop();
            ActiveIndex++;
            if (ActiveIndex >= Songs.Count)
            {
                ActiveIndex = 0;
            }
            CurrentSong = Songs[ActiveIndex];
            if (IsPlaying)
            {
                Play();
            }
        }

        public void Prev()
        {
            Stop();
            ActiveIndex--;
            if (ActiveIndex < 0)
            {
                ActiveIndex = Songs.Count - 1;
                CurrentSong = Songs[ActiveIndex];
            }
            CurrentSong = Songs[ActiveIndex];
            if (IsPlaying)
            {
                Play();
            }
        }

        public void ChangeVolume(double value)
        {
            player.Volume = (double)value;
        }

        public void RefreshPlaylist()
        {
            LoadPlaylist();
        }

        private void player_MediaEnded(object sender, System.Windows.RoutedEventArgs e)
        {
            Debug.WriteLine("Media ended");
            Next();
            Play();
        }


    }

    public enum PlayStatus
    {
        Playing,
        Paused,
        Stopped,
        NotLoaded
    }
}
