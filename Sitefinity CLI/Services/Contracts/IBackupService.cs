using Microsoft.Extensions.Logging;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Services.Contracts
{
    public interface IBackupService
    {
        void Backup(UpgradeOptions upgradeOptions);

        void Restore(UpgradeOptions upgradeOptions, bool cleanup = false);
    }
}
