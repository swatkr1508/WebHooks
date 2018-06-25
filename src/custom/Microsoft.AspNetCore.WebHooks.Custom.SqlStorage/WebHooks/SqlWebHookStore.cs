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
        //private readonly string _nameOrConnectionString;
        //private readonly string _schemaName;
        //private readonly string _tableName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlWebHookStore"/> class
        /// and <paramref name="logger"/>.
        /// Using this constructor, the data will not be encrypted while persisted to the database.
        /// </summary>
        public SqlWebHookStore(ILogger<SqlWebHookStore> logger, WebHookStoreContext context)
            : base(logger, context)
        {
            //if (settings == null)
            //{
            //    throw new ArgumentNullException(nameof(settings));
            //}
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="SqlWebHookStore"/> class with the given <paramref name="settings"/>
        ///// <paramref name="logger"/>, <paramref name="nameOrConnectionString"/>, <paramref name="schemaName"/> and <paramref name="tableName"/>.
        ///// Using this constructor, the data will not be encrypted while persisted to the database.
        ///// </summary>
        //public SqlWebHookStore(SettingsDictionary settings, ILogger logger, WebHookStoreContext context, string nameOrConnectionString, string schemaName, string tableName)
        //    : base(logger, context)
        //{
        //    if (settings == null)
        //    {
        //        throw new ArgumentNullException(nameof(settings));
        //    }

        //    _nameOrConnectionString = nameOrConnectionString;
        //    _schemaName = schemaName;
        //    _tableName = tableName;
        //}

        ///// <summary>
        ///// Initializes a new instance of the <see cref="SqlWebHookStore"/> class with the given <paramref name="settings"/>,
        ///// <paramref name="protector"/>, and <paramref name="logger"/>.
        ///// Using this constructor, the data will be encrypted using the provided <paramref name="protector"/>.
        ///// </summary>
        //public SqlWebHookStore(SettingsDictionary settings, IDataProtector protector, ILogger logger, WebHookStoreContext context)
        //    : base(protector, logger, context)
        //{
        //    if (settings == null)
        //    {
        //        throw new ArgumentNullException(nameof(settings));
        //    }
        //}

        ///// <summary>
        ///// Initializes a new instance of the <see cref="SqlWebHookStore"/> class with the given <paramref name="settings"/>,
        ///// <paramref name="protector"/>, <paramref name="logger"/>, <paramref name="nameOrConnectionString"/>, <paramref name="schemaName"/> and <paramref name="tableName"/>.
        ///// Using this constructor, the data will be encrypted using the provided <paramref name="protector"/>.
        ///// </summary>
        //public SqlWebHookStore(
        //    SettingsDictionary settings,
        //    IDataProtector protector,
        //    ILogger logger,
        //    WebHookStoreContext context,
        //    string nameOrConnectionString,
        //    string schemaName,
        //    string tableName) : base(protector, logger, context)
        //{
        //    if (settings == null)
        //    {
        //        throw new ArgumentNullException(nameof(settings));
        //    }

        //    _nameOrConnectionString = nameOrConnectionString;
        //    _schemaName = schemaName;
        //    _tableName = tableName;
        //}

        ///// <summary>
        ///// Provides a static method for creating a standalone <see cref="SqlWebHookStore"/> instance which will
        ///// encrypt the data to be stored using <see cref="IDataProtector"/>.
        ///// </summary>
        ///// <param name="logger">The <see cref="ILogger"/> instance to use.</param>
        ///// <param name="context">The <see cref="WebHookStoreContext"/> instance to use.</param>
        ///// <returns>An initialized <see cref="SqlWebHookStore"/> instance.</returns>
        //public static IWebHookStore CreateStore(ILogger logger, WebHookStoreContext context)
        //{
        //    return CreateStore(logger, context, encryptData: true);
        //}

        ///// <summary>
        ///// Provides a static method for creating a standalone <see cref="SqlWebHookStore"/> instance.
        ///// </summary>
        ///// <param name="logger">The <see cref="ILogger"/> instance to use.</param>
        ///// <param name="context">The <see cref="WebHookStoreContext"/> instance to use.</param>
        ///// <param name="encryptData">Indicates whether the data should be encrypted using <see cref="IDataProtector"/> while persisted.</param>
        ///// <returns>An initialized <see cref="SqlWebHookStore"/> instance.</returns>
        //public static IWebHookStore CreateStore(ILogger logger, WebHookStoreContext context, bool encryptData)
        //{
        //    return CreateStore(logger, context, encryptData, nameOrConnectionString: null, schemaName: null, tableName: null);
        //}

        ///// <summary>
        ///// Provides a static method for creating a standalone <see cref="SqlWebHookStore"/> instance.
        ///// </summary>
        ///// <param name="logger">The <see cref="ILogger"/> instance to use.</param>
        ///// <param name="context">The <see cref="WebHookStoreContext"/> instance to use.</param>
        ///// <param name="encryptData">Indicates whether the data should be encrypted using <see cref="IDataProtector"/> while persisted.</param>
        ///// <param name="nameOrConnectionString">The custom connection string or name of the connection string application setting. Used to initialize <see cref="WebHookStoreContext"/>.</param>
        ///// <param name="schemaName">The custom name of database schema. Used to initialize <see cref="WebHookStoreContext"/>.</param>
        ///// <param name="tableName">The custom name of database table. Used to initialize <see cref="WebHookStoreContext"/>.</param>
        ///// <returns>An initialized <see cref="SqlWebHookStore"/> instance.</returns>
        //public static IWebHookStore CreateStore(
        //    ILogger logger,
        //    WebHookStoreContext context,
        //    bool encryptData,
        //    string nameOrConnectionString,
        //    string schemaName,
        //    string tableName)
        //{
        //    var settings = CommonServices.GetSettings();
        //    IWebHookStore store;
        //    if (encryptData)
        //    {
        //        var protector = DataSecurity.GetDataProtector();
        //        store = new SqlWebHookStore(settings, protector, logger, context, nameOrConnectionString, schemaName, tableName);
        //    }
        //    else
        //    {
        //        store = new SqlWebHookStore(settings, logger, context, nameOrConnectionString, schemaName, tableName);
        //    }
        //    return store;
        //}

        ////internal static string CheckSqlStorageConnectionString(SettingsDictionary settings)
        ////{
        ////    if (settings == null)
        ////    {
        ////        throw new ArgumentNullException(nameof(settings));
        ////    }

        ////    if (!settings.Connections.TryGetValue(WebHookStoreContext.ConnectionStringName, out var connection) ||
        ////        connection == null ||
        ////        string.IsNullOrEmpty(connection.ConnectionString))
        ////    {
        ////        var message = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_NoConnectionString, WebHookStoreContext.ConnectionStringName);
        ////        throw new InvalidOperationException(message);
        ////    }
        ////    return connection.ConnectionString;
        ////}
    }
}
