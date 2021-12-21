using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSGraph.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph.Tests
{
    [TestClass]
    public class ExportDSMCmdletTest
    {
        static private PowerShell _powershell;

        [ClassInitialize()]
        public static void InitPsGraphUnitTest(TestContext tc)
        {
            _powershell = PowerShell.Create();
            _powershell.AddCommand("Import-Module")
                .AddParameter("Assembly", System.Reflection.Assembly.GetAssembly(typeof(PSGraph.NewPsGraphCmdlet)));
            _powershell.Invoke();
            _powershell.Commands.Clear();
        }

        [ClassCleanup()]
        public static void CleanupPsGraphUnitTest()
        {
            _powershell?.Dispose();
        }

        [TestMethod]
        public void ExportDSM_TEXT_Powershell_Success()
        {
            var graph = TestData.SimpleTestGraph2;
            var f = System.IO.Path.GetTempPath();
            var p = System.IO.Path.Combine(f, "export-test-DSM-TEXT.csv");

            try
            {
                _powershell.AddCommand("New-DSM");
                _powershell.AddParameters(new Dictionary<String, Object> {
                    { "Graph", graph }
                });
                var result = _powershell.Invoke();
                _powershell.Commands.Clear();

                _powershell.AddCommand("Export-DSM");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"Dsm", result[0].ImmediateBaseObject}, { "Format", DSMExportTypes.TEXT }, { "Path", p }
                    });
                result = _powershell.Invoke();

                var info = new System.IO.FileInfo(p);
                Assert.IsTrue(info != null);
                Assert.IsTrue(info.Exists);
                Assert.IsTrue(info.Length > 0);

            }
            finally
            {
                _powershell.Commands.Clear();
            }
        }

        [TestMethod]
        public void ExportDSM_SVG_Powershell_Success()
        {
            var graph = TestData.SimpleTestGraph2;
            var f = System.IO.Path.GetTempPath();
            var p = System.IO.Path.Combine(f, "export-test-DSM-SVG.svg");

            try
            {
                _powershell.AddCommand("New-DSM");
                _powershell.AddParameters(new Dictionary<String, Object> {
                    { "Graph", graph }
                });
                var result = _powershell.Invoke();
                _powershell.Commands.Clear();

                _powershell.AddCommand("Export-DSM");
                _powershell.AddParameters(new Dictionary<String, Object>
                    {
                        {"Dsm", result[0].ImmediateBaseObject}, { "Format", DSMExportTypes.SVG }, { "Path", p }
                    });
                result = _powershell.Invoke();

                var info = new System.IO.FileInfo(p);
                Assert.IsTrue(info != null);
                Assert.IsTrue(info.Exists);
                Assert.IsTrue(info.Length > 0);

            }
            finally
            {
                _powershell.Commands.Clear();
            }
        }

    }
}
