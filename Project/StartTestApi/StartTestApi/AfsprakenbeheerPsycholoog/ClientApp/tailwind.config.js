/** @type {import('tailwindcss').Config} */
export default {
  darkMode: 'class',
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        brand: {
          50: '#FCFAF7',
          100: '#f3e9e7',
          200: '#e2cbd2',
          300: '#f3b5b4',
          400: '#eea3a2',
          500: '#e89c9b',
          600: '#d38c8a',
          700: '#5f80a0',
          800: '#7292b2',
          900: '#4b6c8a',
          950: '#1d1d1b',
        },
      },
      fontFamily: {
        sans: ['Akkurat', 'Inter', '-apple-system', 'sans-serif'],
        header: ['Apollo', 'Cormorant Garamond', 'serif'],
        title: ['Apollo', 'Cormorant Garamond', 'serif'],
        cursive: ['Alex Brush', 'cursive'],
      },
    },
  },
  plugins: [],
}
