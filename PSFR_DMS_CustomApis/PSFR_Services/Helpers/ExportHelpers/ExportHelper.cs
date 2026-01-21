using ClosedXML.Excel;
using PSFR_Services.Enums;
using PSFR_Services.Models;
using System.Text;
using System.Text.Json.Nodes;

namespace PSFR_Services.Helpers.ExportHelpers
{
    public class ExportHelper
    {
        public static string ExportToExcelBase64(IList<JsonNode?> columns, JsonArray rows, JsonArray translations, LanguagePS lang, List<ActiveBasicContentType> activeBasicContentTypes)
        {
            using XLWorkbook workbook = new();
            IXLWorksheet ws = workbook.Worksheets.Add("SearchGrid");

            // Header
            for (int c = 0; c < columns.Count; c++)
            {
                string key = columns[c]!["id"]!.ToString();
                string header = JsonHelper.ResolveHeader(translations, key, lang);

                ws.Cell(1, c + 1).Value = header;
                ws.Cell(1, c + 1).Style.Font.Bold = true;
            }

            int excelRow = 2;

            // Rows
            foreach (var row in rows)
            {
                var formData = JsonHelper.ParseFormData(row!["formData"]);

                for (int c = 0; c < columns.Count; c++)
                {
                    string key = columns[c]!["id"]!.ToString();
                    ws.Cell(excelRow, c + 1).Value = JsonHelper.GetValue(row!, formData, key, activeBasicContentTypes);
                }

                excelRow++;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);

            byte[] bytes = ms.ToArray();
            return Convert.ToBase64String(bytes);
        }

        public static string ExportToCsvBase64(IList<JsonNode?> columns, JsonArray rows, JsonArray translations, LanguagePS language, List<ActiveBasicContentType> activeBasicContentTypes)
        {
            var csv = new StringBuilder();

            // Header
            for (int i = 0; i < columns.Count; i++)
            {
                string key = columns[i]!["id"]!.ToString();
                string header = JsonHelper.ResolveHeader(translations, key, language);

                csv.Append($"\"{header.Replace("\"", "\"\"")}\"");

                if (i < columns.Count - 1)
                    csv.Append(',');
            }
            csv.AppendLine();


            // Rows
            foreach (var row in rows)
            {
                var formData = JsonHelper.ParseFormData(row!["formData"]);

                for (int c = 0; c < columns.Count; c++)
                {
                    string key = columns[c]!["id"]!.ToString();
                    string value = JsonHelper.GetValue(row!, formData, key, activeBasicContentTypes);

                    csv.Append($"\"{value.Replace("\"", "\"\"")}\"");

                    if (c < columns.Count - 1)
                        csv.Append(',');
                }

                csv.AppendLine();
            }

            byte[] bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return Convert.ToBase64String(bytes);
        }
    }
}
