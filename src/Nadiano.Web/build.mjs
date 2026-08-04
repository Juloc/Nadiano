import { build, context } from "esbuild";
import { readdirSync } from "node:fs";
import { fileURLToPath } from "node:url";
import path from "node:path";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const entryDir = path.join(__dirname, "wwwroot/js/pages");
const outDir = path.join(__dirname, "wwwroot/dist/pages");

const entryPoints = readdirSync(entryDir)
  .filter((file) => file.endsWith(".ts"))
  .map((file) => path.join(entryDir, file));

const watch = process.argv.includes("--watch");

const options = {
  entryPoints,
  outdir: outDir,
  bundle: true,
  format: "esm",
  target: "es2022",
  sourcemap: true,
  logLevel: "info",
};

if (watch) {
  const ctx = await context(options);
  await ctx.watch();
  console.log("esbuild: watching wwwroot/js/pages for changes...");
} else {
  await build(options);
}
