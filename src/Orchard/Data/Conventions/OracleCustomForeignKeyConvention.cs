using FluentNHibernate.Conventions;
using Orchard.Data.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Orchard.Data.Conventions
{
    public class OracleCustomForeignKeyConvention : ForeignKeyConvention
    {
        private OracleCustomForeignKeyConvention(){}

        public static OracleCustomForeignKeyConvention Create()
        {
            return new OracleCustomForeignKeyConvention();
        }

        protected override string GetKeyName(FluentNHibernate.Member property, Type type)
        {
            if (property == null)
            {
                return OracleNameCutter.Cut(type.Name + "_id");
            }

            return OracleNameCutter.Cut(property.Name + "_id");
        }
    }
}