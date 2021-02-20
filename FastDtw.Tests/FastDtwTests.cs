
using MathNet.Numerics.LinearAlgebra.Double;
using Newtonsoft.Json;
using System.IO;
using Xunit;

namespace NFastdtw.Tests
{
    public class FastDtwTests
    {
        
        [Fact]
        public void ShouldCorrectlyCalculateDtw()
        {
            var data = JsonConvert.DeserializeObject<TestData>(File.ReadAllText("test.json"));
            var res2 = NFastDtw.FastDtw.Distance(data.x, data.y, 5, (x, y) => (new DenseVector(x) - new DenseVector(y)).L2Norm());
            Assert.Equal(1.0853674243064257, res2);
        }
        private class TestData
        {
            public double[][] x { get; set; }
            public double[][] y { get; set; }
        }

        //public class Point
        //{
        //    public double Latitude { get; set; }
        //    public double Longitude { get; set; }
        //}

        //static String BytesToString(long byteCount)
        //{
        //    string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
        //    if (byteCount == 0)
        //        return "0" + suf[0];
        //    long bytes = Math.Abs(byteCount);
        //    int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        //    double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        //    return (Math.Sign(byteCount) * num).ToString() + suf[place];
        //}

        //private static (T, string) CheckTimeAndMem<T>(string name, Func<T> f) {
        //    GC.Collect();
        //    var mem = GC.GetTotalMemory(false);
        //    var watch = System.Diagnostics.Stopwatch.StartNew();
        //    var result = f();
        //    watch.Stop();
        //    var elapsedS = watch.ElapsedMilliseconds/1000;
        //    var memRep = $"{name}\tMemory={BytesToString(GC.GetTotalMemory(false) - mem)} \tTime s={elapsedS}";
        //    return (result, memRep);
        //}

        //[Fact]
        //public void ShouldCorrectlyCalculateDtwForBigArray()
        //{
        //    using (var reader = new StreamReader("test2.csv"))
        //    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        //    {
        //        var records = csv.GetRecords<Point>().ToArray();
        //        var x = records.Select(x => new double[] { x.Latitude, x.Longitude }).ToArray();
        //        var y = x.Select(a => a.Select(b => b).ToArray()).Take(2000).ToArray();

        //        var r1 = CheckTimeAndMem("dtw", () => new Fastdtw(x, y, (x, y) => (new DenseVector(x) - new DenseVector(y)).L2Norm()).GetCost());
        //        var r2 = CheckTimeAndMem("fastdtw", () => FastDtw.FastDtw.Distance(x, y, 5, (x, y) => (new DenseVector(x) - new DenseVector(y)).L2Norm()));

        //    }
           
        //}
    }


    
}
