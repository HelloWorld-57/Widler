import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { USERS_API_BASE, POSTS_API_BASE } from "./src/api/config.js";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      [`${USERS_API_BASE}/users`]: {
        target: "http://localhost:8081",
        changeOrigin: true
      },
      [`${POSTS_API_BASE}/posts`]: {
        target: "http://localhost:8081",
        changeOrigin: true
      }
    }
  }
});