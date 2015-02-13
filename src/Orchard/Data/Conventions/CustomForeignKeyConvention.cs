using FluentNHibernate.Conventions;
using Orchard.Data.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Orchard.Data.Conventions
{
    public class CustomForeignKeyConvention : ForeignKeyConvention
    {
        private CustomForeignKeyConvention(){}

        public static CustomForeignKeyConvention Create()
        {
            return new CustomForeignKeyConvention();
        }

        protected override string GetKeyName(FluentNHibernate.Member property, Type type)
        {
            if (property == null)
                return OracleDataServicesProvider.GetAlias(type.Name + "_id");

            return OracleDataServicesProvider.GetAlias(property.Name + "_id");
        }
    }
}