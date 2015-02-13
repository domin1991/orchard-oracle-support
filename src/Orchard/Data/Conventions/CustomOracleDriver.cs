using NHibernate.Driver;
using NHibernate.SqlTypes;
using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Orchard.Data.Conventions
{
    public class CustomOracleDriver : OracleClientDriver
    {
        protected override void InitializeParameter(System.Data.IDbDataParameter dbParam, string name, SqlType sqlType)
        {
            base.InitializeParameter(dbParam, name, sqlType);
            if ((sqlType is StringClobSqlType))
            {
                ((OracleParameter)dbParam).OracleType = OracleType.NClob;
            }
        }
    }
}
