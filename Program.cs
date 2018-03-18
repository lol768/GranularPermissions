using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using GranularPermissions.Tests;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace GranularPermissions
{
    
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SystemPerformanceTests>(new AllowNonOptimized());
        }
    }
    
    class AllowNonOptimized : ManualConfig
    {
        public AllowNonOptimized()
        {
            Add(JitOptimizationsValidator.DontFailOnError); // ALLOW NON-OPTIMIZED DLLS
            Add(CsvMeasurementsExporter.Default);
            Add(RPlotExporter.Default);
            Add(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
            Add(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default
            Add(DefaultConfig.Instance.GetColumnProviders().ToArray()); // manual config has no columns by default
        }
    }
}