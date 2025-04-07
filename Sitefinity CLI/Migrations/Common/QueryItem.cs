using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progress.Sitefinity.MigrationTool.ConsoleApp.Migrations.Common;
internal class QueryItem
{
    public bool IsGroup { get; set; }

    public string Join { get; set; }

    public string Name { get; set; }

    public string Value { get; set; }

    public Condition Condition { get; set; }
}
