import React, { useState, useEffect, Suspense, lazy } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, Link, useNavigate, useLocation } from 'react-router-dom';
import { authApi } from './services/api';
import { User } from './types';
import { CookieBanner } from './components/CookieBanner';
import { 
  Brain, LayoutDashboard, Users, CalendarDays, 
  Settings, LogOut, Loader2, Menu, X, UserIcon,
  RefreshCw
} from 'lucide-react';

const LandingPageModern = lazy(() => import('./pages/LandingPage/LandingPageModern').then(m => ({ default: m.LandingPageModern })));
const Login = lazy(() => import('./pages/Login').then(m => ({ default: m.Login })));
const Register = lazy(() => import('./pages/Register').then(m => ({ default: m.Register })));
const ExternalAuthCallback = lazy(() => import('./pages/ExternalAuthCallback').then(m => ({ default: m.ExternalAuthCallback })));
const PsycholoogDashboard = lazy(() => import('./pages/PsycholoogDashboard').then(m => ({ default: m.PsycholoogDashboard })));
const PatientDashboard = lazy(() => import('./pages/PatientDashboard').then(m => ({ default: m.PatientDashboard })));
const Patients = lazy(() => import('./pages/Patients').then(m => ({ default: m.Patients })));
const AfspraakTypes = lazy(() => import('./pages/AfspraakTypes').then(m => ({ default: m.AfspraakTypes })));
const CalendarPage = lazy(() => import('./pages/CalendarPage').then(m => ({ default: m.CalendarPage })));

// Background preloading object for instant zero-latency route transitions
export const pagePreloaders: Record<string, () => Promise<any>> = {
  '/dashboard': () => import('./pages/PsycholoogDashboard'),
  '/calendar': () => import('./pages/CalendarPage'),
  '/patients': () => import('./pages/Patients'),
  '/appointment-types': () => import('./pages/AfspraakTypes'),
  '/portal': () => import('./pages/PatientDashboard'),
  '/login': () => import('./pages/Login'),
  '/register': () => import('./pages/Register'),
};

const PageLoader = () => (
  <div className="min-h-[50vh] flex items-center justify-center">
    <Loader2 className="animate-spin h-8 w-8 text-brand-600 dark:text-brand-400" />
  </div>
);

const App: React.FC = () => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const checkSession = async () => {
      try {
        const currentUser = await authApi.me();
        setUser(currentUser);
        localStorage.setItem('user', JSON.stringify(currentUser));
      } catch (err) {
        console.log('No active session.');
        setUser(null);
        localStorage.removeItem('user');
      } finally {
        setLoading(false);
      }
    };
    checkSession();

    // Achtergrond preloading: laad alle andere pagina-componenten in tijdens browser idle-time
    const idleTimer = setTimeout(() => {
      Object.values(pagePreloaders).forEach(preload => preload().catch(() => {}));
    }, 800);

    return () => clearTimeout(idleTimer);
  }, []);

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-50">
        <Loader2 className="animate-spin h-10 w-10 text-brand-600" />
      </div>
    );
  }

  return (
    <Router>
      <Suspense fallback={<PageLoader />}>
        <Routes>
          <Route 
            path="/" 
            element={<LandingPageModern user={user} />} 
          />
          <Route 
            path="/modern" 
            element={<LandingPageModern user={user} />} 
          />

          <Route 
            path="/login" 
            element={user ? <Navigate to={user.isPsycholoog ? "/dashboard" : "/portal"} replace /> : <Login setUser={setUser} />} 
          />
          <Route 
            path="/register" 
            element={user ? <Navigate to="/portal" replace /> : <Register setUser={setUser} />} 
          />
          <Route 
            path="/external-auth-callback" 
            element={<ExternalAuthCallback setUser={setUser} />} 
          />
          
          {/* Protected App Routes */}
          <Route 
            path="/*" 
            element={
              user ? (
                <AppLayout user={user} setUser={setUser}>
                  <Routes>
                    {user.isPsycholoog ? (
                      <>
                        <Route path="/dashboard" element={<PsycholoogDashboard initialTab="agenda" />} />
                        <Route path="/calendar" element={<CalendarPage />} />
                        <Route path="/patients" element={<Patients />} />
                        <Route path="/ai-lab" element={<PsycholoogDashboard initialTab="ai_lab" />} />
                        <Route path="/google-sync" element={<PsycholoogDashboard initialTab="google_setup" />} />
                        <Route path="/appointment-types" element={<AfspraakTypes />} />
                        <Route path="*" element={<Navigate to="/dashboard" replace />} />
                      </>
                    ) : (
                      <>
                        <Route path="/portal" element={<PatientDashboard />} />
                        <Route path="*" element={<Navigate to="/portal" replace />} />
                      </>
                    )}
                  </Routes>
                </AppLayout>
              ) : (
                <Navigate to="/login" replace />
              )
            } 
          />
        </Routes>
      </Suspense>
      <CookieBanner />
    </Router>
  );
};

