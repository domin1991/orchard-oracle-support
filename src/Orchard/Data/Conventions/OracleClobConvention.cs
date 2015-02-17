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
    public class OracleClobConvention : AttributePropertyConvention<StringLengthMaxAttribute>
    {
        private OracleClobConvention() { }

        public static OracleClobConvention Create()
        {
            return new OracleClobConvention();
        }

        protected override void Apply(StringLengthMaxAttribute attribute, IPropertyInstance instance)
        {
            instance.CustomSqlType("NCLOB");
            instance.CustomType("StringClob");
        }
    }
}
