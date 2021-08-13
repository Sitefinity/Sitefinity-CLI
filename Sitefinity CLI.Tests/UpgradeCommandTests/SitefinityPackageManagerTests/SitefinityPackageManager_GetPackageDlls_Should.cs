using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sitefinity_CLI.PackageManagement;

namespace SitefinityCLI.Tests.UpgradeCommandTests.SitefinityPackageManagerTests
{
    [TestClass]
    public class SitefinityPackageManager_GetPackageDlls_Should
    {

        [TestMethod]
        public void Return_Dlls_ForTheSpecified_NetFrameworkVersion()
        {
            var sitefinitytPackagManager = new SitefinityPackageManager(null, null, null, null, null);

            var dataPath = $"{Directory.GetCurrentDirectory()}\\UpgradeCommandTests\\SitefinityPackageManagerTests\\Data\\NetFrameworkOnlyAssemblies";
            var netStandardDllPath = $"{dataPath}\\lib\\nedstandard2.0\\nedstandard2.0.dll";

            // netframework v.3.5
            var packgeDlls = sitefinitytPackagManager.GetPackageDlls(dataPath, "v3.5");
            var expectedPathForNet35 = $"{dataPath}\\lib\\net35\\net35.dll";
            Assert.IsTrue(packgeDlls.Contains(expectedPathForNet35), $"{expectedPathForNet35} was not found");
            Assert.IsFalse(packgeDlls.Contains(netStandardDllPath), $"{netStandardDllPath} should not be returned");

            // netFramework v 4.5
            packgeDlls = sitefinitytPackagManager.GetPackageDlls(dataPath, "v4.5");
            var expectedPathForNet45 = $"{dataPath}\\lib\\net45\\net45.dll";
            Assert.IsTrue(packgeDlls.Contains(expectedPathForNet45), $"{expectedPathForNet45} was not found");
            Assert.IsFalse(packgeDlls.Contains(netStandardDllPath), $"{netStandardDllPath} should not be returned");

            // netFramework v 4.6.1
            packgeDlls = sitefinitytPackagManager.GetPackageDlls(dataPath, "v4.6.1");
            var expectedPathForNet461 = $"{dataPath}\\lib\\net461\\net461.dll";
            Assert.IsTrue(packgeDlls.Contains(expectedPathForNet461), $"{expectedPathForNet461} was not found");
            Assert.IsFalse(packgeDlls.Contains(netStandardDllPath), $"{netStandardDllPath} should not be returned");

            // netFramework v 4.7.1
            packgeDlls = sitefinitytPackagManager.GetPackageDlls(dataPath, "v4.7.1");
            var expectedPathForNet471 = $"{dataPath}\\lib\\net471\\net471.dll";
            Assert.IsTrue(packgeDlls.Contains(expectedPathForNet471), $"{expectedPathForNet471} was not found");
            Assert.IsFalse(packgeDlls.Contains(netStandardDllPath), $"{netStandardDllPath} should not be returned");

            // netFramework v 4.7.2
            packgeDlls = sitefinitytPackagManager.GetPackageDlls(dataPath, "v4.7.2");
            var expectedPathForNet472 = $"{dataPath}\\lib\\net472\\net472.dll";
            Assert.IsTrue(packgeDlls.Contains(expectedPathForNet472), $"{expectedPathForNet472} was not found");
            Assert.IsFalse(packgeDlls.Contains(netStandardDllPath), $"{netStandardDllPath} should not be returned");

            // netFramework v 4.8
            packgeDlls = sitefinitytPackagManager.GetPackageDlls(dataPath, "v4.8");
            var expectedPathForNet48 = $"{dataPath}\\lib\\net48\\net48.dll";
            Assert.IsTrue(packgeDlls.Contains(expectedPathForNet48), $"{expectedPathForNet48} was not found");
            Assert.IsFalse(packgeDlls.Contains(netStandardDllPath), $"{netStandardDllPath} should not be returned");
        }

        [TestMethod]
        public void Return_Dlls_For_NetStandard_WhenThereAreNodllsForNetFramework()
        {
            var sitefinitytPackagManager = new SitefinityPackageManager(null, null, null, null, null);

            var dataPath = $"{Directory.GetCurrentDirectory()}\\UpgradeCommandTests\\SitefinityPackageManagerTests\\Data\\NetStandardOnlyAssemblies";
            var netStandardDllPath = $"{dataPath}\\lib\\netstandard2.0\\netstandard2.0.dll";

            // netframework v.3.5
            var packgeDlls = sitefinitytPackagManager.GetPackageDlls(dataPath, "v4.8");
            Assert.IsTrue(packgeDlls.Contains(netStandardDllPath), $"{netStandardDllPath} was not found");
        }
    }
}
