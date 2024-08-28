using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlogProjeMVC.HtmlHelpers
{
    public static class HtmlHelpers
    {
        public static IHtmlContent ShortenContent(this IHtmlHelper htmlHelper, string content, int maxLength)
        {
            if (string.IsNullOrEmpty(content)) return HtmlString.Empty;
            var shortened = content.Length <= maxLength ? content : content.Substring(0, maxLength) + "...";
            return new HtmlString(shortened);
        }
    }
}
