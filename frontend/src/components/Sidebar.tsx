import {
  HomeIcon,
  UsersIcon,
  ChartPieIcon,
  AdjustmentsHorizontalIcon,
  Bars3Icon,
} from '@heroicons/react/24/outline';
import { useEffect, useState, type ComponentType, type SVGProps } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import ThemeToggle from './ThemeToggle';

type NavItem = {
  label: string;
  icon: ComponentType<SVGProps<SVGSVGElement>>;
  path: string;
};

// Itens de navegação da sidebar
const navItems: NavItem[] = [
  { label: 'Dashboard', icon: HomeIcon, path: '/dashboard' },
  { label: 'Clientes', icon: UsersIcon, path: '/clientes' },
  { label: 'Cesta Recomendada', icon: ChartPieIcon, path: '/admin/cesta' },
  { label: 'Motor de Compra', icon: AdjustmentsHorizontalIcon, path: '/admin/motor' },
];

export default function Sidebar() {
  const [isOpen, setIsOpen] = useState(false);
  const location = useLocation();

  // Fecha o menu mobile ao navegar para outra página
  useEffect(() => {
    setIsOpen(false);
  }, [location.pathname]);

  // Componente de lista de navegação com estilização condicional para rota ativa
  const NavList = () => (
    <div className="mt-6 space-y-1">
      {navItems.map(({ label, icon: Icon, path }) => (
        <NavLink
          key={path}
          to={path}
          className={({ isActive }: { isActive: boolean }) =>
            [
              'group flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-colors',
              isActive
                ? 'bg-brand-50 text-brand-800 ring-1 ring-brand-100 dark:bg-brand-900/40 dark:text-brand-50 dark:ring-brand-800/60'
                : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900 dark:text-slate-300 dark:hover:bg-slate-800/60 dark:hover:text-white',
            ].join(' ')
          }
        >
          <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-white shadow-sm ring-1 ring-slate-200 transition group-hover:scale-[1.02] dark:bg-slate-800 dark:ring-slate-700">
            <Icon className="h-5 w-5 text-[#EC7000] dark:text-orange-400" />
          </span>
          <span>{label}</span>
        </NavLink>
      ))}
    </div>
  );

  return (
    <>
      <aside className="fixed left-0 top-0 hidden h-screen w-72 flex-col border-r border-slate-200/80 bg-white/95 backdrop-blur-sm px-4 py-6 shadow-lg dark:border-slate-800 dark:bg-slate-900 dark:shadow-none md:flex overflow-y-auto">
        <div className="flex items-center justify-between px-2">
          <div>
            <p className="text-xs uppercase tracking-[0.18em] text-orange-500 dark:text-orange-400 font-bold">
              Itaú Corretora
            </p>
            <h2 className="text-lg font-bold text-[#003399] dark:text-white">Compra Programada</h2>
          </div>
          <ThemeToggle compact />
        </div>

        <NavList />
      </aside>

      {/* Menu mobile - visível apenas em telas pequenas */}
      <div className="md:hidden">
        <div className="flex items-center justify-between border-b border-slate-200 bg-white px-4 py-3 dark:border-slate-800 dark:bg-slate-900">
          <button
            onClick={() => setIsOpen((prev) => !prev)}
            className="rounded-lg border border-slate-200 bg-white p-2.5 shadow-sm transition hover:bg-slate-100 dark:border-slate-800 dark:bg-slate-800"
            aria-label="Abrir navegação"
          >
            <Bars3Icon className="h-5 w-5 text-[#0000bf] dark:text-slate-100" />
          </button>
          <ThemeToggle compact />
        </div>

        {/* Menu dropdown mobile com overlay */}
        {isOpen && (
          <div className="fixed inset-x-0 top-14 z-20 border-b border-slate-200 bg-white/95 px-4 pb-4 pt-3 shadow-lg backdrop-blur dark:border-slate-800 dark:bg-slate-900/95">
            <NavList />
          </div>
        )}
      </div>
    </>
  );
}
