using FluentNHibernate.Conventions;
using Orchard.Data.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Orchard.Data.Conventions
{
    public class OracleForeignKeyConvention : ForeignKeyConvention
    {
        private OracleForeignKeyConvention(){}

        public static OracleForeignKeyConvention Create()
        {
            return new OracleForeignKeyConvention();
        }

        protected override string GetKeyName(FluentNHibernate.Member property, Type type)
        {
            if (property == null)
            {
                return OracleNameService.Normalize(type.Name + "_id");
            }

            return OracleNameService.Normalize(property.Name + "_id");
        }
    }
}