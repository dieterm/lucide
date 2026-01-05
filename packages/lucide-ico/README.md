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
