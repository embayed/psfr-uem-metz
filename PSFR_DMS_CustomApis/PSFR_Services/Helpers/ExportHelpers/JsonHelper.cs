using PSFR_Services.Enums;
using PSFR_Services.Models;
using System.Text.Json.Nodes;

namespace PSFR_Services.Helpers.ExportHelpers
{
    public class JsonHelper
    {
        public static int ParseOrder(JsonNode? node)
        {
            if (node == null) return int.MaxValue;

            return int.TryParse(node.ToString(), out int value)
                ? value
                : int.MaxValue;
        }

        public static JsonObject ParseFormData(JsonNode? node)
        {
            if (node == null) return [];
            try
            {
                string json = node.ToString();
                if (string.IsNullOrWhiteSpace(json)) return [];
                return JsonNode.Parse(json)!.AsObject();
            }
            catch { return []; }
        }

        public static string GetValue(JsonNode row, JsonObject formData, string key, List<ActiveBasicContentType> contentTypes)
        {
            return key switch
            {
                "Name" => BuildFileName(row),
                "ReferenceNumber" => row["referenceNumber"]?.ToString() ?? string.Empty,
                "Version" => row["version"]?.ToString() ?? string.Empty,
                "Location" => row["location"]?.ToString() ?? string.Empty,
                "CreatedBy" => row["createdByUser"]?.ToString() ?? string.Empty,
                "CreatedDate" => row["createdDate"]?.ToString() ?? string.Empty,
                "ModifiedBy" => row["modifiedByUser"]?.ToString() ?? string.Empty,
                "ModifiedDate" => row["modifiedDate"]?.ToString() ?? string.Empty,
                "DefaultIcons" => string.Empty,
                "Size" => FormatBytes(row["fileSize"]),

                "ContentType" => contentTypes.FirstOrDefault(x => x.Id == (row["fileContentTypeId"]?.ToString() ?? "0"))?.Text ?? "Document",
                _ => formData.ContainsKey(key) ? formData[key]?.ToString() ?? string.Empty : string.Empty
            };
        }

        public static string ResolveHeader(JsonArray translations, string key, LanguagePS lang = LanguagePS.Fr)
        {
            if (translations == null || string.IsNullOrWhiteSpace(key))
            {
                return key;
            }

            JsonNode? match = translations.FirstOrDefault(item => item?["Keyword"]?.ToString().Equals(key, StringComparison.OrdinalIgnoreCase) == true);

            string? value = match?[lang.ToString()]?.ToString();

            return string.IsNullOrWhiteSpace(value) ? key : value;
        }

        private static string BuildFileName(JsonNode row)
        {
            string name = row["name"]?.ToString() ?? string.Empty;
            string extension = row["extension"]?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(extension))
            {
                return name;
            }

            return $"{name}.{extension}";
        }

        private static string FormatBytes(JsonNode? sizeNode)
        {
            if (sizeNode == null)
            {
                return string.Empty;
            }

            if (!long.TryParse(sizeNode.ToString(), out long bytes))
            {
                return string.Empty;
            }

            string[] units = ["B", "KB", "MB", "GB", "TB"];
            double size = bytes;
            int unitIndex = 0;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return $"{size:0.##} {units[unitIndex]}";
        }
    }
}
