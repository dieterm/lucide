using System.Drawing;
using System.Resources;
using System.Text.Json;

// Get the project directory (where the .csproj is located)
var projectDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? Environment.CurrentDirectory;

// When running via 'dotnet run', we need to find the actual project directory
// The .csproj is in the scripts folder, so we look for it
var csprojPath = Path.Combine(Environment.CurrentDirectory, "scripts", "GenerateResources.csproj");
if (File.Exists(csprojPath))
{
    // Running from package root (via pnpm build)
    projectDir = Path.Combine(Environment.CurrentDirectory, "scripts");
}
else if (File.Exists(Path.Combine(Environment.CurrentDirectory, "GenerateResources.csproj")))
{
    // Running directly from scripts folder
    projectDir = Environment.CurrentDirectory;
}

var packageDir = Path.GetFullPath(Path.Combine(projectDir, ".."));
var sourceIconFolder = Path.GetFullPath(Path.Combine(packageDir, "..", "lucide-ico", "dist"));
var iconsMetadataFolder = Path.GetFullPath(Path.Combine(packageDir, "..", "..", "icons"));
var categoriesFolder = Path.GetFullPath(Path.Combine(packageDir, "..", "..", "categories"));
var targetFolder = Path.GetFullPath(Path.Combine(packageDir, "dist"));

Console.WriteLine("🎨 Building WinForms .resx Resource files...");
Console.WriteLine($"📁 ICO Source: {sourceIconFolder}");
Console.WriteLine($"📁 Icons Metadata: {iconsMetadataFolder}");
Console.WriteLine($"📁 Categories: {categoriesFolder}");
Console.WriteLine($"📁 Output: {targetFolder}");
Console.WriteLine();

// Ensure source folder exists
if (!Directory.Exists(sourceIconFolder))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"❌ Error: ICO source folder not found: {sourceIconFolder}");
    Console.WriteLine("   Please run 'pnpm build' in the lucide-ico package first.");
    Console.ResetColor();
    Environment.Exit(1);
}

// Create output directory
Directory.CreateDirectory(targetFolder);

// Get all available ICO files
var icoFiles = Directory.GetFiles(sourceIconFolder, "*.ico")
    .ToDictionary(f => Path.GetFileNameWithoutExtension(f), f => f, StringComparer.OrdinalIgnoreCase);

Console.WriteLine($"Found {icoFiles.Count} ICO files available.");
Console.WriteLine();

// Read icon metadata to get category mappings
var iconCategories = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

