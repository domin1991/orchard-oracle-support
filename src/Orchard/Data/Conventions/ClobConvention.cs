using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Data.Conventions
{
    public class ClobConvention : AttributePropertyConvention<StringLengthMaxAttribute>
    {
        private ClobConvention() { }

        public static ClobConvention Create()
        {
            return new ClobConvention();
        }
        protected override void Apply(StringLengthMaxAttribute attribute, IPropertyInstance instance)
        {
            instance.CustomSqlType("CLOB");
            instance.CustomType("StringClob");
        }
    }
}
