namespace MeasureThat.Net.Logic
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Http;
    using Models;

    public class InputDataParser
    {
        private static readonly Regex TestCaseKeyRegex = new Regex("TestCases\\[(\\d+)\\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // TODO: Unit tests
        public static List<TestCaseDto> ReadTestCases([NotNull] HttpRequest request)
        {
            var testCases = new List<TestCaseDto>();
            if (!request.HasFormContentType)
            {
                return testCases;
            }

            var indexes = new HashSet<int>(); // list of test case indexes
            IFormCollection form = request.Form;
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("TestCases["))
                {
                    var match = TestCaseKeyRegex.Match(key);
                    if (!match.Success || match.Groups.Count != 2)
                    {
                        continue;
                    }

                    int index = 0;
                    if (int.TryParse(match.Groups[1].Value, out index))
                    {
                        indexes.Add(index);
                    }
                }
            }

            foreach (var idx in indexes)
            {
                string nameKey = $"TestCases[{idx}].TestCaseName";
                string codeKey = $"TestCases[{idx}].BenchmarkCode";

                if (form.ContainsKey(nameKey) && form.ContainsKey(codeKey))
                {
                    var name = form[nameKey];
                    var code = form[codeKey];
                    var testCase = new TestCaseDto()
                    {
                        BenchmarkCode = code,
                        TestCaseName = name
                    };
                    testCases.Add(testCase);
                }
            }

            return testCases;
        }
    }
}