interface AppLayoutProps {
  user: User;
  setUser: (user: User | null) => void;
  children: React.ReactNode;
}

const AppLayout: React.FC<AppLayoutProps> = ({ user, setUser, children }) => {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = async () => {
    try {
      await authApi.logout();
      setUser(null);
      localStorage.removeItem('user');
      navigate('/login');
    } catch (err) {
      console.error(err);
    }
  };

  const navItems = user.isPsycholoog 
    ? [
        { path: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
        { path: '/calendar', label: 'Agenda & Kalender', icon: CalendarDays },
        { path: '/patients', label: 'Patiënten', icon: Users },
        { path: '/ai-lab', label: 'AI Lab', icon: Brain },
        { path: '/google-sync', label: 'Google Agenda Sync', icon: RefreshCw },
        { path: '/appointment-types', label: 'Afspraaktypes', icon: Settings },
      ]
    : [
        { path: '/portal', label: 'Mijn Portaal', icon: CalendarDays },
      ];

  return (
    <div className="min-h-screen flex bg-slate-50 dark:bg-brand-950 text-slate-800 dark:text-brand-100 transition-colors duration-300 relative">
      {/* Skip to Main Content Link (WCAG 2.4.1) */}
      <a 
        href="#main-content" 
        className="sr-only focus:not-sr-only focus:absolute focus:top-3 focus:left-3 focus:z-[100] focus:px-4 focus:py-2.5 focus:bg-brand-800 focus:text-white focus:rounded-xl focus:shadow-xl focus:outline-none focus:ring-2 focus:ring-brand-400 font-semibold text-xs"
      >
        Ga direct naar de inhoud
      </a>

      {/* Sidebar Desktop */}
      <aside className="hidden md:flex flex-col w-64 bg-white dark:bg-brand-900 border-r border-slate-100 dark:border-brand-800/40 p-6 space-y-8 min-h-screen shadow-sm sticky top-0">
        {/* Brand */}
        <div className="flex justify-center w-full py-1">
          <img src="/images/logo_hero.png" alt="De Verstandhouding" className="h-24 w-auto object-contain dark:hidden mx-auto" />
          <img 
            src="/images/logo_dark_compact.png" 
            onError={(e) => { e.currentTarget.src = '/images/logo_dark_compact.svg'; }}
            alt="De Verstandhouding" 
            className="h-24 w-auto object-contain hidden dark:block mx-auto rounded-2xl overflow-hidden shadow-md" 
          />
        </div>

        {/* User Card */}
        <div className="p-4 bg-slate-50 dark:bg-brand-950 rounded-2xl border border-slate-100 dark:border-brand-800/40 flex items-center space-x-3">
          <div className="h-9 w-9 rounded-xl bg-brand-100 dark:bg-brand-800 text-brand-700 dark:text-brand-300 flex items-center justify-center">
            <UserIcon className="h-4 w-4" />
          </div>
          <div className="min-w-0">
            <p className="text-xs font-bold text-slate-700 dark:text-brand-50 truncate">{user.voornaam} {user.achternaam}</p>
            <p className="text-[9px] uppercase tracking-wider font-semibold text-slate-400 dark:text-brand-400">
              {user.isPsycholoog ? 'Psycholoog' : 'Patiënt'}
            </p>
          </div>
        </div>

        {/* Navigation Links */}
        <nav className="flex-1 space-y-1.5" aria-label="Hoofdnavigatie">
          {navItems.map((item) => {
            const Icon = item.icon;
            const isActive = location.pathname === item.path;
            return (
              <Link
                key={item.path}
                to={item.path}
                onMouseEnter={() => {
                  if (pagePreloaders[item.path]) pagePreloaders[item.path]();
                }}
                className={`flex items-center space-x-3 px-4 py-3 rounded-2xl text-sm font-semibold transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 ${
                  isActive 
                    ? 'bg-brand-800 text-white shadow-lg shadow-brand-500/15' 
                    : 'text-slate-500 dark:text-brand-300 hover:text-slate-700 dark:hover:text-brand-50 hover:bg-slate-50 dark:hover:bg-brand-800/30'
                }`}
              >
                <Icon className="h-5 w-5" />
                <span>{item.label}</span>
              </Link>
            );
          })}
        </nav>

        {/* Footer actions */}
        <div>
          <button
            onClick={handleLogout}
            className="w-full flex items-center space-x-3 px-4 py-3 rounded-2xl text-sm font-semibold text-red-500 hover:bg-red-50 transition focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-red-500"
          >
            <LogOut className="h-5 w-5" />
            <span>Uitloggen</span>
          </button>
        </div>
      </aside>

      {/* Mobile Drawer */}
      {sidebarOpen && (
        <div className="fixed inset-0 z-50 flex md:hidden bg-slate-900/40 backdrop-blur-sm">
          <div className="w-64 bg-white dark:bg-brand-900 p-6 flex flex-col space-y-6 animate-slide-in">
            <div className="flex justify-between items-center">
              <div className="flex items-center">
                <img src="/images/logo_hero.png" alt="De Verstandhouding" className="h-14 w-auto object-contain dark:hidden" />
                <img 
                  src="/images/logo_dark_compact.png" 
                  onError={(e) => { e.currentTarget.src = '/images/logo_dark_compact.svg'; }}
                  alt="De Verstandhouding" 
                  className="h-14 w-auto object-contain hidden dark:block rounded-xl overflow-hidden shadow-xs" 
                />
              </div>
              <button 
                onClick={() => setSidebarOpen(false)}
                aria-label="Sluit navigatiemenu"
                className="p-1 rounded-lg hover:bg-slate-100 dark:hover:bg-brand-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500"
              >
                <X className="h-5 w-5 text-slate-500 dark:text-brand-300" />
              </button>
            </div>
            
            <div className="p-3 bg-slate-50 dark:bg-brand-950 rounded-xl flex items-center space-x-2">
              <UserIcon className="h-4 w-4 text-brand-600 dark:text-brand-400" />
              <div className="text-xs">
                <p className="font-bold text-slate-700 dark:text-brand-50">{user.voornaam} {user.achternaam}</p>
                <p className="text-[9px] text-slate-400 dark:text-brand-400 uppercase font-semibold">{user.isPsycholoog ? 'Psycholoog' : 'Patiënt'}</p>
              </div>
            </div>

            <nav className="flex-grow space-y-2" aria-label="Mobiele hoofdnavigatie">
              {navItems.map((item) => {
                const Icon = item.icon;
                const isActive = location.pathname === item.path;
                return (
                  <Link
                    key={item.path}
                    to={item.path}
                    onClick={() => setSidebarOpen(false)}
                    className={`flex items-center space-x-3 px-4 py-2.5 rounded-xl text-sm font-semibold transition focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 ${
                      isActive 
                        ? 'bg-brand-800 text-white shadow-md shadow-brand-500/10' 
                        : 'text-slate-500 dark:text-brand-300 hover:bg-slate-50 dark:hover:bg-brand-800/30'
                    }`}
                  >
                    <Icon className="h-4 w-4" />
                    <span>{item.label}</span>
                  </Link>
                );
              })}
            </nav>

            <button
              onClick={handleLogout}
              className="flex items-center space-x-3 px-4 py-2.5 rounded-xl text-sm font-semibold text-red-500 hover:bg-red-50 transition focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-red-500"
            >
              <LogOut className="h-4 w-4" />
              <span>Uitloggen</span>
            </button>
          </div>
        </div>
      )}

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col min-h-screen bg-slate-50 dark:bg-brand-950 transition-colors duration-300">
        {/* Mobile Header */}
        <header className="flex md:hidden items-center justify-between bg-white dark:bg-brand-900 px-6 py-4 border-b border-slate-100 dark:border-brand-800/40 shadow-sm">
          <div className="flex items-center">
            <img src="/images/logo_hero.png" alt="De Verstandhouding" className="h-14 w-auto object-contain dark:hidden" />
            <img 
              src="/images/logo_dark_compact.png" 
              onError={(e) => { e.currentTarget.src = '/images/logo_dark_compact.svg'; }}
              alt="De Verstandhouding" 
              className="h-14 w-auto object-contain hidden dark:block rounded-xl overflow-hidden shadow-xs" 
            />
          </div>
          <button 
            onClick={() => setSidebarOpen(true)}
            aria-label="Open navigatiemenu"
            className="p-2 rounded-xl bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800/40 text-slate-600 dark:text-brand-300 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500"
          >
            <Menu className="h-5 w-5" />
          </button>
        </header>

        {/* Content Body */}
        <main id="main-content" className="flex-grow overflow-x-hidden" tabIndex={-1}>
          {children}
        </main>
      </div>
    </div>
  );
};

export default App;
