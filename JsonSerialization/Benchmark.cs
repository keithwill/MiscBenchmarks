using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Test
{
    record MyType(string Name, int Age);

    [JsonSourceGenerationOptions(WriteIndented = false)]
    [JsonSerializable(typeof(MyType))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {
    }

    [MemoryDiagnoser]
    public class Benchmark
    {
        [Params(10, 1000)]
        public int Count { get; set; }

        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        private List<MyType> _data = new();

        [GlobalSetup]
        public void GlobalSetup()
        {
            for (int i = 0; i < Count; i++)
            {
                _data.Add(new MyType($"SomeName{i}", i));
            }
        }

        [Benchmark]
        public long SerializeAndDeserializeSTJSourceGen()
        {
            long total = 0;

            foreach (var item in _data)
            {
                total += SystemTextJsonSourceGen(item);
            }

            return total;
        }

        [Benchmark]
        public long SerializeAndDeserializeSTJ()
        {
            long total = 0;

            foreach (var item in _data)
            {
                total += SystemTextJson(item, options);
            }

            return total;
        }

        [Benchmark]
        public long SerializeAndDeserializeNewtonsoft()
        {
            long total = 0;
            
            foreach (var item in _data)
            {
                total += NewtonsoftJson(item);
            }

            return total;
        }

        static int SystemTextJsonSourceGen(MyType m)
        {
            var s = JsonSerializer.SerializeToUtf8Bytes(m, SourceGenerationContext.Default.MyType);
            var r = JsonSerializer.Deserialize(s, SourceGenerationContext.Default.MyType)!;
            return r.Age;
        }

        static int SystemTextJson(MyType m, JsonSerializerOptions options)
        {
            var s = JsonSerializer.SerializeToUtf8Bytes(m, options);
            var r = JsonSerializer.Deserialize<MyType>(s, options)!;
            return r.Age;
        }

        static int NewtonsoftJson(MyType m)
        {
            var s = JsonConvert.SerializeObject(m);
            var r = JsonConvert.DeserializeObject<MyType>(s)!;
            return r.Age;
        }
    }
}
