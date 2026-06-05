using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptFormatter.Core.Formatting.Custom;
using System;
using System.Collections.Generic;
using System.IO;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace ScriptFormatter.Tests
{
    [TestClass]
    public class FormatterRegressionTests
    {
        [TestMethod]
        public void Run_All_Regression_Tests()
        {
            string regressionRoot =
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Regression");

            var formatter =
                new CustomSqlFormatterService();

            int totalTests = 0;
            int passedTests = 0;
            int failedTests = 0;

            var failures = new List<string>();

            foreach (string inputFile in Directory.GetFiles(
                regressionRoot,
                "*.Input.sql",
                SearchOption.AllDirectories))
            {
                totalTests++;

                string folder =
                    Path.GetDirectoryName(inputFile);

                string testName =
                    Path.GetFileNameWithoutExtension(inputFile)
                        .Replace(".Input", "");

                TestContext.WriteLine(
                    $"RUNNING : {testName}");

                try
                {
                    string expectedFile =
                        inputFile.Replace(
                            ".Input.sql",
                            ".Expected.sql");

                    if (!File.Exists(expectedFile))
                    {
                        throw new FileNotFoundException(
                            "Expected file missing",
                            expectedFile);
                    }

                    string input =
                        System.IO.File.ReadAllText(inputFile);

                    string expected =
                        File.ReadAllText(expectedFile);

                    string actual =
                        formatter.Format(input);

                    Assert.AreEqual(
                        Normalize(expected),
                        Normalize(actual));

                    passedTests++;

                    TestContext.WriteLine(
                        $"PASS    : {testName}");
                }
                catch (Exception ex)
                {
                    failedTests++;

                    failures.Add(
                        $"{testName} - {ex.Message}");

                    TestContext.WriteLine(
                        $"FAIL    : {testName}");
                }
            }

            TestContext.WriteLine("");
            TestContext.WriteLine("================================");
            TestContext.WriteLine($"Total  : {totalTests}");
            TestContext.WriteLine($"Passed : {passedTests}");
            TestContext.WriteLine($"Failed : {failedTests}");
            TestContext.WriteLine("================================");

            if (failures.Count > 0)
            {
                Assert.Fail(
                    "Regression failures:" +
                    Environment.NewLine +
                    string.Join(
                        Environment.NewLine,
                        failures));
            }
        }
        
        public TestContext TestContext { get; set; }

        private static string Normalize(string value)
        {
            return value
        .Replace("\r\n", "\n")
        .Replace("\r", "\n")
        .Trim();
        }
    }
}