using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Orchard.Data.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Data
{
    public class OracleColumnNameConvention : IPropertyConvention
    {
        private List<string> PROHIBITED_NAMES = new List<string>{
            "NUMBER", "START"
        };

        private OracleColumnNameConvention() { }

        public static OracleColumnNameConvention Create()
        {
            return new OracleColumnNameConvention();
        }

        public void Apply(IPropertyInstance instance)
        {
            var columnName = OracleNameCutter.Cut(instance.Property.Name);
            columnName = WrapProhibitedNames(columnName);
            instance.Column(columnName);
        }

        private string WrapProhibitedNames(string columnName)
        {
            if (PROHIBITED_NAMES.Contains(columnName))
            {
                return "\"" + columnName + "\"";
            }
            return columnName;
        }
    }
}
