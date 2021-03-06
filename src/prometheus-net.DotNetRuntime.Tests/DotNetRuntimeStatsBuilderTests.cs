using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Prometheus.Advanced;
using Prometheus.DotNetRuntime.StatsCollectors;

namespace Prometheus.DotNetRuntime.Tests
{
    [TestFixture]
    public class DotNetRuntimeStatsBuilderTests
    {
        /// <summary>
        /// Verifies that the default stats collectors can be registered with prometheus and that their metrics
        /// are being outputted to the metric server. 
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Default_registers_all_expected_stats()
        {
            // arrange
            DefaultCollectorRegistry.Instance.RegisterOnDemandCollectors(DotNetRuntimeStatsBuilder.Default());

            using (var metricServer = new MetricServer(12203))
            using (var client = new HttpClient())
            {
                metricServer.Start();

                // act + assert
                using (var resp = await client.GetAsync("http://localhost:12203/metrics"))
                {
                    var content = await resp.Content.ReadAsStringAsync();
                    
                    // Some basic assertions to check that the output of our stats collectors is present
                    Assert.That(content, Contains.Substring("dotnet_threadpool"));
                    Assert.That(content, Contains.Substring("dotnet_jit"));
                    Assert.That(content, Contains.Substring("dotnet_gc"));
                    Assert.That(content, Contains.Substring("dotnet_contention"));
                }
            }
        }

        [Test]
        public void WithCustomCollector_will_not_register_the_same_collector_twice()
        {
            var builder = DotNetRuntimeStatsBuilder
                .Customize()
                .WithGcStats()
                .WithCustomCollector(new GcStatsCollector());

            Assert.That(builder.StatsCollectors.Count, Is.EqualTo(1));
        }
    }
}