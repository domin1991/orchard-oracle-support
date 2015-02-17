using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Orchard.Data.Providers;
using Orchard.Environment.ShellBuilders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Data.Conventions
{
    public class OracleTableNameConvention : IClassConvention
    {
        public static OracleTableNameConvention Create()
        {
            return new OracleTableNameConvention();
        }

        private OracleTableNameConvention() { }

        public void Apply(IClassInstance instance)
        {
            instance.Table(OracleNameCutter.Cut(instance.TableName));
        }
    }
}
