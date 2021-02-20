Fork of [fastdtw](https://bitbucket.org/0x6a62/fastdtw/src/dev/) to support multivariate series and custom distance functions

    using MathNet.Numerics.LinearAlgebra.Double;
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
                var res2 = NFastDtw.FastDtw.Distance(data.x, data.y, 5, (x, y) => (new DenseVector(x) - new DenseVector(y)).L2Norm());
                Assert.Equal(1.0853674243064257, result);
            }
            private class TestData
            {
                public double[][] x { get; set; }
                public double[][] y { get; set; }
            }

        }
        
    }

License
====
Released under the MIT license: www.opensource.org/licenses/MIT
