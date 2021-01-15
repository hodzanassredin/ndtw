
using Newtonsoft.Json;
using System.IO;
using Xunit;

namespace Fastdtw.Tests
{
    public class FastDtwTests
    {
        
        [Fact]
        public void ShouldCorrectlyCalculateDtw()
        {
            var data = JsonConvert.DeserializeObject<TestData>(File.ReadAllText("test.json"));
            var result = new Fastdtw(data.x, data.y).GetCost();
            Assert.Equal(1.085367424306426, result);
        }
        private class TestData
        {
            public double[][] x { get; set; }
            public double[][] y { get; set; }
        }

    }


    
}
