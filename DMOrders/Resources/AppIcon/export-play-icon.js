const fs = require('fs');
const path = require('path');

async function main() {
  const { Resvg } = require('@resvg/resvg-js');
  const dir = __dirname;
  const svgPath = path.join(dir, 'play-store-icon-512.svg');
  const outPath = path.join(dir, 'play-store-icon-512.png');
  const svg = fs.readFileSync(svgPath, 'utf8');
  const resvg = new Resvg(svg, {
    fitTo: { mode: 'width', value: 512 },
    font: { loadSystemFonts: true }
  });
  const pngData = resvg.render();
  fs.writeFileSync(outPath, pngData.asPng());
  console.log('Created:', outPath);
}

main().catch(err => {
  console.error(err);
  process.exit(1);
});
