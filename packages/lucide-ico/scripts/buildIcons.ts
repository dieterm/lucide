import fs from 'fs';
import path from 'path';
import { pathToFileURL } from 'url';
import sharp from 'sharp';
import pngToIco from 'png-to-ico';
import { readSvgDirectory } from '@lucide/helpers';

// Default ICO sizes to generate
const DEFAULT_ICO_SIZES = [16, 24, 32, 48, 64, 128, 256];

// Parse command line arguments for optional color
function parseColorArg(): string | null {
  const args = process.argv.slice(2);
  for (const arg of args) {
    // Match hex color patterns: #fff, #ffffff (must start with #)
    const colorMatch = arg.match(/^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$/);
    if (colorMatch) {
      const hex = colorMatch[1];
      // Normalize 3-digit hex to 6-digit
      if (hex.length === 3) {
        return `#${hex[0]}${hex[0]}${hex[1]}${hex[1]}${hex[2]}${hex[2]}`;
      }
      return `#${hex}`;
    }
  }
  return null;
}

// Parse command line arguments for optional sizes (e.g., "16, 24, 32" or "16,24,32")
function parseSizesArg(): number[] | null {
  const args = process.argv.slice(2);
  for (const arg of args) {
    // Match comma-separated numbers pattern (with optional spaces)
    const sizesMatch = arg.match(/^(\d+\s*,\s*)+(\d+)$/);
    if (sizesMatch || /^\d+$/.test(arg)) {
      // Parse the sizes
      const sizes = arg
        .split(',')
        .map((s) => parseInt(s.trim(), 10))
        .filter((n) => !isNaN(n) && n > 0 && n <= 256);
      if (sizes.length > 0) {
        return sizes.sort((a, b) => a - b);
      }
    }
  }
  return null;
}

// Replace stroke color in SVG content
function replaceStrokeColor(svgContent: string, newColor: string): string {
  // Lucide icons use stroke="currentColor" - replace it with the specified color
  return svgContent
    .replace(/stroke="currentColor"/g, `stroke="${newColor}"`)
    .replace(/fill="currentColor"/g, `fill="${newColor}"`);
}

const CUSTOM_COLOR = parseColorArg();
const CUSTOM_SIZES = parseSizesArg();
const ICO_SIZES = CUSTOM_SIZES || DEFAULT_ICO_SIZES;

const ICONS_DIR = path.resolve(process.cwd(), '../../icons');
const OUTPUT_DIR = path.resolve(process.cwd(), 'dist', CUSTOM_COLOR ?? '#000000');

interface IconMetadata {
  name: string;
  aliases?: string[];
}

async function getIconMetaData(iconDirectory: string): Promise<Record<string, IconMetadata>> {
  const iconJsons = await readSvgDirectory(iconDirectory, '.json');
  const entries = await Promise.all(
    iconJsons.map(async (jsonFile: string) => {
      const filePath = path.join(iconDirectory, jsonFile);
      const fileUrl = pathToFileURL(filePath).href;
      const file = await import(fileUrl, { with: { type: 'json' } });
      return [path.basename(jsonFile, '.json'), file.default];
    }),
  );
  return Object.fromEntries(entries);
}

async function svgToIco(svgPath: string, outputPath: string, color: string | null): Promise<void> {
  let svgContent = await fs.promises.readFile(svgPath, 'utf-8');

  // Apply custom color if specified
  if (color) {
    svgContent = replaceStrokeColor(svgContent, color);
  }

  // Generate PNG buffers for each size
  const pngBuffers: Buffer[] = await Promise.all(
    ICO_SIZES.map(async (size) => {
      const pngBuffer = await sharp(Buffer.from(svgContent))
        .resize(size, size, {
          fit: 'contain',
          background: { r: 0, g: 0, b: 0, alpha: 0 },
        })
        .png({
          compressionLevel: 9,
          palette: false, // Use 32-bit color depth
        })
        .toBuffer();

      return pngBuffer;
    }),
  );

  // Convert PNGs to ICO
  const icoBuffer = await pngToIco(pngBuffers);

  await fs.promises.writeFile(outputPath, icoBuffer);
}

async function buildIcons(): Promise<void> {
  console.log('🎨 Building ICO icons...');
  console.log(`📁 Source: ${ICONS_DIR}`);
  console.log(`📁 Output: ${OUTPUT_DIR}`);
  if (CUSTOM_SIZES) {
    console.log(`📐 Custom sizes: ${ICO_SIZES.join(', ')}px`);
  } else {
    console.log(`📐 Sizes: ${ICO_SIZES.join(', ')}px (default)`);
  }
  if (CUSTOM_COLOR) {
    console.log(`🎨 Custom color: ${CUSTOM_COLOR}`);
  } else {
    console.log(`🎨 Color: currentColor (default)`);
  }
  console.log('');

  // Create output directory
  if (!fs.existsSync(OUTPUT_DIR)) {
    fs.mkdirSync(OUTPUT_DIR, { recursive: true });
  }

  // Read all SVG files
  const svgFiles = await readSvgDirectory(ICONS_DIR, '.svg');
  const iconMetaData = await getIconMetaData(ICONS_DIR);

  console.log(`Found ${svgFiles.length} icons to convert...\n`);

  let successCount = 0;
  let errorCount = 0;

  for (const svgFile of svgFiles) {
    const iconName = path.basename(svgFile, '.svg');
    const svgPath = path.join(ICONS_DIR, svgFile);
    const icoPath = path.join(OUTPUT_DIR, `${iconName}.ico`);

    try {
      await svgToIco(svgPath, icoPath, CUSTOM_COLOR);
      successCount++;

      // Show progress every 100 icons
      if (successCount % 100 === 0) {
        console.log(`  ✓ Converted ${successCount}/${svgFiles.length} icons...`);
      }
    } catch (error) {
      errorCount++;
      console.error(`  ✗ Error converting ${iconName}:`, error);
    }
  }

  console.log('');
  console.log(`✅ Successfully converted ${successCount} icons to ICO format`);

  if (errorCount > 0) {
    console.log(`❌ Failed to convert ${errorCount} icons`);
  }

  // Generate index file with icon list
  const indexContent = {
    name: 'lucide-ico',
    version: '0.0.1',
    iconCount: successCount,
    sizes: ICO_SIZES,
    colorDepth: '32-bit with transparency',
    format: 'PNG compressed ICO',
    color: CUSTOM_COLOR || 'currentColor',
    icons: Object.keys(iconMetaData).sort(),
  };

  await fs.promises.writeFile(
    path.join(OUTPUT_DIR, 'index.json'),
    JSON.stringify(indexContent, null, 2),
  );

  console.log(`📝 Generated index.json with ${successCount} icon entries`);
}

// Run the build
buildIcons().catch((error) => {
  console.error('Build failed:', error);
  process.exit(1);
});
