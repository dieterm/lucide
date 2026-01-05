# Lucide WinForms

Lucide icons as WinForms-compatible .resx Resource files for .NET Windows Forms applications.

## Features

- **Category-based resources**: Separate .resx files per icon category
- **Complete icon set**: `LucideIcons.resx` contains all icons
- **WinForms compatible**: Ready to use with `ResourceManager` or Visual Studio designer
- **PascalCase naming**: Icon names converted to valid C# identifiers (e.g., `arrow-down` → `ArrowDown`)

## Installation

```sh
npm install lucide-winforms
```

## Building from source

> **Prerequisite**: The `lucide-ico` package must be built first to generate ICO files.

```sh
# First, build the ICO package
cd ../lucide-ico
pnpm install
pnpm build

# Then build the WinForms resources
cd ../lucide-winforms
pnpm install
pnpm build
```

## File structure

```
dist/
├── index.json                      # Metadata with icon counts per category
├── LucideIcons.resx               # All icons (complete set)
├── LucideIcons.Accessibility.resx # Icons in 'accessibility' category
├── LucideIcons.Account.resx       # Icons in 'account' category
├── LucideIcons.Arrows.resx        # Icons in 'arrows' category
├── ...                            # Other category files
```

## Usage in .NET Applications

### Using ResourceManager directly

```csharp
using System.Drawing;
using System.Resources;

// Load all icons
var resourceManager = new ResourceManager(
    "YourNamespace.LucideIcons",
    typeof(YourClass).Assembly
);

// Get an icon by name (PascalCase)
var icon = (Image)resourceManager.GetObject("ArrowDown");
pictureBox1.Image = icon;
```

### Using category-specific resources

```csharp
// Load only navigation icons
var navResources = new ResourceManager(
    "YourNamespace.LucideIcons.Navigation",
    typeof(YourClass).Assembly
);

var homeIcon = (Image)navResources.GetObject("Home");
```

### Adding to Visual Studio project

1. Copy the desired `.resx` files to your project
2. Set the file's **Build Action** to `Embedded Resource`
3. Use the Visual Studio designer or `ResourceManager` to access icons

### Designer support

After adding the `.resx` file to your project:

```csharp
// Access via generated strongly-typed class
var icon = LucideIcons.ArrowDown;
```

## Available Categories

| Category       | Description                     |
| -------------- | ------------------------------- |
| Accessibility  | Accessibility-related icons     |
| Account        | User and account icons          |
| Animals        | Animal icons                    |
| Arrows         | Arrow and direction icons       |
| Brands         | Brand and logo icons            |
| Buildings      | Building and architecture icons |
| Charts         | Chart and graph icons           |
| Communication  | Communication icons             |
| Connectivity   | Network and connectivity icons  |
| Cursors        | Cursor and pointer icons        |
| Design         | Design tool icons               |
| Development    | Development and code icons      |
| Devices        | Device and hardware icons       |
| Emoji          | Emoji and expression icons      |
| Files          | File and folder icons           |
| Finance        | Finance and money icons         |
| FoodBeverage   | Food and drink icons            |
| Gaming         | Gaming icons                    |
| Home           | Home and household icons        |
| Layout         | Layout and grid icons           |
| Mail           | Email icons                     |
| Math           | Mathematical icons              |
| Medical        | Medical and health icons        |
| Multimedia     | Audio and video icons           |
| Nature         | Nature and environment icons    |
| Navigation     | Navigation icons                |
| Notifications  | Alert and notification icons    |
| People         | People and user icons           |
| Photography    | Camera and photo icons          |
| Science        | Science icons                   |
| Seasons        | Season and calendar icons       |
| Security       | Security and lock icons         |
| Shapes         | Shape icons                     |
| Shopping       | Shopping and e-commerce icons   |
| Social         | Social media icons              |
| Sports         | Sports and fitness icons        |
| Sustainability | Sustainability icons            |
| Text           | Text formatting icons           |
| Time           | Time and clock icons            |
| Tools          | Tool icons                      |
| Transportation | Vehicle and transport icons     |
| Travel         | Travel icons                    |
| Weather        | Weather icons                   |

## Requirements

- .NET 8.0 SDK (for building)
- `lucide-ico` package (must be built first)

## License

ISC License - see [LICENSE](./LICENSE) for details.
