using NHibernate.Dialect;
using NHibernate.SqlTypes;
using Orchard.Data.Migration.Convention;
using Orchard.Data.Migration.Schema;
using Orchard.Data.Providers;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Orchard.Data.Migration.Interpreters
{
    public class OracleCommandInterpreter :
        ICommandInterpreter<AlterTableCommand>,
        ICommandInterpreter<CreateTableCommand>,
        ICommandInterpreter<DropTableCommand>,
        ICommandInterpreter<SqlStatementCommand>,
        ICommandInterpreter<CreateForeignKeyCommand>,
        ICommandInterpreter<DropForeignKeyCommand>
    {
        private ICommandNameConvention _commandNameConvention;
        private readonly Dialect _dialect;
        private readonly ShellSettings _shellSettings;
        private const char Space = ' ';

        public string DataProvider
        {
            get { return "Oracle"; }
        }

        public Localizer T { get; set; }

        public OracleCommandInterpreter(ShellSettings shellSettings, ISessionFactoryHolder sessionFactoryHolder, ICommandNameConvention commandNameConvention)
        {
            T = NullLocalizer.Instance;
            _commandNameConvention = commandNameConvention;
            _shellSettings = shellSettings;
            var configuration = sessionFactoryHolder.GetConfiguration();
            _dialect = Dialect.GetDialect(configuration.Properties);
        }

        public string[] CreateStatements(AlterTableCommand command)
        {
            _commandNameConvention.ChangeCommand(command);
            List<string> sqlStatements = new List<string>();

            if (command.TableCommands.Count == 0)
            {
                return sqlStatements.ToArray();
            }

            // drop columns
            foreach (var dropColumn in command.TableCommands.OfType<DropColumnCommand>())
            {
                var builder = new StringBuilder();
                Visit(builder, dropColumn);
                sqlStatements.Add(builder.ToString());
            }

            // add columns
            foreach (var addColumn in command.TableCommands.OfType<AddColumnCommand>())
            {
                var builder = new StringBuilder();
                Visit(builder, addColumn);
                sqlStatements.Add(builder.ToString());
            }

            // alter columns
            foreach (var alterColumn in command.TableCommands.OfType<AlterColumnCommand>())
            {
                var builder = new StringBuilder();
                Visit(builder, alterColumn);
                sqlStatements.Add(builder.ToString());
            }

            // add index
            foreach (var addIndex in command.TableCommands.OfType<AddIndexCommand>())
            {
                var builder = new StringBuilder();
                Visit(builder, addIndex);
                sqlStatements.Add(builder.ToString());
            }

            // drop index
            foreach (var dropIndex in command.TableCommands.OfType<DropIndexCommand>())
            {
                var builder = new StringBuilder();
                Visit(builder, dropIndex);
                sqlStatements.Add(builder.ToString());
            }
            return sqlStatements.ToArray();
        }

        public string[] CreateStatements(CreateTableCommand command)
        {
            _commandNameConvention.ChangeCommand(command);
            var builder = new StringBuilder();
            builder.Append("DECLARE pragma autonomous_transaction; BEGIN EXECUTE immediate '");
            builder.Append(_dialect.CreateMultisetTableString)
                .Append(' ')
                .Append(_dialect.QuoteForTableName(command.Name))
                .Append(" (");

            var appendComma = false;
            foreach (var createColumn in command.TableCommands.OfType<CreateColumnCommand>())
            {
                if (appendComma)
                {
                    builder.Append(", ");
                }
                appendComma = true;

                Visit(builder, createColumn);
            }

            var primaryKeys = command.TableCommands.OfType<CreateColumnCommand>().Where(ccc => ccc.IsPrimaryKey).Select(ccc => ccc.ColumnName).ToArray();
            if (primaryKeys.Any())
            {
                if (appendComma)
                {
                    builder.Append(", ");
                }

                builder.Append(_dialect.PrimaryKeyString)
                    .Append(" ( ")
                    .Append(String.Join(", ", primaryKeys.ToArray()))
                    .Append(" )");
            }

            builder.Append(" )");
            builder.Append("'; END;");

            return new[] {
                builder.ToString()
            };
        }

        public string[] CreateStatements(DropTableCommand command)
        {
            _commandNameConvention.ChangeCommand(command);
            var builder = new StringBuilder();

            builder.Append(_dialect.GetDropTableString(command.Name));

            return new string[] { builder.ToString() };
        }

        public string[] CreateStatements(SqlStatementCommand command)
        {
            _commandNameConvention.ChangeCommand(command);
            if (command.Providers.Count != 0 && !command.Providers.Contains(_shellSettings.DataProvider))
            {
                return new string[0];
            }
            return new string[] { command.Sql };
        }

        public string[] CreateStatements(CreateForeignKeyCommand command)
        {
            _commandNameConvention.ChangeCommand(command);
            var builder = new StringBuilder();

            builder.Append("alter table ")
                .Append(_dialect.QuoteForTableName(command.SrcTable));

            builder.Append(_dialect.GetAddForeignKeyConstraintString(command.Name,
                command.SrcColumns,
                _dialect.QuoteForTableName(command.DestTable),
                command.DestColumns,
                false));

            return new string[] { builder.ToString() };
        }

        public string[] CreateStatements(DropForeignKeyCommand command)
        {
            _commandNameConvention.ChangeCommand(command);
            var builder = new StringBuilder();

            builder.AppendFormat("alter table {0} drop constraint {1}", _dialect.QuoteForTableName(command.SrcTable), command.Name);
            return new string[] { builder.ToString() };
        }

        private void Visit(StringBuilder builder, CreateColumnCommand command)
        {
            // name
            builder.Append(_dialect.QuoteForColumnName(command.ColumnName)).Append(Space);

            if (!command.IsIdentity || _dialect.HasDataTypeInIdentityColumn)
            {
                {
                    builder.Append(GetTypeName(_dialect, command.DbType, command.Length, command.Precision, command.Scale, _shellSettings.DataProvider));
                }
            }

            // append identity if handled
            if (command.IsIdentity && _dialect.SupportsIdentityColumns)
            {
                builder.Append(Space).Append(_dialect.IdentityColumnString);
            }

            // [default value]
            if (command.Default != null)
            {
                builder.Append(" default ").Append(ConvertToSqlValue(command.Default, _shellSettings.DataProvider)).Append(Space);
            }

            // nullable
            builder.Append(command.IsNotNull
                               ? " not null"
                               : !command.IsPrimaryKey && !command.IsUnique
                                     ? _dialect.NullColumnString
                                     : string.Empty);

            // append unique if handled, otherwise at the end of the satement
            if (command.IsUnique && _dialect.SupportsUnique)
            {
                builder.Append(" unique");
            }

        }

        public void Visit(StringBuilder builder, AddColumnCommand command)
        {
            builder.AppendFormat("alter table {0} add ", _dialect.QuoteForTableName(command.TableName));

            Visit(builder, (CreateColumnCommand)command);
        }

        public void Visit(StringBuilder builder, DropColumnCommand command)
        {
            builder.AppendFormat("alter table {0} drop column {1}",
                _dialect.QuoteForTableName(command.TableName),
                _dialect.QuoteForColumnName(command.ColumnName));
        }

        public void Visit(StringBuilder builder, AlterColumnCommand command)
        {
            builder.AppendFormat("alter table {0} alter column {1} ",
                 _dialect.QuoteForTableName(command.TableName),
                 _dialect.QuoteForColumnName(command.ColumnName));

            // type
            if (command.DbType != DbType.Object)
            {
                builder.Append(GetTypeName(_dialect, command.DbType, command.Length, command.Precision, command.Scale));
            }
            else
            {
                if (command.Length > 0 || command.Precision > 0 || command.Scale > 0)
                {
                    throw new OrchardException(T("Error while executing data migration: you need to specify the field's type in order to change its properties"));
                }
            }

            // [default value]
            if (command.Default != null)
            {
                builder.Append(" set default ").Append(ConvertToSqlValue(command.Default)).Append(Space);
            }
        }

        public void Visit(StringBuilder builder, AddIndexCommand command)
        {
            builder.AppendFormat("create index {1} on {0} ({2}) ",
                _dialect.QuoteForTableName(command.TableName),
                _dialect.QuoteForColumnName(command.IndexName),
                String.Join(", ", command.ColumnNames)
                );
        }

        public void Visit(StringBuilder builder, DropIndexCommand command)
        {
            builder.AppendFormat("drop index {0} ON {1}",
                _dialect.QuoteForColumnName(command.IndexName),
                _dialect.QuoteForTableName(command.TableName));
        }

        public static string GetTypeName(Dialect dialect, DbType dbType, int? length, byte precision, byte scale, string dataProvider)
        {
            var result = precision > 0
                       ? dialect.GetTypeName(new SqlType(dbType, precision, scale))
                       : length.HasValue
                             ? dialect.GetTypeName(new SqlType(dbType, length.Value))
                             : dialect.GetTypeName(new SqlType(dbType));

            if (length.HasValue && length.Value > 2000 && length.Value <= 4000 && result.StartsWith("NVARCHAR2"))
            {
                result = String.Format("VARCHAR2({0})", length.Value);
            }

            return result;
        }

        public static string GetTypeName(Dialect dialect, DbType dbType, int? length, byte precision, byte scale)
        {
            return GetTypeName(dialect, dbType, length, precision, scale, null);
        }

        public static string ConvertToSqlValue(object value, string dataProvider)
        {
            if (value == null)
            {
                return "null";
            }

            TypeCode typeCode = Type.GetTypeCode(value.GetType());
            switch (typeCode)
            {
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.String:
                case TypeCode.Char:
                    return String.Concat("''", Convert.ToString(value).Replace("'", "''"), "''");
                case TypeCode.Boolean:
                    return (bool)value ? "1" : "0";
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return Convert.ToString(value, CultureInfo.InvariantCulture);
                case TypeCode.DateTime:
                    return String.Concat("'", Convert.ToString(value, CultureInfo.InvariantCulture), "'");
            }

            return "null";
        }

        public static string ConvertToSqlValue(object value)
        {
            return ConvertToSqlValue(value, null);
        }
    }
}