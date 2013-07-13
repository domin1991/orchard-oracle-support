Orchard-support-Oracle
======================

This is the Orchard sourcecode which can install in Oracle 11g.

Orchard修改支持Oracle数据库，具体参考http://orchard.codeplex.com/discussions/401440，同时结合自己实际操作，现在已经能成功运行。

具体步骤如下：

1、准备数据库

    Orcale数据库不支持数据自增，因此要使用id自动增加就需要建立序列在每次插入数据的时候就要获取序列一次。

    Execute SQL query: create sequence hibernate_sequence

2、限制标识符长度。Oracle中标识符（表名和列名）不能超过30个字符。

    a）确保自定义模块中的数据库记录的所有字段名不超过30个字符;

    b）替换“ContentFieldDefinitionRecord”为“ContentFieldDefRecord”;

    在以下文件中：

    src\Orchard.Tests.Modules\Widgets\Services\WidgetsServiceTest.cs

    src\Orchard.Web\Core\Settings\Metadata\ContentDefinitionManager.cs

    src\Orchard.Web\Core\Settings\Metadata\Records\ContentFieldDefinitionRecord.cs

    src\Orchard.Web\Core\Settings\Metadata\Records\ContentPartFieldDefinitionRecord.cs

    src\Orchard.Web\Core\Settings\Migrations.cs

    src\Orchard.Core.Tests\Settings\Metadata\ContentDefinitionManagerTests.cs

    c）创建一个实用工具类，将所有的表名变短。（在我的项目，在类TableAliasGenerator中有一个返回唯一表别名的函数）；

    在我实际操作过程中，只是简单采用截取30个字符（因为我试验发现好像Orchard中，有两个表名到第29个字符才不一样，同时这样截取是不能再加前缀，否则字数将超过30。）

    d) 使用TableAliasGenerator。

    i. Orchard.Setup.Services.SetupService.Setup function:

var tableName = TableAliasGenerator.Generate(tablePrefix + "Settings_ShellDescriptorRecord");
    ii. (Orchard.Framework)Orchard.Data.Migration.Schema.SchemaBuilder. Aplly it in all statements like this:

CreateTableCommand(TableAliasGenerator.Generate(String.Concat(_formatPrefix(_featurePrefix), name)));
    iii. (Orchard.Framework)Orchard.Environment.ShellBuilders.CompositionStrategy.BuildRecord function:

return new RecordBlueprint {
  Type = type,
  Feature = feature,
  TableName = TableAliasGenerator.Generate(dataTablePrefix + extensionName + '_' + type.Name);
}
3、使所有列名变为大写。Oracle数据库在执行“select”时，所有列名是转换为大写字母执行的。因此Orchard里定义的列名是不能在Oracle中访问的。

private string columnName;
public string ColumnName
{
   get { return columnName; }
   set { columnName = value.ToUpper(); }
}
4、给标识符添加引号。

    修改Orchard.Data.Migration.Interpreters.DefaultDataMigrationInterpreter.Visit(CreateTableCommand command) function:

builder.Append(_dialect.PrimaryKeyString) .Append(" ( ") .Append(String.Join(", ", primaryKeys.Select(key => "\"" + key + "\"").ToArray())) .Append(" )");
5、在所有'create table'查询中，将“NVARCHAR2”数据类型改为“VARCHAR2”。在Orchard中有些列是NVARCHAR2(2048)类型，但是在Oracle中限制为NVARCHAR2(2000)，因此采用VARCHAR2替代NVARCHAR2。

    在Orchard.Data.Migration.Interpreters.DefaultDataMigrationInterpreter.GetTypeName function末尾添加:

if (_dialect is NHibernate.Dialect.Oracle9iDialect)
{
   result = result.Replace("NVARCHAR2", "VARCHAR2");
}//result - is a return value
6、替换“Number”为“Number_”。因为在Oracle数据库中，“Number”是保留字。

    在以下文件中：

    Orchard.ContentManagement.Records.ContentItemVersionRecord

    Orchard.ContentManagement.DataMigrations.FrameworkDataMigration

    Orchard.Tests.ContentManagement.DefaultContentManagerTests

    Orchard.ContentManagement.ContentItem

    Orchard.ContentManagement.DefaultContentManager

7、Oracle数据库不支持空字符串，将空字符串认为是null。

    修改 Orchard.Alias.Implementation.Updater.AliasHolderUpdater.Refresh function:

_aliasHolder.SetAliases(aliases.Select(alias => new AliasInfo { Path = alias.Item1 ?? string.Empty, Area = alias.Item2, RouteValues = alias.Item3 }));
8、实现OracleDataServiceProvider。

    类似MySqlDataServiceProvider。

9、应用OracleDataServiceProvider

    a) Add in Orchard.Setup.Index.cshtml:

<div> 
   @Html.RadioButtonFor(svm => svm.DatabaseProvider, Orchard.Setup.Controllers.SetupDatabaseType.Oracle.ToString(), new { id = "oracle" }) 
   <label for="oracle" class="forcheckbox">@T("Use an existing Oracle database")</label> 
</div>
    b) Add 'Oracle' value to Orchard.Setup.Controllers.SetupDatabaseType enum

    c) Add to Orchard.Setup.Controllers.SetupController.IndexPOST function the following code:

case SetupDatabaseType.Oracle: 
   providerName = "Oracle"; 
   break;
10、采用自治事务解决Oracle中DDl操作时ORA-02089: COMMIT 不允许在附属会话中的问题错误

    修改 Orchard.Data.Migration.Interpreters.DefaultDataMigrationInterpreter.Visit(CreateTableCommand command) function:

public override void Visit(CreateTableCommand command) {
   if (ExecuteCustomInterpreter(command)) {
       return;
   }
   var builder = new StringBuilder();
   builder
       .Append("DECLARE pragma autonomous_transaction; BEGIN EXECUTE immediate '")
       .Append(" ");
       .Append(_dialect.CreateMultisetTableString)
       .Append(_dialect.QuoteForTableName(PrefixTableName(command.Name.ToUpper())))
       .Append(" (");
   var appendComma = false;
   foreach (var createColumn in command.TableCommands.OfType<CreateColumnCommand>()) {
       if (appendComma) {
           builder.Append(", ");
       }
       appendComma = true;
       Visit(builder, createColumn);
   }
   var primaryKeys = command.TableCommands.OfType<CreateColumnCommand>().Where(ccc => ccc.IsPrimaryKey).Select(ccc => ccc.ColumnName);
   if (primaryKeys.Any()) {
       if (appendComma) {
           builder.Append(", ");
       }
       builder.Append(_dialect.PrimaryKeyString)
           .Append(" ( ")
           .Append(String.Join(", ", primaryKeys.Select(key => "\"" + key + "\"").ToArray()))
           .Append(" )");
   }
   builder.Append(" )");
   builder.Append("'; END;");
   _sqlStatements.Add(builder.ToString());
   RunPendingStatements();
}
    实质是添加了以下两句话

builder.Append("DECLARE pragma autonomous_transaction; BEGIN EXECUTE immediate '");
builder.Append("'; END;");
    同时修改Orchard.Data.Migration.Interpreters.DefaultDataMigrationInterpreter.ConvertToSqlValue(object value) function:

case TypeCode.Char:
   return String.Concat("''", Convert.ToString(value).Replace("'", "''"), "''");
    修改完成的源代码我已放在github上，地址https://github.com/GAUDJ/Orchard-support-Oracle
