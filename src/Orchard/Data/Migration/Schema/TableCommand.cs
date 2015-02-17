namespace Orchard.Data.Migration.Schema {
    public class TableCommand : ISchemaBuilderCommand{
        public string TableName { get; set; }

        public TableCommand(string tableName) {
            TableName = tableName;
        }

    }
}
