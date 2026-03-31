import { test } from '@playwright/test';
import path from 'path';
import http from 'http';
import fs from 'fs';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const frameworks = [
  { file: '01-tailwind.html', name: '01-tailwind' },
  { file: '02-bootstrap.html', name: '02-bootstrap' },
  { file: '03-bulma.html', name: '03-bulma' },
  { file: '04-daisyui.html', name: '04-daisyui' },
  { file: '05-pico.html', name: '05-pico' },
  { file: '06-nes-css.html', name: '06-nes-css' },
  { file: '07-water-css.html', name: '07-water-css' },
  { file: '08-open-props.html', name: '08-open-props' },
  { file: '09-shoelace.html', name: '09-shoelace' },
  { file: '10-sakura.html', name: '10-sakura' },
  { file: '11-primeflex.html', name: '11-primeflex' },
  { file: '12-fantasy-custom.html', name: '12-fantasy-custom' },
];

const screenshotDir = path.resolve(__dirname, '..', '..', 'docs', 'css-comparison', 'screenshots');
const htmlDir = path.resolve(__dirname, '..', '..', 'docs', 'css-comparison');

let server: http.Server;
let serverPort: number;

test.beforeAll(async () => {
  server = http.createServer((req, res) => {
    const basename = path.basename(req.url ?? '/');
    const safeName = basename.endsWith('.html') ? basename : 'index.html';
    const filePath = path.join(htmlDir, safeName);
    const resolved = path.resolve(filePath);
    if (!resolved.startsWith(htmlDir)) {
      res.writeHead(403);
      res.end('Forbidden');
      return;
    }
    const content = fs.existsSync(resolved) ? fs.readFileSync(resolved) : null;
    if (content) {
      res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
      res.end(content);
    } else {
      res.writeHead(404);
      res.end('Not found');
    }
  });
  await new Promise<void>((resolve) => {
    server.listen(0, () => {
      serverPort = (server.address() as { port: number }).port;
      resolve();
    });
  });
});

test.afterAll(async () => {
  await new Promise<void>((resolve) => server.close(() => resolve()));
});

for (const fw of frameworks) {
  test(`screenshot – ${fw.name}`, async ({ page }) => {
    await page.goto(`http://localhost:${serverPort}/${fw.file}`);

    // Wait for external CSS/fonts/JS to load
    await page.waitForLoadState('networkidle');
    // Extra delay for web font rendering and JS execution
    await page.waitForTimeout(2000);

    await page.screenshot({
      path: path.join(screenshotDir, `${fw.name}.png`),
      fullPage: true,
    });
  });
}