foreach (var jsonFile in Directory.GetFiles(iconsMetadataFolder, "*.json"))
{
    var iconName = Path.GetFileNameWithoutExtension(jsonFile);
    
    try
    {
        var jsonContent = File.ReadAllText(jsonFile);
        using var doc = JsonDocument.Parse(jsonContent);
        
        if (doc.RootElement.TryGetProperty("categories", out var categoriesElement))
        {
            var categories = new List<string>();
            foreach (var category in categoriesElement.EnumerateArray())
            {
                var categoryName = category.GetString();
                if (!string.IsNullOrEmpty(categoryName))
                {
                    categories.Add(categoryName);
                }
            }
            iconCategories[iconName] = categories;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ⚠ Warning: Could not parse {iconName}.json: {ex.Message}");
    }
}

Console.WriteLine($"Loaded category metadata for {iconCategories.Count} icons.");
Console.WriteLine();

// Get all category names from the categories folder
var categoryNames = Directory.GetFiles(categoriesFolder, "*.json")
    .Select(f => Path.GetFileNameWithoutExtension(f))
    .OrderBy(n => n)
    .ToList();

Console.WriteLine($"Found {categoryNames.Count} categories:");
foreach (var cat in categoryNames)
{
    Console.WriteLine($"  • {cat}");
}
Console.WriteLine();

// Build category to icons mapping
var categoryIcons = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
foreach (var category in categoryNames)
{
    categoryIcons[category] = new List<string>();
}

foreach (var (iconName, categories) in iconCategories)
{
    foreach (var category in categories)
    {
        if (categoryIcons.TryGetValue(category, out var icons))
        {
            icons.Add(iconName);
        }
    }
}

// Sort icons within each category
foreach (var icons in categoryIcons.Values)
{
    icons.Sort(StringComparer.OrdinalIgnoreCase);
}

// Generate .resx file for each category
Console.WriteLine("Generating category .resx files...");

foreach (var (category, icons) in categoryIcons.OrderBy(kv => kv.Key))
{
    var categoryFileName = ToPascalCase(category);
    var resxPath = Path.Combine(targetFolder, $"LucideIcons.{categoryFileName}.resx");
    
    var addedCount = 0;
    
    using (var writer = new ResXResourceWriter(resxPath))
    {
        foreach (var iconName in icons)
        {
            if (icoFiles.TryGetValue(iconName, out var icoPath))
            {
                try
                {
                    var resourceKey = ToResourceKey(iconName);
                    using var image = Image.FromFile(icoPath);
                    writer.AddResource(resourceKey, image);
                    addedCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ⚠ Warning: Could not add {iconName}: {ex.Message}");
                }
            }
        }
        
        writer.Generate();
    }
    
    Console.WriteLine($"  ✓ {categoryFileName}: {addedCount} icons");
}

Console.WriteLine();

// Generate main LucideIcons.resx with all icons
Console.WriteLine("Generating LucideIcons.resx with all icons...");

var allIconsResxPath = Path.Combine(targetFolder, "LucideIcons.resx");
var totalCount = 0;

using (var writer = new ResXResourceWriter(allIconsResxPath))
{
    foreach (var (iconName, icoPath) in icoFiles.OrderBy(kv => kv.Key))
    {
        try
        {
            var resourceKey = ToResourceKey(iconName);
            using var image = Image.FromFile(icoPath);
            writer.AddResource(resourceKey, image);
            totalCount++;
            
            // Show progress every 100 icons
            if (totalCount % 100 == 0)
            {
                Console.WriteLine($"  ✓ Added {totalCount}/{icoFiles.Count} icons...");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠ Warning: Could not add {iconName}: {ex.Message}");
        }
    }
    
    writer.Generate();
}

Console.WriteLine($"  ✓ LucideIcons.resx: {totalCount} icons");
Console.WriteLine();

// Generate index.json with metadata
var indexContent = new
{
    name = "lucide-winforms",
    version = "0.0.1",
    iconCount = totalCount,
    categoryCount = categoryNames.Count,
    format = "WinForms .resx Resource Files",
    files = new[] { "LucideIcons.resx" }
        .Concat(categoryNames.Select(c => $"LucideIcons.{ToPascalCase(c)}.resx"))
        .ToArray(),
    categories = categoryIcons
        .OrderBy(kv => kv.Key)
        .ToDictionary(kv => kv.Key, kv => kv.Value.Count)
};

var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(targetFolder, "index.json"), JsonSerializer.Serialize(indexContent, jsonOptions));

Console.WriteLine($"📝 Generated index.json");
Console.WriteLine();
Console.WriteLine($"✅ Successfully generated {categoryNames.Count + 1} .resx files with {totalCount} icons total");

// Helper function to convert kebab-case to PascalCase
static string ToPascalCase(string input)
{
    if (string.IsNullOrEmpty(input)) return input;
    
    var parts = input.Split('-', '_');
    return string.Concat(parts.Select(p => 
        string.IsNullOrEmpty(p) ? p : char.ToUpperInvariant(p[0]) + p.Substring(1).ToLowerInvariant()
    ));
}

// Helper function to convert icon name to valid C# resource key
static string ToResourceKey(string iconName)
{
    // Convert kebab-case to PascalCase for valid C# identifiers
    // e.g., "arrow-down" -> "ArrowDown"
    return ToPascalCase(iconName);
}
