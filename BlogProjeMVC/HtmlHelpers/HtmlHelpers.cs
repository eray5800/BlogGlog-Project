using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;

namespace BlogProjeMVC.HtmlHelpers
{
    public static class HtmlHelpers
    {
        public static IHtmlContent ShortenContent(this IHtmlHelper htmlHelper, string content, int maxLength)
        {
            if (string.IsNullOrEmpty(content)) return HtmlString.Empty;

            
            string contentWithoutHtmlTags = Regex.Replace(content, "<.*?>", string.Empty);

            string contentWithoutSpaces = contentWithoutHtmlTags.Replace(" ", "");


            var shortened = contentWithoutSpaces.Length <= maxLength
                ? contentWithoutHtmlTags
                : contentWithoutHtmlTags.Substring(0, maxLength) + "...";

            return new HtmlString(shortened);
        }
    }
}
