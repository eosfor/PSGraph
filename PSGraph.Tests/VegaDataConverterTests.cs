using PSGraph.Vega.Extensions;
using FluentAssertions;
using PSGraph.Vega.Spec;

namespace PSGraph.Tests
{
    public class VegaDataConverterTests
    {

        [Fact]
        public void ShouldEmbedNodeLinkIntoVegaForceLayoutTemplate()
        {
            var graph = GraphTestData.DSMFull;
            var records = graph.ConvertToVegaNodeLink();
            var vega = VegaHelper.GetVegaTemplateObject("vega.force.directed.layout.json");

            vega.Data[0].Values = records.nodes.ToList<object>();
            vega.Data[1].Values = records.links.ToList<object>();

            string json = vega.ToJson();
            File.WriteAllText("x.force.directed.layout.vega.json", json);
        }

        [Fact]
        public void ShouldEmbedNodeLinkIntoVegaAdjMatrixTemplate()
        {
            var graph = GraphTestData.DSMFull;
            var records = graph.ConvertToVegaNodeLink();
            var vega = VegaHelper.GetVegaTemplateObject("vega.adj.matrix.json");

            vega.Data[0].Values = records.nodes.ToList<object>();
            vega.Data[1].Values = records.links.ToList<object>();

            string json = vega.ToJson();
            File.WriteAllText("x.adj.matrix.vega.json", json);
        }

        [Fact]
        public void ShouldEmbedParentChildIntoTreeLayoutTemplate()
        {
            var graph = GraphTestData.DSMFull;
            var records = graph.ConvertToParentChildList();
            var vega = VegaHelper.GetVegaTemplateObject("vega.tree.layout.json");

            vega.Data[0].Values = records.ToList<object>();

            string json = vega.ToJson();
            File.WriteAllText("x.tree.layout.vega.json", json);
        }

        [Fact]
        public void ShouldRenderAdjMatrixHtmlWithVegaSpec()
        {
            var graph = GraphTestData.DSMFull;
            var records = graph.ConvertToVegaNodeLink();
            var vega = VegaHelper.GetVegaTemplateObject("vega.adj.matrix.json");
            vega.Data[0].Values = records.nodes.ToList<object>();
            vega.Data[1].Values = records.links.ToList<object>();
            string html = VegaHelper.RenderHtml(vega);

            html.Should().Contain("<html>");
            html.Should().Contain("vegaEmbed");
            html.Should().Contain("\"values\"");

            File.WriteAllText("x.vega.adj.matrix.html", html);
        }

        [Fact]
        public void ShouldRenderForceDirectedHtmlWithVegaSpec()
        {
            // Arrange: создаем простой граф и данные
            var graph = GraphTestData.DSMFull;
            var records = graph.ConvertToVegaNodeLink();
            var vega = VegaHelper.GetVegaTemplateObject("vega.force.directed.layout.json");
            vega.Data[0].Values = records.nodes.ToList<object>();
            vega.Data[1].Values = records.links.ToList<object>();
            string html = VegaHelper.RenderHtml(vega);

            html.Should().Contain("<html>");
            html.Should().Contain("vegaEmbed");
            html.Should().Contain("\"values\"");

            // Записываем для ручного просмотра, если нужно
            File.WriteAllText("x.vega.force.directed.layout.html", html);
        }

        [Fact]
        public void ShouldRenderTreelayoutHtmlWithVegaSpec()
        {
            var graph = GraphTestData.DSMFull;
            var records = graph.ConvertToParentChildList();
            var vega = VegaHelper.GetVegaTemplateObject("vega.tree.layout.json");
            vega.Data[0].Values = records.ToList<object>();

            string html = VegaHelper.RenderHtml(vega);

            html.Should().Contain("<html>");
            html.Should().Contain("vegaEmbed");
            html.Should().Contain("\"values\"");

            // Записываем для ручного просмотра, если нужно
            File.WriteAllText("x.vega.tree.layout.html", html);
        }
    }

}