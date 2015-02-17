using Orchard.Data.Providers;
using Orchard.Environment.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Data
{
    public class OracleNameService
    {
        private static readonly int MAX_LENGTH = 30;
        private static readonly string PK_SUFFIX = "_PK";
        private static readonly int MAX_LENGTH_FOR_PK = MAX_LENGTH - PK_SUFFIX.Length;
        private static readonly string DEFAULT_ORCHARD_ENTITY_SUFFIX = "Record";

        public static string Normalize(string name)
        {
            string result = name;

            if (IsMultipartName(name))
            {
                var prefix = name.Substring(0, name.LastIndexOf('_'));
                var mainName = name.Substring(name.LastIndexOf('_'));
                if (mainName.EndsWith(DEFAULT_ORCHARD_ENTITY_SUFFIX))
                {
                    mainName = mainName.Substring(0, mainName.LastIndexOf(DEFAULT_ORCHARD_ENTITY_SUFFIX));
                }
                
                result = string.Format("{0}{1}", ShortPrefix(prefix), mainName);
            }

            return (result.Length > MAX_LENGTH ? result.Substring(0, MAX_LENGTH) : result).ToUpper();
        }

        private static bool IsMultipartName(string name)
        {
            return name.Contains('_');
        }

        private static object ShortPrefix(string prefix)
        {
            return new string(prefix.ToCharArray()
                .Where(c => char.IsUpper(c) || c == '_')
                .ToArray());
        }

        public static string PrimaryKey(string name)
        {
            name = Normalize(name);
            if (name.Length > MAX_LENGTH_FOR_PK)
            {
                name = name.Substring(0, MAX_LENGTH_FOR_PK);
            }
            return name + PK_SUFFIX;
        }
    }
}
