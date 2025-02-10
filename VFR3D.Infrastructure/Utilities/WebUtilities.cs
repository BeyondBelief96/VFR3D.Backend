namespace VFR3D.Infrastructure.Utilities
{
    public static class WebUtilities
    {
        public static string AddQueryString(string baseUrl, Dictionary<string, string> queryParams)
        {
            var uriBuilder = new UriBuilder(baseUrl);
            var query = string.Join("&", queryParams
                .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            if (uriBuilder.Query != null && uriBuilder.Query.Length > 1)
                uriBuilder.Query = uriBuilder.Query.Substring(1) + "&" + query;
            else
                uriBuilder.Query = query;

            return uriBuilder.ToString();
        }
    }
}
