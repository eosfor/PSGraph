using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSGraph.Vega.Spec;

namespace PSGraph.Vega.Extensions;

public static class VegaHelper
{
    public static JObject GetVegaTemplate(string templateName)
    {
        var assemblyPath = System.IO.Path.GetDirectoryName(typeof(VegaHelper).Assembly.Location);
        var templatePath = System.IO.Path.Combine(assemblyPath ?? ".", "Assets", templateName);

        if (!System.IO.File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file '{templateName}' not found in Assets directory at '{templatePath}'.");
        }

        return JObject.Parse(System.IO.File.ReadAllText(templatePath));
    }

    public static Spec.Vega GetVegaTemplateObject(string templateName)
    {
        var currentDir = System.IO.Directory.GetCurrentDirectory();
        var templatePath = System.IO.Path.Combine(currentDir, "Assets", templateName);

        if (!System.IO.File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file '{templateName}' not found in Assets directory.");
        }

        return Vega.Spec.Vega.FromJson(
            System.IO.File.ReadAllText(templatePath))
            ?? throw new InvalidOperationException($"Failed to deserialize Vega template from '{templateName}'.");

        // return JsonConvert.DeserializeObject<Spec.Vega>(System.IO.File.ReadAllText(templatePath), Converter.Settings)
        //     ?? throw new InvalidOperationException($"Failed to deserialize Vega template from '{templatePath}'.");
    }

    public static Spec.Vega GetVegaTemplateObjectFromModulePath(string modulePath, string templateName)
    {
        var templatePath = System.IO.Path.Combine(modulePath, "Assets", templateName);

        if (!System.IO.File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file '{templateName}' not found in Assets directory.");
        }

        return Vega.Spec.Vega.FromJson(
            System.IO.File.ReadAllText(templatePath))
            ?? throw new InvalidOperationException($"Failed to deserialize Vega template from '{templateName}'.");

        // return JsonConvert.DeserializeObject<Spec.Vega>(System.IO.File.ReadAllText(templatePath), Converter.Settings)
        //     ?? throw new InvalidOperationException($"Failed to deserialize Vega template from '{templatePath}'.");
    }

    public static void SaveVegaTemplate(Spec.Vega vegaSpec, string templateName)
    {
        var currentDir = System.IO.Directory.GetCurrentDirectory();
        var templatePath = System.IO.Path.Combine(currentDir, "Assets", templateName);

        var json = JsonConvert.SerializeObject(vegaSpec, Converter.Settings);
        System.IO.File.WriteAllText(templatePath, json);
    }

    public static string InsertData(JObject data, string[] properties, VegaExportTypes exportType, string modulePath, string vegaSpecFileName)
    {
        var vega = VegaHelper.GetVegaTemplateObjectFromModulePath(modulePath, vegaSpecFileName); //"vega.dsm.matrix.json"

        // assuming these indices are correct for the matrix template
        foreach (string property in properties)
        {
            vega.Data.Single(d => d.Name == property).Values =
                data[property]["values"].ToObject<List<object>>();
        }

        return exportType switch
        {
            VegaExportTypes.HTML => VegaHelper.RenderHtml(vega),
            _ => vega.ToJson()
        };
    }

    public static string RenderHtml(Vega.Spec.Vega vegaSpec)
    {
        return $"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8">
            <title>Vega Visualization</title>
            <script src="https://cdn.jsdelivr.net/npm/vega@6"></script>
            <script src="https://cdn.jsdelivr.net/npm/vega-lite@6"></script>
            <script src="https://cdn.jsdelivr.net/npm/vega-embed@7"></script>
        </head>
        <body>
            <div id="vis"></div>
            <script type="text/javascript">
                const spec = {vegaSpec.ToJson()};
                vegaEmbed("#vis", spec).catch(console.error);
            </script>
        </body>
        </html>
        """;
    }
}