using Sitefinity_CLI.Model;
using Sitefinity_CLI.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Tests.UpgradeCommandTests.Mocks
{
    internal class BackupServiceMock : IBackupService
    {
        public void Backup(UpgradeOptions upgradeOptions)
        {
        }

        public void Restore(UpgradeOptions upgradeOptions, bool cleanup = false)
        {
        }
    }
}
