using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Windows.Media.Control;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Source_MediaInfo
{
    public class ExtType
    {
        public static readonly string Type = "Source";
        public static readonly string Name = "MediaInfo";
        //return type json = json stringified
        public static readonly Dictionary<string, string> Interfaces = new Dictionary<string, string>() { 
            { "GetMediaInfo", "json" }, 
            { "GetMediaInfoWithThumbnail", "json" } 
        };

    }

    public class Source
    {
        private static string Join(IReadOnlyList<string> list)
        {
            var sb = new StringBuilder();
            foreach (var item in list)
            {
                if (sb.Length > 0) { sb.Append(";"); }
                sb.Append(item.ToString());
            }
            return sb.ToString();
        }

        private async static Task<JObject> GetData(bool Thumbnail = false)
        {
            JObject obf = new JObject();
            obf["Artist"] = null;
            obf["Title"] = null;
            obf["AlbumTitle"] = null;
            obf["Genres"] = null;
            obf["TrackNumber"] = 0;
            obf["AlbumTrackCount"] = 0;
            obf["Thumbnail"] = null;
            obf["error"] = null;

            try
            {
                GlobalSystemMediaTransportControlsSessionManager gsmtcsm = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties = await gsmtcsm.GetCurrentSession().TryGetMediaPropertiesAsync();
                obf["Artist"] = (mediaProperties.Artist == null || mediaProperties.Artist.Trim() == "") ? mediaProperties.AlbumArtist : mediaProperties.Artist;
                obf["Title"] = mediaProperties.Title;
                obf["AlbumTitle"] = mediaProperties.AlbumTitle;
                obf["Genres"] = Join(mediaProperties.Genres);
                obf["TrackNumber"] = mediaProperties.TrackNumber;
                obf["AlbumTrackCount"] = mediaProperties.AlbumTrackCount;

                if(Thumbnail) {
                    IRandomAccessStreamWithContentType tor = await mediaProperties.Thumbnail.OpenReadAsync();
                    IBuffer buffer = new Windows.Storage.Streams.Buffer((uint)tor.Size);
                    await tor.ReadAsync(buffer, (uint)tor.Size, InputStreamOptions.None);
                    obf["Thumbnail"] = Convert.ToBase64String(buffer.ToArray());
                }
            }
            catch (Exception err) { obf["error"] = err.StackTrace; }

            return obf;
        }

        public async static Task<string> GetMediaInfo()
        {
            Task<JObject> obt = GetData(false);
            obt.Wait();
            JObject obf = obt.Result;
            return JsonConvert.SerializeObject(obf);
        }

        public async static Task<string> GetMediaInfoWithThumbnail()
        {
            Task<JObject> obt = GetData(true);
            obt.Wait();
            JObject obf = obt.Result;
            return JsonConvert.SerializeObject(obf);
        }
    }
}
