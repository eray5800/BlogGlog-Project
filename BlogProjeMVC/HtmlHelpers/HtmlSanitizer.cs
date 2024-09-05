namespace BlogProjeMVC.HtmlHelpers
{
    public class HtmlSanitizer
    {
        public static string HtmlEncodeScriptTags(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input.Replace("<script", "&lt;script")
                        .Replace("</script>", "&lt;/script&gt;");
        }
    }
}
