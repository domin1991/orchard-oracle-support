using Orchard.Data.Migration.Schema;
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
}
