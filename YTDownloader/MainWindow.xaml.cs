using System;
using System.Collections.Generic;
using System.IO;
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
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YTDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        YoutubeClient client;
        List<YouTubeList> videoList;
        YoutubeConverter converter;

        public bool ControlsActive
        {
            set
            {
                if (value == true)
                {
                    convert.IsEnabled = true;
                    url.IsEnabled = true;
                    videos.IsEnabled = true;
                    add.IsEnabled = true;
                }
                else
                {
                    convert.IsEnabled = false;
                    url.IsEnabled = false;
                    videos.IsEnabled = false;
                    add.IsEnabled = false;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            videoList = new List<YouTubeList>();
            client = new YoutubeClient();
            converter = new YoutubeConverter();
            progress.Minimum = 0;
            progress.Maximum = 1;
            progress.Value = 1;
            this.ResizeMode = ResizeMode.NoResize;
        }

        public static string Truncate(string value, int maxChars = 45)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars - 4) + " ...";
        }

        private void convert_Click(object sender, RoutedEventArgs e)
        {
            if (videoList.Count == 0)
            {
                MessageBox.Show("Add some videos first!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (mp3.IsChecked == true)
            {
                ConvertVideos("mp3");
            }
            else if (mp4.IsChecked == true)
            {
                ConvertVideos("mp4");
            }
            else if (wav.IsChecked == true)
            {
                ConvertVideos("wav");
            }
        }

        private async void ConvertVideos(string type)
        {
            try
            {
                ControlsActive = false;
                progress.Value = 0;
                progress.Minimum = 0;
                progress.Maximum = videoList.Count;
                status.Content = $"{progress.Value}/{progress.Maximum}";
                Video video;
                string videoName;
                string author;
                StreamManifest sm;
                IStreamInfo[] streamInfo;
                switch (type)
                {
                    case "mp3":
                        foreach (var i in videoList)
                        {
                            video = await client.Videos.GetAsync(i.VideoURL);
                            videoName = video.Title;
                            videoName = ReplaceChars(videoName);
                            author = video.Author;
                            author = ReplaceChars(author);
                            await converter.DownloadVideoAsync(video.Id, System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), $"{author} - {videoName}.{type}"));
                            ++progress.Value;
                            status.Content = $"{progress.Value}/{progress.Maximum}";
                        }
                        break;
                    case "wav":
                        goto case "mp3";
                    case "mp4":
                        foreach (var i in videoList)
                        {
                            video = await client.Videos.GetAsync(i.VideoURL);
                            videoName = video.Title;
                            videoName = ReplaceChars(videoName);
                            author = video.Author;
                            author = ReplaceChars(author);

                            sm = await client.Videos.Streams.GetManifestAsync(video.Id);
                            var audio = sm.GetAudio().WithHighestBitrate();
                            var videoStream = sm.GetVideo().FirstOrDefault(s => s.VideoQualityLabel == "1080p");
                            streamInfo = new IStreamInfo[] { audio, videoStream };
                            await converter.DownloadAndProcessMediaStreamsAsync(streamInfo, System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), $"{author} - {videoName}.{type}"), "mp4");
                            ++progress.Value;
                            status.Content = $"{progress.Value}/{progress.Maximum}";
                        }
                        break;
                }
                status.Content = $"Done";
                ControlsActive = true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to convert videos, try having ffmpeg.exe installed in the programs directory, if that doesn't work, contact the developer. \n" + Convert.ToString(e), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ControlsActive = false;
                YouTubeList video;
                var vid = await client.Videos.GetAsync(url.Text);
                video = new YouTubeList(vid.Title, vid.Author, vid.Url, vid.Engagement.ViewCount, vid.Thumbnails.HighResUrl);
                AddToList(video);
                url.Text = string.Empty;
                ControlsActive = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to add video.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ControlsActive = true;
            }
        }

        private void AddToList(YouTubeList video)
        {
            videos.ItemsSource = null;
            videoList.Add(video);
            videos.ItemsSource = videoList;
        }

        private void remove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = videos.SelectedIndex;
                videoList.RemoveAt(index);
                videos.ItemsSource = null;
                videos.ItemsSource = videoList;
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to remove video.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ReplaceChars(string name)
        {
            string illegal = name;
            string invalid = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());

            foreach (char c in invalid)
            {
                illegal = illegal.Replace(c.ToString(), "");
            }

            return illegal;
        }
    }
}
