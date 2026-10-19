import type { Config } from "tailwindcss";

const config: Config = {
  content: ["./src/**/*.{js,ts,jsx,tsx}"],
  theme: {
    extend: {
      colors: {
        paper: "#f3ecdf",
        card: "#fffaf3",
        ink: "#241c15",
        muted: "#6f6256",
        line: "#e6d9c6",
        leaf: "#1f6b45",
        clay: "#b5482a",
      },
      fontFamily: {
        sans: ["var(--font-sarabun)", "sans-serif"],
      },
      boxShadow: {
        card: "0 12px 30px rgba(56, 38, 18, 0.06)",
      },
    },
  },
  plugins: [],
};

export default config;
