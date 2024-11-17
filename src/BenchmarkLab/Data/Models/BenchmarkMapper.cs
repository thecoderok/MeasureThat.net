using System;
using System.Collections.Generic;
using BenchmarkLab.Models;
using MeasureThat.Net.Models;

public static class BenchmarkMapper
{
    public static MeasureThatBenchmark ToMeasureThatBenchmark(BenchmarkDto benchmarkDto)
    {
        if (benchmarkDto == null)
        {
            throw new ArgumentNullException(nameof(benchmarkDto));
        }

        var testCases = new List<TestCase>();
        foreach (var testCaseDto in benchmarkDto.TestCases)
        {
            var testCase = new TestCase
            {
                Name = testCaseDto.TestCaseName,
                Code = testCaseDto.BenchmarkCode,
                IsDeferred = testCaseDto.Deferred
            };
            testCases.Add(testCase);
        }

        var measureThatBenchmark = new MeasureThatBenchmark
        {
            ScriptPreparationCode = benchmarkDto.ScriptPreparationCode,
            IsPython = benchmarkDto.IsPython,
            TestCases = testCases
        };

        return measureThatBenchmark;
    }
}
