using MapTalkie.Utils;
using NetTopologySuite.Geometries;
using Xunit;
using Xunit.Abstractions;

namespace MaptalkieTests
{
    public class SandboxTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public SandboxTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Test()
        {
            for (var i = 0; i < 10; i += 1)
            {
                _testOutputHelper.WriteLine($"LEVEL {i}");
                _testOutputHelper.WriteLine("\t(180, 90) " + MapUtilities.GetAreaId(new Point(180, 90), i));
                _testOutputHelper.WriteLine("\t(180, -90) " + MapUtilities.GetAreaId(new Point(180, -90), i));
                _testOutputHelper.WriteLine("\t(-180, -90) " + MapUtilities.GetAreaId(new Point(-180, -90), i));
                _testOutputHelper.WriteLine("\t(-180, 90) " + MapUtilities.GetAreaId(new Point(-180, 90), i));
                _testOutputHelper.WriteLine("\t(0, 0) " + MapUtilities.GetAreaId(new Point(0, 0), i));
                _testOutputHelper.WriteLine("\t(90, 45) " + MapUtilities.GetAreaId(new Point(90, 45), i));
                _testOutputHelper.WriteLine("\t(110, 0) " + MapUtilities.GetAreaId(new Point(0, 0), i));
                _testOutputHelper.WriteLine("\t(-1, -1) " + MapUtilities.GetAreaId(new Point(-1, -1), i));
                _testOutputHelper.WriteLine("\t(1, 1) " + MapUtilities.GetAreaId(new Point(1, 1), i));
            }
        }
    }
}