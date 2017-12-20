using System.Collections.Generic;

namespace Sitefinity_CLI.Model
{
    public class CommandModel
    {
        public string Title { get; set; }

        public string Name { get; set; }

        public List<string> Args { get; set; }

        public List<OptionModel> Options { get; set; }
    }
}
