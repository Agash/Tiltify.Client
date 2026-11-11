// Types Tiltify's paginated list responses before Kiota generates the client.
//
// Tiltify's OpenAPI document declares every list endpoint's 200 response as allOf: [PaginatedResponse], and
// PaginatedResponse.data as an array of bare objects. The item shape appears only in the response example, so no
// generator can type it. This matches each response's example item against the component schemas (the schema with
// the fewest properties that contains every field of the example) and gives the response a typed data array.
//
// Usage: dotnet run scripts/patch-openapi.cs -- <input.json> <output.json>

using System.Text.Json;
using System.Text.Json.Nodes;

if (args.Length != 2)
{
    Console.Error.WriteLine(
        "Usage: dotnet run scripts/patch-openapi.cs -- <input.json> <output.json>"
    );
    return 1;
}

JsonNode root =
    JsonNode.Parse(File.ReadAllText(args[0])) ?? throw new InvalidOperationException("Empty spec.");
JsonObject schemas = root["components"]!["schemas"]!.AsObject();
Dictionary<string, HashSet<string>> properties = schemas
    .Where(static s => s.Value?["properties"] is JsonObject)
    .ToDictionary(
        static s => s.Key,
        static s =>
            s.Value!["properties"]!
                .AsObject()
                .Select(static p => p.Key)
                .ToHashSet(StringComparer.Ordinal)
    );

// The page cursor block, shared by every list response.
schemas["PaginationMetadata"] = schemas["PaginatedResponse"]!["properties"]![
    "metadata"
]!.DeepClone();

int typed = 0;
List<string> untyped = [];
foreach ((string path, JsonNode? operations) in root["paths"]!.AsObject())
{
    if (
        operations?["get"]?["responses"]?["200"]?["content"]?["application/json"]?["schema"]
            is not JsonObject schema
        || schema["allOf"] is not JsonArray allOf
        || !allOf.Any(static part =>
            part?["$ref"]?.GetValue<string>()
                .EndsWith("/PaginatedResponse", StringComparison.Ordinal) == true
        )
    )
    {
        continue;
    }

    if (
        schema["example"]?["data"] is not JsonArray { Count: > 0 } items
        || items[0] is not JsonObject first
    )
    {
        untyped.Add($"{path} (no example item)");
        continue;
    }

    HashSet<string> fields = [.. first.Select(static f => f.Key)];
    string? match = properties
        .Where(candidate => fields.IsSubsetOf(candidate.Value))
        .OrderBy(static candidate => candidate.Value.Count)
        .ThenBy(static candidate => candidate.Key, StringComparer.Ordinal)
        .Select(static candidate => candidate.Key)
        .FirstOrDefault();
    if (match is null)
    {
        untyped.Add(
            $"{path} (no schema has fields {string.Join(", ", fields.Order(StringComparer.Ordinal))})"
        );
        continue;
    }

    // Replace the allOf rather than extending it: Kiota models allOf as inheritance and keeps the base's untyped data.
    JsonNode? example = schema["example"]?.DeepClone();
    JsonObject replacement = new()
    {
        ["type"] = "object",
        ["required"] = new JsonArray("data"),
        ["properties"] = new JsonObject
        {
            ["data"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject { ["$ref"] = $"#/components/schemas/{match}" },
            },
            ["metadata"] = new JsonObject { ["$ref"] = "#/components/schemas/PaginationMetadata" },
        },
    };
    if (example is not null)
    {
        replacement["example"] = example;
    }

    operations["get"]!["responses"]!["200"]!["content"]!["application/json"]!["schema"] =
        replacement;
    typed++;
    Console.WriteLine($"{path} -> {match}");
}

File.WriteAllText(args[1], root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Typed {typed} list responses.");
foreach (string path in untyped)
{
    Console.WriteLine($"Left untyped: {path}");
}

return 0;
