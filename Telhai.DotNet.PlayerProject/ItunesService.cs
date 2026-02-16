using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Telhai.DotNet.PlayerProject
{
    public class ItunesService
    {
        private readonly HttpClient http = new HttpClient();

        public async Task<SongMetadata?> SearchAsync(string query, CancellationToken token)
        {
            try
            {
                string url = $"https://itunes.apple.com/search?term={query}&entity=song&limit=1";

                var response = await http.GetAsync(url, token);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync(token);

                using JsonDocument doc = JsonDocument.Parse(json);

                var results = doc.RootElement.GetProperty("results");

                if (results.GetArrayLength() == 0)
                    return null;

                var r = results[0];

                return new SongMetadata
                {
                    TrackName = r.GetProperty("trackName").GetString() ?? "",
                    ArtistName = r.GetProperty("artistName").GetString() ?? "",
                    AlbumName = r.GetProperty("collectionName").GetString() ?? "",
                    ArtworkUrl = r.GetProperty("artworkUrl100").GetString() ?? ""
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
