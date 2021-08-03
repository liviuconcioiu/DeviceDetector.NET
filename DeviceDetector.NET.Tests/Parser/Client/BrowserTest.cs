﻿using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using DeviceDetectorNET.Parser.Client;
using DeviceDetectorNET.Tests.Class.Client;
using DeviceDetectorNET.Yaml;
using System.Linq;
using System.Threading.Tasks;
using DeviceDetectorNET.Results.Client;

namespace DeviceDetectorNET.Tests.Parser.Client
{
    [Trait("Category", "Browser")]
    public class BrowserTest
    {
        private readonly List<BrowserFixture> _fixtureData;

        public BrowserTest()
        {
            var path = $"{Utils.CurrentDirectory()}\\{@"Parser\Client\fixtures\browser.yml"}";

            var parser = new YamlParser<List<BrowserFixture>>();
            _fixtureData = parser.ParseFile(path);

            //replace null
            _fixtureData = _fixtureData.Select(f =>
            {
                f.client.version ??= string.Empty;
                f.client.engine ??= string.Empty;
                f.client.engine_version ??= string.Empty;
                return f;
            }).ToList();
        }

        [Fact]
        public void TestGetAvailableBrowserFamilies()
        {
            BrowserParser.GetAvailableBrowserFamilies().Count.Should().BeGreaterThan(5);
        }


        [Fact]
        public void TestAllBrowsersTested()
        {
            BrowserParser.SetVersionTruncation(BrowserParser.VERSION_TRUNCATION_NONE);

            Parallel.ForEach(_fixtureData, fixture =>
            {
                var browsers = new BrowserParser();
                browsers.SetUserAgent(fixture.user_agent);
                var result = browsers.Parse();

                result.Success.Should().BeTrue("Match should be with success");
                var browserResult = result.Match as BrowserMatchResult;

                browserResult.Should().NotBeNull("Match should be of type BrowserMatchResult");

                if (browserResult == null) return;

                browserResult.Engine.Should()
                             .BeEquivalentTo(fixture.client.engine, "Engine should be equal " + fixture.user_agent);
                browserResult.EngineVersion.Should().BeEquivalentTo(fixture.client.engine_version.ToString(),
                    "EngineVersion should be equal");
                browserResult.Name.Should().BeEquivalentTo(fixture.client.name, "Names should be equal");
                browserResult.Type.Should().BeEquivalentTo(fixture.client.type, "Type should be equal");
                browserResult.Version.Should().BeEquivalentTo(fixture.client.version, "Version should be equal");
            });
        }

        [Fact]
        public void TestGetAvailableClients()
        {
            var available = new BrowserParser().GetAvailableClients();
            BrowserParser.GetAvailableBrowsers().Count.Should().BeGreaterOrEqualTo(available.Count);
        }

    }
}
