using PSFR_Services.Apis;
using PSFR_Services.Constants;
using PSFR_Services.Enums;
using PSFR_Services.Helpers.ExportHelpers;
using PSFR_Services.Models;
using System.Collections.Specialized;
using System.Net;
using System.Text.Json.Nodes;
using System.Web;

namespace PSFR_Services.Services
{
    public class ExportAdvancedSearchResults
    {
        public static async Task<byte[]> GenerateExportableSearchResultService(string formData, SearchType searchType, string dmsUrl, string token, string? selectedFiles, LanguagePS language = LanguagePS.Fr, ExportFormat format = ExportFormat.Excel, string pageSize = "10000", CancellationToken cancellationToken = default)
        {
            try
            {
                formData = EnsureFullExportPagingWhenNoSelection(formData, selectedFiles, pageSize);

                int? contentTypeId = ExtractContentTypeId(formData);

                string searchGridString = ExportServiceConstants.DeafultSearchGridString;
                List<ActiveBasicContentType> activeBasicContentTypes = BuildDefaultBasicContentTypes();

                await LoadContentTypesAndSearchGridIfNeeded(contentTypeId, dmsUrl, token, cancellationToken, activeBasicContentTypes, (string sg) => searchGridString = sg);

                string responseBody = await ExecuteSearchAsync(searchType, formData, dmsUrl, token, cancellationToken);
                JsonNode root = JsonNode.Parse(responseBody)!;
                JsonArray rows = root["data"]!.AsArray();

                FilterRowsBySelectedFilesIfNeeded(rows, selectedFiles);

                JsonNode searchGrid = JsonNode.Parse(searchGridString)!;

                List<JsonNode?> columns = [ .. searchGrid["columns"]!
                    .AsArray()
                    .Where(c => c?["id"]?.ToString() != "DefaultIcons")
                    .OrderBy(c => JsonHelper.ParseOrder(c?["order"]))];

                JsonArray translations = searchGrid["translations"]!.AsArray();

                string base64File = BuildExportBase64(format, columns, rows, translations, language, activeBasicContentTypes);

                return Convert.FromBase64String(base64File);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while generating the report file.", ex);
            }
        }

        private static string EnsureFullExportPagingWhenNoSelection(string formData, string? selectedFiles, string pageSize)
        {
            if (!string.IsNullOrWhiteSpace(selectedFiles)) return formData;

            NameValueCollection query = HttpUtility.ParseQueryString(formData);

            query["draw"] = "1";
            query["start"] = "0";
            query["length"] = pageSize;

            return query.ToString() ?? formData;
        }

        private static List<ActiveBasicContentType> BuildDefaultBasicContentTypes()
        {
            return
            [
                new ActiveBasicContentType { Id = "0", Text = "Document" },
                new ActiveBasicContentType { Id = null, Text = "Document" }
            ];
        }

        private static async Task LoadContentTypesAndSearchGridIfNeeded(int? contentTypeId, string dmsUrl, string token, CancellationToken cancellationToken, List<ActiveBasicContentType> activeBasicContentTypes, Action<string> setSearchGridString)
        {
            if (contentTypeId != null && contentTypeId.Value == 0) return;

            List<ActiveBasicContentType> loaded = await AdvancedSearchApis.ListActiveBasicContentTypes(dmsUrl, token, cancellationToken);
            activeBasicContentTypes.Clear();
            activeBasicContentTypes.AddRange(loaded);

            if (contentTypeId == null) return;

            ActiveFileContentType activeFileContentType = await AdvancedSearchApis.GetActiveFileContentType(dmsUrl, token, contentTypeId.Value, cancellationToken);
            setSearchGridString(activeFileContentType.searchGrid ?? ExportServiceConstants.DeafultSearchGridString);
        }

        private static async Task<string> ExecuteSearchAsync(SearchType searchType, string formData, string dmsUrl, string token, CancellationToken cancellationToken)
        {
            return searchType switch
            {
                SearchType.ExpertSearch => await AdvancedSearchApis.ListExpertSearch(formData, dmsUrl, token, cancellationToken),
                SearchType.AdvancedSearch => await AdvancedSearchApis.ListAdvancedSearch(formData, dmsUrl, token, cancellationToken),
                SearchType.AdvanceSearch => await AdvancedSearchApis.ListAdvancedSearch(formData, dmsUrl, token, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType, "Unsupported search type")
            };
        }

        private static void FilterRowsBySelectedFilesIfNeeded(JsonArray rows, string? selectedFiles)
        {
            if (string.IsNullOrWhiteSpace(selectedFiles))
            {
                return;
            }

            string[] selectedFilesList = selectedFiles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            for (int i = rows.Count - 1; i >= 0; i--)
            {
                JsonNode? node = rows[i];
                string? id = node?["id"]?.ToString();

                if (string.IsNullOrEmpty(id) || !selectedFilesList.Contains(id))
                {
                    rows.RemoveAt(i);
                }
            }
        }

        private static string BuildExportBase64(ExportFormat format, List<JsonNode?> columns, JsonArray rows, JsonArray translations, LanguagePS language, List<ActiveBasicContentType> activeBasicContentTypes)
        {
            return format switch
            {
                ExportFormat.Excel => ExportHelper.ExportToExcelBase64(columns, rows, translations, language, activeBasicContentTypes),
                ExportFormat.Csv => ExportHelper.ExportToCsvBase64(columns, rows, translations, language, activeBasicContentTypes),
                _ => ExportHelper.ExportToExcelBase64(columns, rows, translations, language, activeBasicContentTypes)
            };
        }

        private static int? ExtractContentTypeId(string formBody)
        {
            if (string.IsNullOrWhiteSpace(formBody)) return null;

            string[] pairs = formBody.Split('&', StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> data = new(StringComparer.OrdinalIgnoreCase);

            foreach (string pair in pairs)
            {
                string[] kv = pair.Split('=', 2);

                string key = WebUtility.UrlDecode(kv[0]);
                string value = kv.Length > 1 ? WebUtility.UrlDecode(kv[1]) : string.Empty;

                data[key] = value;
            }

            if (data.TryGetValue("model[fileContentType]", out string? raw) && int.TryParse(raw, out int value1))
            {
                return value1;
            }

            return null;
        }
    }
}
