using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSGraph.Vega.Spec;

namespace PSGraph.Vega.Extensions;

public static class VegaHelper
{
    public static JObject GetVegaTemplate(string templateName)
    {
        var currentDir = System.IO.Directory.GetCurrentDirectory();
        var templatePath = System.IO.Path.Combine(currentDir, "Assets", templateName);

        if (!System.IO.File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file '{templateName}' not found in Assets directory.");
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

    public static JObject InsertData(JObject template, JArray data)
    {
        var dataToken = template["data"];

        if (dataToken is JArray dataArray &&
            dataArray.Count > 0 &&
            dataArray[0] is JObject firstData &&
            firstData["values"] != null)
        {
            firstData["values"] = data;
        }
        else
        {
            throw new InvalidOperationException("The 'values' field is missing in the template.");
        }

        return template;
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