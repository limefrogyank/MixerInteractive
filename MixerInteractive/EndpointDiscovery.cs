using MixerInteractive.InteractiveError;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MixerInteractive
{
    public class EndpointDiscovery
    {
        public async Task<IEnumerable<string>> RetrieveEndpointsAsync(string endpoint = "https://mixer.com/api/v1/interactive/hosts")
        {
            var httpClient = new HttpClient();
            var res = await httpClient.GetAsync(endpoint);
            if (res.IsSuccessStatusCode)
            {
                var resStr = await res.Content.ReadAsStringAsync();
                if (resStr.Length > 0)
                {
                    var doc = System.Text.Json.JsonDocument.Parse(resStr);
                    var enumerator = doc.RootElement.EnumerateArray();
                    List<string> urls = new List<string>();
                    while (enumerator.MoveNext())
                    {
                        var prop = enumerator.Current.GetProperty("address");
                        urls.Add(prop.GetRawText().Trim('"'));
                    }
                    return urls;
                }
                else
                    throw new NoInteractiveServersAvailableException("No Interactive servers are available, please try again.");
            }
            else
                throw new NoInteractiveServersAvailableException("No Interactive servers are available, please try again.");
        }
    }
}
