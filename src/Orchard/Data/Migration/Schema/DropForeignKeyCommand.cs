namespace Orchard.Data.Migration.Schema {
    public class DropForeignKeyCommand : SchemaCommand {
        public string SrcTable { get; set; }

        public DropForeignKeyCommand(string srcTable, string name)
            : base(name, SchemaCommandType.DropForeignKey) {
            SrcTable = srcTable;
        }
    }
}
