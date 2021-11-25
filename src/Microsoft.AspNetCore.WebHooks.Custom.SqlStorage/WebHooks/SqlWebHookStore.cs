// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information

using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebHooks.Custom.SqlStorage.Properties;
using Microsoft.AspNetCore.WebHooks.Storage;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks;

/// <summary>
/// Provides an implementation of <see cref="IWebHookStore"/> storing registered WebHooks in Microsoft SQL Server.
/// </summary>
//[CLSCompliant(false)]
public class SqlWebHookStore : DbWebHookStore<WebHookStoreContext, Registration>
{
    private readonly ILogger<SqlWebHookStore> _logger;
    private readonly WebHookStoreContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlWebHookStore"/> class
    /// and <paramref name="logger"/>.
    /// Using this constructor, the data will not be encrypted while persisted to the database.
    /// </summary>
    public SqlWebHookStore(ILogger<SqlWebHookStore> logger, WebHookStoreContext context)
        : base(logger, context)
    {
        _context = context;
        _logger = logger;
    }

    public override async Task<StoreResult> DisableWebhookAsync(string id)
    {
        try
        {
            var registration = await _context.Set<Registration>()
                .Where(x => x.Id == id)
                .SingleOrDefaultAsync();

            if (registration == null)
            {
                return StoreResult.NotFound;
            }

            var webhook = ConvertToWebHook(registration);
            webhook.IsPaused = true;

            UpdateRegistrationFromWebHook(registration.User, webhook, registration);
            _context.Entry(registration).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException uex)
        {
            var error = uex.GetBaseException().Message;
            var message = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_SqlOperationFailed, "Insert", error);
            _logger.LogError(message, uex);
            return StoreResult.Conflict;
        }
        catch (DBConcurrencyException ocex)
        {
            var message = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_ConcurrencyError, "Update", ocex.Message);
            _logger.LogError(message, ocex);
            return StoreResult.Conflict;
        }
        catch (SqlException sqlex)
        {
            var message = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_SqlOperationFailed, "Update", sqlex.Message);
            _logger.LogError(message, sqlex);
            return StoreResult.OperationError;
        }
        catch (DbException dbex)
        {
            var message = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_SqlOperationFailed, "Update", dbex.Message);
            _logger.LogError(message, dbex);
            return StoreResult.OperationError;
        }
        catch (Exception ex)
        {
            var message = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_OperationFailed, "Update", ex.Message);
            _logger.LogError(message, ex);
            return StoreResult.InternalError;
        }
        return StoreResult.Success;
    }
}
