namespace Rydo.HttpClient.Formatters
{
    using System;
    using System.Text.RegularExpressions;

    internal static class UriFormatter
    {
        internal const string InvalidTemplateException = "The number of URL parameters should be the same as the informed args.";
        
        public static string Format(string template, params object[] args)
        {
            //TODO: Improve performance!

            var matches = Regex.Matches(template, "{.*?}", RegexOptions.Compiled);
            if (matches.Count < 1)
                return template;

            if (matches.Count != args.Length)
                throw new InvalidOperationException(InvalidTemplateException);

            for (var i = 0; i < matches.Count; i++)
            {
                template = template.Replace(matches[i].Value, args[i].ToString());
            }
            
            return template;
        }
    }
}