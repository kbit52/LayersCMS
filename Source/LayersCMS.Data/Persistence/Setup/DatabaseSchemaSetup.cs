﻿using System.Configuration;
using LayersCMS.Data.Domain.Core.Media;
using LayersCMS.Data.Domain.Core.Pages;
using LayersCMS.Data.Domain.Core.Security;
using LayersCMS.Data.Domain.Core.Settings;
using LayersCMS.Util.Security.Interfaces;
using ServiceStack.OrmLite;
using System;
using System.Data;

namespace LayersCMS.Data.Persistence.Setup
{
    /// <summary>
    /// Constructs the database schema. Should only be run once, and access to run the code should be protected in the UI somehow,
    /// as this will drop and recreate all tables.
    /// </summary>
    public class DatabaseSchemaSetup
    {
        private readonly IHashHelper _hashHelper;

        /// <param name="hashHelper">The hashing helper to hash the user's password</param>
        public DatabaseSchemaSetup(IHashHelper hashHelper)
        {
            _hashHelper = hashHelper;
        }

        /// <summary>
        /// Drops all tables matching the name of the LayersCmsDomainObjects to be created,
        /// then creates fresh tables for those domain objects.
        /// </summary>
        public void InitialiseCoreTables(DatabaseSetupConfig config)
        {
            // Get the connection string
            ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings[config.ConnectionStringName];
            if (connectionString == null)
                throw new NullReferenceException("No connection string exists by that key.");

            // Tell the configuration to use unicode, e.g. nvarchar compared to varchar in SQL Server
            config.DatabaseDialect.UseUnicode = true;

            // Initialise the data connection factory
            var dbFactory = new OrmLiteConnectionFactory(connectionString.ConnectionString, false, config.DatabaseDialect);

            // Check the config class passed is valid, and we have all we need to complete the setup
            if (config.DatabaseDialect == null)
                throw new NullReferenceException("No DatabaseDialect specified. Cannot initialise a database without knowing what type of database to use.");

            if (String.IsNullOrWhiteSpace(config.UserEmailAddress))
                throw new NullReferenceException("No UserEmailAddress specified. Cannot create primary user without an email address.");

            if (String.IsNullOrWhiteSpace(config.UserPassword))
                throw new NullReferenceException("No UserPassword specified. Cannot create primary user without a password.");

            
            // Open a database connection
            using (IDbConnection dbConn = dbFactory.OpenDbConnection())
            {
                // Create the LayersCmsPage table
                dbConn.DropAndCreateTable<LayersCmsPage>();

                // Add the homepage
                dbConn.Save(new LayersCmsPage()
                    {
                        Active = true,
                        Content = "<p>Welcome to Layers CMS</p>",
                        DisplayName = "Home",
                        PageTitle = "Index",
                        ParentId = null,
                        PublishEnd = null,
                        PublishStart = DateTime.Now.Date,
                        RedirectTypeEnum = RedirectTypeEnum.None,
                        RedirectUrl = null,
                        ShowInNavigation = true,
                        SortOrder = 0,
                        Url = "/",
                        WindowTitle = "Index"
                    });

                // Create the LayersCmsUser table and insert the first user
                dbConn.DropAndCreateTable<LayersCmsUser>();
                dbConn.Save(new LayersCmsUser()
                    {
                        Active = true,
                        EmailAddress = config.UserEmailAddress,
                        Password = _hashHelper.HashString(config.UserPassword) // A hashed version of the plain text password
                    });

                // Create the LayersCmsImage table
                dbConn.DropAndCreateTable<LayersCmsImage>();

                // Create the settings table and add some default settings
                dbConn.DropAndCreateTable<LayersCmsSetting>();
                dbConn.SaveAll(new []
                    {
                        new LayersCmsSetting(){SettingType = LayersCmsSettingType.ContactEmailAddress, Value = "email@address.com"},
                        new LayersCmsSetting(){SettingType = LayersCmsSettingType.ContactTelephoneNumber, Value = "0000 000000"},
                        new LayersCmsSetting(){SettingType = LayersCmsSettingType.GoogleAnalyticsAccountId, Value = ""}
                    });

            }            
        }
    }
}
