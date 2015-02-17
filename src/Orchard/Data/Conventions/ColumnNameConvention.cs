using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Orchard.Data.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Data
{
    public class ColumnNameConvention : IPropertyConvention
    {
        private ColumnNameConvention() { }

        public static ColumnNameConvention Create()
        {
            return new ColumnNameConvention();
        }

        public void Apply(IPropertyInstance instance)
        {
            instance.Column(OracleDataServicesProvider.GetAlias(instance.Property.Name));
        }
    }
}
