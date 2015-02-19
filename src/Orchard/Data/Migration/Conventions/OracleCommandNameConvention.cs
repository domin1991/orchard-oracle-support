using Orchard.Data.Migration.Schema;
using Orchard.Data.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Environment.Configuration;

namespace Orchard.Data.Migration.Convention
{
    public interface ICommandNameConvention : IDependency
    {
        SchemaCommand ChangeCommand(SchemaCommand command);
    }

    public class OracleCommandNameConvention : ICommandNameConvention
    {
        ShellSettings _shellSettings;

        public OracleCommandNameConvention(ShellSettings shellSettings)
        {
            _shellSettings = shellSettings;
        }

        private string PrefixName(string name)
        {
            if (string.IsNullOrEmpty(_shellSettings.DataTablePrefix))
            {
                return name;
            }
            return _shellSettings.DataTablePrefix + "_" + name;
        }

        public Schema.SchemaCommand ChangeCommand(Schema.SchemaCommand command)
        {
            switch (command.Type)
            {
                case SchemaCommandType.CreateTable:
                    Change((CreateTableCommand)command);
                    break;
                case SchemaCommandType.AlterTable:
                    Change((AlterTableCommand)command);
                    break;
                case SchemaCommandType.DropTable:
                    Change((DropTableCommand)command);
                    break;
                case SchemaCommandType.SqlStatement:
                    Change((SqlStatementCommand)command);
                    break;
                case SchemaCommandType.CreateForeignKey:
                    Change((CreateForeignKeyCommand)command);
                    break;
                case SchemaCommandType.DropForeignKey:
                    Change((DropForeignKeyCommand)command);
                    break;
            }
            return command;
        }

        private void Change(DropForeignKeyCommand command)
        {
            command.SrcTable = OracleNameService.Normalize(PrefixName(command.SrcTable));
            command.Name = OracleNameService.Normalize(command.Name);
        }

        private void Change(CreateForeignKeyCommand command)
        {
            command.SrcTable = OracleNameService.Normalize(PrefixName(command.SrcTable));
            command.SrcColumns = command.SrcColumns.Select(n => OracleNameService.Normalize(n)).ToArray();
            command.Name = OracleNameService.Normalize(command.Name);
            command.DestTable = OracleNameService.Normalize(PrefixName(command.SrcTable));
            command.DestColumns = command.DestColumns.Select(n => OracleNameService.Normalize(n)).ToArray();
        }

        private void Change(SqlStatementCommand command)
        {

        }

        private void Change(DropTableCommand command)
        {
            command.Name = OracleNameService.Normalize(PrefixName(command.Name));
        }

        private void Change(AlterTableCommand command)
        {
            foreach (var item in command.TableCommands.OfType<DropColumnCommand>())
            {
                item.TableName = OracleNameService.Normalize(PrefixName(item.TableName));
                item.ColumnName = OracleNameService.Normalize(item.ColumnName);
            }
            foreach (var item in command.TableCommands.OfType<AddColumnCommand>())
            {
                item.TableName = OracleNameService.Normalize(PrefixName(item.TableName));
                item.ColumnName = OracleNameService.Normalize(item.ColumnName);
            }
            foreach (var item in command.TableCommands.OfType<AlterColumnCommand>())
            {
                item.TableName = OracleNameService.Normalize(PrefixName(item.TableName));
                item.ColumnName = OracleNameService.Normalize(item.ColumnName);
            }
            foreach (var item in command.TableCommands.OfType<AddIndexCommand>())
            {
                item.TableName = OracleNameService.Normalize(PrefixName(item.TableName));
                item.IndexName = OracleNameService.Normalize(PrefixName(item.IndexName));
                item.ColumnNames = item.ColumnNames.Select(x => OracleNameService.Normalize(x)).ToArray();
            }
            foreach (var item in command.TableCommands.OfType<DropIndexCommand>())
            {
                item.TableName = OracleNameService.Normalize(PrefixName(item.TableName));
                item.IndexName = OracleNameService.Normalize(PrefixName(item.IndexName));
            }
        }

        private void Change(CreateTableCommand command)
        {
            command.Name = OracleNameService.Normalize(PrefixName(command.Name));
            foreach (var createColumn in command.TableCommands.OfType<CreateColumnCommand>())
            {
                createColumn.ColumnName = OracleNameService.Normalize(createColumn.ColumnName);
            }
        }
    }
}
