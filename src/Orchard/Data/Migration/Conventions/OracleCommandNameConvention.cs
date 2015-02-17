using Orchard.Data.Migration.Schema;
using Orchard.Data.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Data.Migration.Convention
{
    public interface ICommandNameConvention : IDependency
    {
        SchemaCommand ChangeCommand(SchemaCommand command);
    }

    public class OracleCommandNameConvention : ICommandNameConvention
    {
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
            command.SrcTable = OracleNameCutter.Cut(command.SrcTable);
            command.Name = OracleNameCutter.Cut(command.Name);
        }

        private void Change(CreateForeignKeyCommand command)
        {
            command.SrcTable = OracleNameCutter.Cut(command.SrcTable);
            command.Name = OracleNameCutter.Cut(command.Name);
            command.DestTable = OracleNameCutter.Cut(command.SrcTable);
        }

        private void Change(SqlStatementCommand command)
        {

        }

        private void Change(DropTableCommand command)
        {
            command.Name = OracleNameCutter.Cut(command.Name);
        }

        private void Change(AlterTableCommand command)
        {
            foreach (var item in command.TableCommands.OfType<DropColumnCommand>())
            {
                item.TableName = OracleNameCutter.Cut(item.TableName);
                item.ColumnName = OracleNameCutter.Cut(item.ColumnName);
            }
            foreach (var item in command.TableCommands.OfType<AddColumnCommand>())
            {
                item.TableName = OracleNameCutter.Cut(item.TableName);
                item.ColumnName = OracleNameCutter.Cut(item.ColumnName);
            }
            foreach (var item in command.TableCommands.OfType<AlterColumnCommand>())
            {
                item.TableName = OracleNameCutter.Cut(item.TableName);
                item.ColumnName = OracleNameCutter.Cut(item.ColumnName);
            }
            foreach (var item in command.TableCommands.OfType<AddIndexCommand>())
            {
                item.TableName = OracleNameCutter.Cut(item.TableName);
                item.IndexName = OracleNameCutter.Cut(item.IndexName);
                item.ColumnNames = item.ColumnNames.Select(x => OracleNameCutter.Cut(x)).ToArray();
            }
            foreach (var item in command.TableCommands.OfType<DropIndexCommand>())
            {
                item.TableName = OracleNameCutter.Cut(item.TableName);
                item.IndexName = OracleNameCutter.Cut(item.IndexName);
            }
        }

        private void Change(CreateTableCommand command)
        {
            command.Name = OracleNameCutter.Cut(command.Name);
            foreach (var createColumn in command.TableCommands.OfType<CreateColumnCommand>())
            {
                createColumn.ColumnName = OracleNameCutter.Cut(createColumn.ColumnName);
            }
        }
    }
}
