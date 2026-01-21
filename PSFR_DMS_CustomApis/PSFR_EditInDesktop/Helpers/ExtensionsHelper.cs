namespace PSFR_EditInDesktop.Helpers
{
    public class ExtensionsHelper
    {
        public static string GetMsExtContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".mpp" => "application/vnd.ms-project",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls" => "application/vnd.ms-excel",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".vsdx" => "application/vnd.ms-visio.drawing",
                ".vsd" => "application/vnd.visio",
                ".pdf" => "application/pdf",
                ".dwg" => "application/acad",
                ".dxf" => "application/dxf",
                ".dwt" => "application/acad",
                _ => "application/octet-stream"
            };
        }
    }
}
