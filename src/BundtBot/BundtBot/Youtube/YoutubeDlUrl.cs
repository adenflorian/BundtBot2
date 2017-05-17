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
        public static YoutubeDlUrl FromYoutubeSearchString(string searchString)
        {
            return new YoutubeDlUrl($"\"ytsearch1:{searchString}\"");
        }

        /// <summary>
        /// Will search soundcloud and download first audioclip found.
        /// </summary>
        public static YoutubeDlUrl FromSoundcloudSearchString(string searchString)
        {
            return new YoutubeDlUrl($"\"scsearch1:{searchString}\"");
        }

        // If you want to add more search options
        // First see this page: https://rg3.github.io/youtube-dl/supportedsites.html
        // If the site you want has a :search thingy, then look for a python file for that extractor here: https://rg3.github.io/youtube-dl/supportedsites.html
        // Inside that python file search for _SEARCH_KEY
        // You should be able to connect the dots from there...

        YoutubeDlUrl(string youtubeDlUrl)
        {
            UrlOrSearchString = youtubeDlUrl;
        }
    }
}