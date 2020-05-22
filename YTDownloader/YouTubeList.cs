using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace YTDownloader
{
    public class YouTubeList
    {
        public string VideoName { get; set; }
        public string VideoChannel { get; set; }
        public string VideoURL { get; set; }
        public long VideoViews { get; set; }
        public string VideoThumbnail { get; set; }

        public YouTubeList(string name, string author, string url, long views, string source)
        {
            this.VideoName = name;
            this.VideoChannel = author;
            this.VideoURL = url;
            this.VideoViews = views;
            this.VideoThumbnail = source;
        }
    }
}
