import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import './index.css';
import App from './App';
import { ThemeProvider } from './hooks/useTheme';

// Inicialização da aplicação React com roteamento e gerenciamento de tema
createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <ThemeProvider>
    <App />
      </ThemeProvider>
    </BrowserRouter>
  </StrictMode>,
);
