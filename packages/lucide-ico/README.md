# Lucide ICO

Lucide icons in ICO format for Windows applications and favicons.

## Features

- **Multiple sizes**: 16×16, 24×24, 32×32, 48×48, 64×64, 128×128, 256×256
- **32-bit color depth** with full transparency support
- **PNG compressed** for optimal file size

## Installation

```sh
npm install lucide-ico
```

## Usage

The ICO files are located in the `dist` folder after building.

### Building from source

```sh
pnpm install
pnpm build
```

### Building with a custom color

By default, icons use `currentColor` (default is black: `#000000`). You can specify a custom color using a hex color code:

```sh
# White icons
pnpm build "#ffffff"

# Red icons
pnpm build "#ff0000"

# Short hex format also works
pnpm build "#fff"
```

### Building with custom sizes

By default, icons are generated in sizes: 16, 24, 32, 48, 64, 128, 256px. You can specify custom sizes:

```sh
# Only small sizes
pnpm build "#000000" "16, 24, 32"

# All default sizes explicitly
pnpm build "#ffffff" "16, 24, 32, 48, 64, 128, 256"

# Without spaces also works
pnpm build "#fff" "16,24,32,48"
```

Note: the `pnpm build` command will delete all files and subfolders inside the `lucide-ico\dist` folder.
To prevent this, execute ``pnpm build:icons` instead (this will not perform `pnpm clean`).

```sh
cd <path_to>\lucide\packages\lucide-ico;

pnpm build:icons "#000000"

pnpm build:icons "#000000" "16, 32"
```

### File structure

```
dist/
├── index.json          # Metadata with all icon names
├── activity.ico        # Individual ICO files
├── airplay.ico
├── ...
```

## Using in your project

### As favicon

```html
<link
  rel="icon"
  type="image/x-icon"
  href="path/to/icon.ico"
/>
```

### In Windows applications

ICO files can be used directly as application icons in Windows desktop applications.

## License

ISC License - see [LICENSE](./LICENSE) for details.
