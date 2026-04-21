/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  darkMode: 'class',
  theme: {
    extend: {
      fontFamily: {
        display: ['Inter', 'system-ui', 'sans-serif'],
      },
      colors: {
        brand: {
          50: '#fff7ed',
          100: '#ffede5',
          200: '#ffd0bc',
          300: '#ff9c74',
          400: '#ff7000',
          500: '#ec7000',
          600: '#dd5a00',
          700: '#b84400',
          800: '#913700',
          900: '#763100',
        },
        itau: {
          navy: '#003399',
          orange: '#EC7000',
          gray: '#666666',
        }
      },
    },
  },
  plugins: [],
}

