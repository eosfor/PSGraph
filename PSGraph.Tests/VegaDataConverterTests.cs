using PSGraph.Model.VegaDataModels;
using Newtonsoft.Json.Linq;
using PSGraph.Vega.Extensions;
using FluentAssertions;

namespace PSGraph.Tests
{
    public class VegaDataConverterTests
    {
        [Fact]
        public async Task ShouldEmbedGraphRecordsIntoVegaTemplate()
        {
            var graph = GraphTestData.SimpleTestGraph5;

            List<GraphRecord> records = graph.ConvertToParentChildList();

            var currentDir = System.IO.Directory.GetCurrentDirectory();
            var testTemplatePath = System.IO.Path.Combine(currentDir, "Assets", "vega.tree.layout.json");
            string template = File.ReadAllText(testTemplatePath);

            var vega = JObject.Parse(template);

            var dataToken = vega["data"];

            if (dataToken is JArray dataArray &&
                dataArray.Count > 0 &&
                dataArray[0] is JObject firstData &&
                firstData["values"] != null)
            {
                firstData["values"] = JArray.FromObject(records);
            }
            else
            {
                Console.WriteLine("Поле 'values' отсутствует.");
            }

            string json = vega.ToString(Newtonsoft.Json.Formatting.Indented);

            // Сохранение в файл
            File.WriteAllText("vega.json", json);
        }
    }
}