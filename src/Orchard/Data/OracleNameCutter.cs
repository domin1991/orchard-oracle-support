using Orchard.Data.Providers;
using Orchard.Environment.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Data
{
    public class OracleNameCutter
    {
        public static string Cut(string name)
        {
            string result = name;

            if (name.LastIndexOf('_') > 0)
            {
                var prefix = name.Substring(0, name.LastIndexOf('_'));
                var suffix = name.Substring(name.LastIndexOf('_'));
                prefix = new string(prefix.ToCharArray()
                    .Where(c => char.IsUpper(c))
                    .ToArray());
                result = string.Format("{0}{1}", prefix, suffix);
            }

            return result.Length > 30 ? result.Substring(0, 30).ToUpper() : result.ToUpper();
        }
    }
}
