using System;

namespace BundtBot.Youtube
{
    public struct YoutubeDlUrl
    {
        public readonly string UrlOrSearchString;
        
        /// <summary>
        /// See this link for supported sites: https://rg3.github.io/youtube-dl/supportedsites.html
        /// </summary>
        public static YoutubeDlUrl FromUrl(Uri mediaUrl)
        {
            if (mediaUrl.IsAbsoluteUri == false) throw new ArgumentException("Must be an absolute uri", nameof(mediaUrl));

            return new YoutubeDlUrl(mediaUrl.ToString());
        }

        /// <summary>
        /// Will search youtube and download first video found.
        /// </summary>
        public static YoutubeDlUrl FromSearchString(string searchString)
        {
            return new YoutubeDlUrl($"\"ytsearch1:{searchString}\"");
        }

        YoutubeDlUrl(string youtubeDlUrl)
        {
            UrlOrSearchString = youtubeDlUrl;
        }
    }
}