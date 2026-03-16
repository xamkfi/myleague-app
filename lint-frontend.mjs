import { execFileSync } from 'child_process';
import { resolve, dirname } from 'path';
import { fileURLToPath } from 'url';

const rootDir = dirname(fileURLToPath(import.meta.url));
const frontendDir = resolve(rootDir, 'src', 'frontend');
const files = process.argv.slice(2);

try {
  execFileSync('npx', ['eslint', '--no-warn-ignored', '--max-warnings', '0', ...files], {
    cwd: frontendDir,
    stdio: 'inherit',
    shell: true,
  });
} catch {
  process.exit(1);
}
