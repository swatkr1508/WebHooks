// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information

using System;
using Microsoft.AspNetCore.WebHooks.Storage;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Defines a <see cref="DbContext"/> which contains the set of WebHook <see cref="Registration"/> instances.
    /// </summary>
    public class WebHookStoreContext : DbContext
    {
        internal const string ConnectionStringName = "MS_SqlStoreConnectionString";
        private const string ConnectionStringNameParameter = "name=" + ConnectionStringName;
        //private readonly string _tableName;
        //private readonly string _schemaName = "WebHooks";

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookStoreContext"/> class.
        /// </summary>
        public WebHookStoreContext(DbContextOptions<WebHookStoreContext> options) : base(options)
        {

        }

        /// <summary>
        /// Gets or sets the current collection of <see cref="Registration"/> instances.
        /// </summary>
        public virtual DbSet<Registration> Registrations { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            //modelBuilder.HasDefaultSchema(_schemaName);


            var registrationConfiguration = modelBuilder.Entity<Registration>();
            registrationConfiguration.HasKey(x => new { x.Id, x.User });

        }
    }
}
