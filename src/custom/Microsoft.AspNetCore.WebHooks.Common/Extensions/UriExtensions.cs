// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;

namespace System
{
    /// <summary>
    /// Extension methods for <see cref="string"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class UriExtensions
    {
        /// <summary>
        /// Checks to see if the URI scheme is 'http'. The check is case-insensitive.
        /// </summary>
        /// <param name="input">The URI to verify.</param>
        /// <returns><c>true</c> if URI scheme is 'http'; false otherwise.</returns>
        public static bool IsHttp(this Uri input)
        {
            return input != null && input.IsAbsoluteUri && string.Equals(input.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks to see if the URI scheme is 'https'. The check is case-insensitive.
        /// </summary>
        /// <param name="input">The URI to verify.</param>
        /// <returns><c>true</c> if URI scheme is 'https'; false otherwise.</returns>
        public static bool IsHttps(this Uri input)
        {
            return input != null && input.IsAbsoluteUri && string.Equals(input.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Parsing url query string
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseQueryString(this Uri uri)
        {
            var requestQueryString = uri.Query;
            Dictionary<string, string> rc = new Dictionary<string, string>();
            string[] ar1 = requestQueryString.Split(new char[] { '&', '?' });
            foreach (string row in ar1)
            {
                if (string.IsNullOrEmpty(row)) continue;
                int index = row.IndexOf('=');
                rc[Uri.UnescapeDataString(row.Substring(0, index))] = Uri.UnescapeDataString(row.Substring(index + 1)); // use Unescape only parts          
            }
            return rc;

        }
    }
}
