// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information

using Microsoft.AspNetCore.WebHooks.Storage;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Provides an implementation of <see cref="IWebHookStore"/> storing registered WebHooks in Microsoft SQL Server.
    /// </summary>
    //[CLSCompliant(false)]
    public class SqlWebHookStore : DbWebHookStore<WebHookStoreContext, Registration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlWebHookStore"/> class
        /// and <paramref name="logger"/>.
        /// Using this constructor, the data will not be encrypted while persisted to the database.
        /// </summary>
        public SqlWebHookStore(ILogger<SqlWebHookStore> logger, WebHookStoreContext context)
            : base(logger, context)
        {
        }
    }
}
