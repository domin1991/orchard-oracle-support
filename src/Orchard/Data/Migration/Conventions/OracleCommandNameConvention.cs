using Orchard.Data.Migration.Schema;
using Orchard.Data.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Data.Migration.Convention
{
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
            command.SrcTable = OracleDataServicesProvider.GetAlias(command.SrcTable);
            command.Name = OracleDataServicesProvider.GetAlias(command.Name);
        }

        private void Change(CreateForeignKeyCommand command)
        {
            command.SrcTable = OracleDataServicesProvider.GetAlias(command.SrcTable);
            command.Name = OracleDataServicesProvider.GetAlias(command.Name);
            command.DestTable = OracleDataServicesProvider.GetAlias(command.SrcTable);
                    
        }

        private void Change(SqlStatementCommand command)
        {
            //throw new NotImplementedException();
        }

        private void Change(DropTableCommand command)
        {
            command.Name = OracleDataServicesProvider.GetAlias(command.Name);
        }

        private void Change(AlterTableCommand command)
        {
            foreach (var item in command.TableCommands.OfType<DropColumnCommand>())
            {
                item.TableName = OracleDataServicesProvider.GetAlias(item.TableName);
                item.ColumnName = OracleDataServicesProvider.GetAlias(item.ColumnName);
            }
            foreach (var item in command.TableCommands.OfType<AddColumnCommand>())
            {
                item.TableName = OracleDataServicesProvider.GetAlias(item.TableName);
                item.ColumnName = OracleDataServicesProvider.GetAlias(item.ColumnName);
            }
            foreach (var item in command.TableCommands.OfType<AlterColumnCommand>())
            {
                item.TableName = OracleDataServicesProvider.GetAlias(item.TableName);
                item.ColumnName = OracleDataServicesProvider.GetAlias(item.ColumnName);
            }
            foreach (var item in command.TableCommands.OfType<AddIndexCommand>())
            {
                item.TableName = OracleDataServicesProvider.GetAlias(item.TableName);
                item.IndexName = OracleDataServicesProvider.GetAlias(item.IndexName);
                item.ColumnNames = item.ColumnNames.Select(x => OracleDataServicesProvider.GetAlias(x)).ToArray();
                
            }
            foreach (var item in command.TableCommands.OfType<DropIndexCommand>())
            {
                item.TableName = OracleDataServicesProvider.GetAlias(item.TableName);
                item.IndexName = OracleDataServicesProvider.GetAlias(item.IndexName);
            }
        }

        private void Change(CreateTableCommand command)
        {
            command.Name = OracleDataServicesProvider.GetAlias(command.Name);
            foreach (var createColumn in command.TableCommands.OfType<CreateColumnCommand>())
            {
                createColumn.ColumnName = OracleDataServicesProvider.GetAlias(createColumn.ColumnName);
            }
        }
        
    }
}
