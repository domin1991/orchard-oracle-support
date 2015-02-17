using System;
using System.Linq;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Automapping;
using System.Collections.Generic;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.Data.Conventions;
using NHibernate.Driver;
using NHibernate.SqlTypes;
using System.Data.OracleClient;
using FluentNHibernate.Conventions;

namespace Orchard.Data.Providers
{
    public class OracleDataServicesProvider : AbstractDataServicesProvider
    {
        private readonly string _dataFolder;
        private readonly string _connectionString;

        public OracleDataServicesProvider(string dataFolder, string connectionString)
        {
            _dataFolder = dataFolder;
            _connectionString = connectionString;
        }

        public static string ProviderName
        {
            get { return "Oracle"; }
        }

        public override IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase)
        {
            var persistence = OracleClientConfiguration.Oracle10;
            persistence.Driver<CustomOracleDriver>();
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentException("The connection string is empty");
            }
            persistence = persistence.ConnectionString(_connectionString);
            return persistence;
        }

        protected override void AdjustPersistantModel(AutoPersistenceModel persistanceModel)
        {
            persistanceModel.Conventions.Setup(x =>
            {
                x.Add(OracleCustomForeignKeyConvention.Create());
                x.Add(OracleColumnNameConvention.Create());
                x.Add(OracleClobConvention.Create());
                x.Add(OracleTableNameConvention.Create());
            });
        }

        public class CustomOracleDriver : OracleClientDriver
        {
            protected override void InitializeParameter(System.Data.IDbDataParameter dbParam, string name, SqlType sqlType)
            {
                base.InitializeParameter(dbParam, name, sqlType);

                // System.Data.OracleClient.dll driver generates an ORA-01461 exception because 
                // the driver mistakenly infers the column type of the string being saved, and 
                // tries forcing the server to update a LONG value into a CLOB/NCLOB column type. 
                // The reason for the incorrect behavior is even more obscure and only happens 
                // when all the following conditions are met.
                //   1.) IDbDataParameter.Value = (string whose length: 4000 > length > 2000 )
                //   2.) IDbDataParameter.DbType = DbType.String
                //   3.) DB Column is of type NCLOB/CLOB

                // The above is the default behavior for NHibernate.OracleClientDriver
                // So we use the built-in StringClobSqlType to tell the driver to use the NClob Oracle type
                // This will work for both NCLOB/CLOBs without issues.
                // Mapping file must be updated to use StringClob as the property type
                // See: http://thebasilet.blogspot.be/2009/07/nhibernate-oracle-clobs.html
                if ((sqlType is StringClobSqlType))
                {
                    ((OracleParameter)dbParam).OracleType = OracleType.NClob;
                }
            }
        }
    }
}