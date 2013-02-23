using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard
{
    public class TableAliasGenerator
    {
        //修改链接Oracle数据库-缩短数据库表名类(直接截取30个字符)
        public static string Generate(string tablename)
        {
            int nameLength = tablename.Length;

            return tablename.Substring(0, nameLength > 30 ? 30 : nameLength).ToUpper();
        }
    }
}
