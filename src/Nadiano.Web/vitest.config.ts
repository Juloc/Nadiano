import { defineConfig } from "vitest/config";

export default defineConfig({
  test: {
    environment: "node",
    include: ["wwwroot/js/**/*.test.ts"],
  },
});
