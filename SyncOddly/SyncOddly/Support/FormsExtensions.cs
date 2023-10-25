using System;
using System.Collections.Generic;
using System.Web;

namespace FormsExtensions;

public static class FormsRoutingExtensions
{
    /// <summary>
    /// Safe helper to get optional attributes from a URL
    /// </summary>
    /// <param name="dict">Dictionary typically for URI routing in Shell</param>
    /// <param name="attribute">name in URI encoded form, typically just alphanumeric</param>
    /// <returns>null string if not found</returns>
    public static string DecodedParam(this IDictionary<string, string> dict, string attribute)
	{
        string encValue;
        if (!dict.TryGetValue(attribute, out encValue))
            return null;
        return HttpUtility.UrlDecode(encValue);

    }
}

