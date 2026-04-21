import { useMemo } from 'react';
import {
  Navigate,
  Route,
  Routes,
  useLocation,
} from 'react-router-dom';
import Sidebar from './components/Sidebar';
import ClientesList from './pages/clientes/List';
import ClientesAdesao from './pages/clientes/Adesao';
import ClienteDetalhes from './pages/clientes/Detalhes';
import CestaConfig from './pages/admin/CestaConfig';
import MotorControle from './pages/admin/MotorControle';
import Dashboard from './pages/dashboard/Dashboard';

function App() {
  const location = useLocation();

  const path = location.pathname;
  // Título dinâmico baseado na rota atual
  const title = useMemo(() => {
    if (path === '/clientes/novo') return 'Adesão ao Produto';
    if (path.startsWith('/clientes/detalhes')) return 'Detalhes do Cliente';
    if (path.startsWith('/clientes')) return 'Gestão de Clientes';
    if (path === '/admin/cesta') return 'Cesta Recomendada';
    if (path === '/admin/motor') return 'Motor de Compra';
    return 'Visão Geral (Dashboard)';
  }, [path]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50/10 to-slate-100 text-slate-900 transition-colors dark:bg-slate-950 dark:text-slate-100">
      <Sidebar />

      <div className="flex flex-1 flex-col md:ml-72">
          <header className="sticky top-0 z-10 flex items-center justify-between border-b border-orange-500/20 bg-[#003399] backdrop-blur-md shadow-sm px-6 py-4 dark:border-slate-800 dark:bg-slate-900">
            <div>
              <p className="text-sm text-blue-100 dark:text-slate-400">
                Itaú Corretora - Compra Programada
              </p>
              <h1 className="text-2xl font-bold text-white dark:text-slate-100">{title}</h1>
            </div>
          </header>

          <main className="flex-1 p-4 md:p-8">
            <section className="relative">
              <Routes>
                <Route path="/" element={<Navigate to="/dashboard" replace />} />
                <Route path="/dashboard" element={<Dashboard />} />
                <Route path="/clientes" element={<ClientesList />} />
                <Route path="/clientes/novo" element={<ClientesAdesao />} />
                <Route path="/clientes/detalhes/:id" element={<ClienteDetalhes />} />
                <Route path="/admin/cesta" element={<CestaConfig />} />
                <Route path="/admin/motor" element={<MotorControle />} />
              </Routes>
            </section>
          </main>
      </div>
    </div>
  );
}

export default App;
