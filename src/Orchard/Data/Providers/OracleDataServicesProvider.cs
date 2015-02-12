using System;
using System.Linq;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Automapping;
using System.Collections.Generic;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.Data.Conventions;

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
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentException("The connection string is empty");
            }
            persistence = persistence.ConnectionString(_connectionString);
            return persistence;
        }

        public static string GetAlias(string name)
        {
            if (name.LastIndexOf('_') < 0)
            {
                return name.Length > 30 ? name.Substring(0, 30).ToUpper() : name.ToUpper();
            }
            var prefix = name.Substring(0, name.LastIndexOf('_'));
            prefix = new string(prefix.ToCharArray().Where(c => String.Equals(c, char.ToUpper(c))).ToArray());
            var result = string.Format("{0}{1}", prefix, name.Substring(name.LastIndexOf('_')));
            result = result.Length > 30 ? result.Substring(0, 30).ToUpper() : result.ToUpper();
            return result;
        }

        public static int GetColumnLength(int length)
        {
            return length > 2000 ? 2000 : length;
        }

        protected override void AdjustPersistantModel(AutoPersistenceModel persistanceModel)
        {
            persistanceModel.Conventions.Setup(x =>
            {
                x.Add(CustomForeignKeyConvention.Create());
                x.Add(ColumnNameConvention.Create());
            });
        }
    }
}