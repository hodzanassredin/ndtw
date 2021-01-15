Fork of [NDtw](https://nuget.org/packages/NDtw) to match python's fastdtw api and support MathNet Numerics distance functions

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
                var result = new Fastdtw(
                    data.x, 
                    data.y, 
                    (x, y) => (new DenseVector(x) - new DenseVector(y)).L2Norm()).GetCost();
                Assert.Equal(1.085367424306426, result);
            }
            private class TestData
            {
                public double[][] x { get; set; }
                public double[][] y { get; set; }
            }

        }
        
    }


It is the same as this NDtw code:

    private static double[] SelectDim(double[][] arr, int dim) {
            return arr.Select(x => x[dim]).ToArray();
    }
    var series = new[] {
        new SeriesVariable(SelectDim(x, 0), SelectDim(y, 0)),
        new SeriesVariable(SelectDim(x, 1), SelectDim(y, 1)),
    };
    return new Dtw(series, DistanceMeasure.Euclidean).GetCost();

License
====
NDtw is released under the MIT license: www.opensource.org/licenses/MIT
