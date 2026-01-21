using Newtonsoft.Json;
using PSFR_Services.Models;
using System.Net.Http.Headers;
using System.Text;

namespace PSFR_Services.Apis
{
    public class AdvancedSearchApis
    {
        public static async Task<string> ListAdvancedSearch(string formData, string dmsUrl, string token, CancellationToken cancellationToken)
        {

            string url = $"{dmsUrl}/AdvancedSearch/List";
            using HttpClient client = new();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using HttpContent content = new StringContent(formData, Encoding.UTF8, "application/x-www-form-urlencoded");

            using HttpResponseMessage response = await client.PostAsync(url, content, cancellationToken);

            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return responseBody;
        }
        public static async Task<string> ListAdvanceSearch(string formData, string dmsUrl, string token, CancellationToken cancellationToken)
        {

            string url = $"{dmsUrl}/AdvancedSearch/ListAdvance";
            using HttpClient client = new();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using HttpContent content = new StringContent(formData, Encoding.UTF8, "application/x-www-form-urlencoded");

            using HttpResponseMessage response = await client.PostAsync(url, content, cancellationToken);

            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return responseBody;
        }
        public static async Task<string> ListExpertSearch(string formData, string dmsUrl, string token, CancellationToken cancellationToken)
        {

            string url = $"{dmsUrl}/ExpertSearch/List";
            using HttpClient client = new();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using HttpContent content = new StringContent(formData, Encoding.UTF8, "application/x-www-form-urlencoded");

            using HttpResponseMessage response = await client.PostAsync(url, content, cancellationToken);

            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return responseBody;
        }

        public static async Task<List<ActiveBasicContentType>> ListActiveBasicContentTypes(string dmsUrl, string token, CancellationToken cancellationToken)
        {

            string url = $"{dmsUrl}/FileContentTypes/ListActiveBasic";
            using HttpClient client = new();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage response = await client.GetAsync(url, cancellationToken);

            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            List<ActiveBasicContentType> responseBodyDeserialized = JsonConvert.DeserializeObject<List<ActiveBasicContentType>>(responseBody)
                ?? throw new Exception("An error occured while trying to deserialize ActiveBasic ContentTypes");

            return responseBodyDeserialized;
        }

        public static async Task<ActiveFileContentType> GetActiveFileContentType(string dmsUrl, string token, int contentTypeId, CancellationToken cancellationToken)
        {
            string url = $"{dmsUrl}/FileContentTypes/GetActive?fileContentTypeId={contentTypeId}";

            using HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using HttpResponseMessage response = await client.GetAsync(url, cancellationToken);

            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            ActiveFileContentType responseBodyDeserialized = JsonConvert.DeserializeObject<ActiveFileContentType>(responseBody)
                ?? throw new Exception("An error occured while trying to deserialize ActiveBasic ContentTypes");

            return responseBodyDeserialized;
        }
    }
}
