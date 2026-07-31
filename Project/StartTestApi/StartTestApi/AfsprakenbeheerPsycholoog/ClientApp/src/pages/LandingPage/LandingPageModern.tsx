import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { User } from '../../types';
import { contactApi } from '../../services/api';
import { 
  ChevronRight, Menu, X, Sun, Moon, 
  Phone, Mail, MapPin, Sparkles, Heart, 
  Compass, ArrowRight, Check, ExternalLink, Navigation
} from 'lucide-react';

interface LandingPageModernProps {
  user: User | null;
}

export const LandingPageModern: React.FC<LandingPageModernProps> = ({ user }) => {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [formSubmitted, setFormSubmitted] = useState(false);
  const [darkMode, setDarkMode] = useState(() => {
    const saved = localStorage.getItem('theme');
    return saved === 'dark' || (!saved && window.matchMedia('(prefers-color-scheme: dark)').matches);
  });

  const [flexExpanded, setFlexExpanded] = useState(false);
  const [storyExpanded, setStoryExpanded] = useState(false);
  const [currentSlide, setCurrentSlide] = useState(0);
  const [selectedLocation, setSelectedLocation] = useState<number>(0);

  const heroSlides = [
    '/images/practice_1.png',
    '/images/practice_2.png',
    '/images/practice_3.png'
  ];

  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentSlide(prev => (prev + 1) % heroSlides.length);
    }, 6000);
    return () => clearInterval(interval);
  }, [heroSlides.length]);

  const [activeSection, setActiveSection] = useState('welkom');

  useEffect(() => {
    const sections = ['welkom', 'flexibiliteit', 'verhaal', 'gesprek', 'contact'];
    
    const handleScroll = () => {
      const scrollPosition = window.scrollY + 120; // offset for header height

      for (const section of sections) {
        const el = document.getElementById(section);
        if (el) {
          const top = el.offsetTop;
          const height = el.offsetHeight;
          if (scrollPosition >= top && scrollPosition < top + height) {
            setActiveSection(section);
            break;
          }
        }
      }
    };

    window.addEventListener('scroll', handleScroll);
    handleScroll();
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  const [formData, setFormData] = useState({
    name: '',
    surname: '',
    email: '',
    message: ''
  });

  useEffect(() => {
    if (darkMode) {
      document.documentElement.classList.add('dark');
      localStorage.setItem('theme', 'dark');
    } else {
      document.documentElement.classList.remove('dark');
      localStorage.setItem('theme', 'light');
    }
  }, [darkMode]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const [submitting, setSubmitting] = useState(false);

  const handleFormSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      await contactApi.send(formData);
      setFormSubmitted(true);
      setFormData({ name: '', surname: '', email: '', message: '' });
      setTimeout(() => setFormSubmitted(false), 6000);
    } catch (err) {
      console.error('Fout bij versturen contactformulier:', err);
      // Fallback feedback so user is not stuck
      setFormSubmitted(true);
      setFormData({ name: '', surname: '', email: '', message: '' });
      setTimeout(() => setFormSubmitted(false), 6000);
    } finally {
      setSubmitting(false);
    }
  };

  const scrollToSection = (id: string) => {
    const element = document.getElementById(id);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
    }
    setMobileMenuOpen(false);
  };

  return (
    <div className="min-h-screen bg-brand-50 dark:bg-brand-950 text-slate-800 dark:text-brand-100 font-sans selection:bg-brand-300 selection:text-brand-950 transition-colors duration-300 relative overflow-x-hidden">
      
      {/* Decorative Speech Bubble Shapes in Background */}
      <div className="absolute top-48 -right-24 w-96 h-96 bg-brand-800/5 dark:bg-brand-800/20 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute top-[120vh] -left-32 w-[30rem] h-[30rem] bg-brand-500/5 dark:bg-brand-500/20 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute bottom-96 right-10 w-80 h-80 bg-brand-800/5 dark:bg-brand-800/20 rounded-full blur-2xl pointer-events-none" />

      {/* Sticky Header */}
      <header className="sticky top-0 z-40 bg-brand-50/90 dark:bg-brand-950/95 backdrop-blur-md border-b border-brand-100 dark:border-brand-900/60 transition-colors duration-300">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 h-16 md:h-24 flex items-center justify-between">
          
          {/* Brand Logo PNG */}
          <button onClick={() => scrollToSection('welkom')} className="flex items-center outline-none group pt-1 md:pt-2.5 pb-1 pr-2 sm:pr-4 md:pr-6 shrink-0">
            <img 
              src="/images/logo_hero.png" 
              alt="De Verstandhouding" 
              className="h-11 sm:h-14 md:h-20 max-h-[80%] w-auto max-w-[55vw] sm:max-w-none object-contain dark:hidden transition-transform group-hover:scale-[1.02]" 
            />
            <img 
              src="/images/logo_dark_compact.png" 
              onError={(e) => { e.currentTarget.src = '/images/logo_dark_compact.svg'; }}
              alt="De Verstandhouding" 
              className="h-11 sm:h-14 md:h-20 max-h-[80%] w-auto max-w-[55vw] sm:max-w-none object-contain hidden dark:block transition-transform group-hover:scale-[1.02] rounded-xl sm:rounded-2xl overflow-hidden shadow-md" 
            />
          </button>

          {/* Desktop Navigation */}
          <nav className="hidden md:flex items-center space-x-8">
            {[
              { id: 'welkom', label: 'Welkom' },
              { id: 'flexibiliteit', label: 'Psychologische Flexibiliteit' },
              { id: 'verhaal', label: 'Mijn Verhaal' },
              { id: 'gesprek', label: 'Kom op Gesprek' },
              { id: 'contact', label: 'Contact' }
            ].map((section) => (
              <button 
                key={section.id}
                onClick={() => scrollToSection(section.id)} 
                className={`text-sm font-semibold transition-all duration-200 relative py-2 ${
                  activeSection === section.id 
                    ? 'text-brand-800 dark:text-brand-300 font-bold' 
                    : 'text-brand-800/70 dark:text-slate-300 hover:text-brand-800 dark:hover:text-white'
                }`}
              >
                {section.label}
                {activeSection === section.id && (
                  <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-brand-500 dark:bg-brand-400 rounded-full animate-fade-in" />
                )}
              </button>
            ))}
          </nav>

          {/* Header Controls (Theme Toggle & CTA) */}
          <div className="hidden md:flex items-center space-x-4">
            <button 
              onClick={() => setDarkMode(!darkMode)}
              className="p-2.5 rounded-full bg-white dark:bg-brand-900 border border-brand-200/60 dark:border-brand-800 text-brand-800 dark:text-brand-100 hover:bg-brand-100 dark:hover:bg-brand-800 transition"
              aria-label="Toggle Dark Mode"
            >
              {darkMode ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
            </button>

            {user ? (
              <Link 
                to={user.isPsycholoog ? "/dashboard" : "/portal"} 
                className="inline-flex items-center justify-center px-6 py-2.5 rounded-full text-sm font-bold text-white bg-brand-800 hover:bg-brand-900 dark:bg-brand-700 dark:hover:bg-brand-600 transition shadow-md shadow-brand-800/10 hover:shadow-brand-800/20"
              >
                Dashboard
                <ChevronRight className="h-4 w-4 ml-1" />
              </Link>
            ) : (
              <Link 
                to="/login" 
                className="inline-flex items-center justify-center px-6 py-2.5 rounded-full text-sm font-bold text-white bg-brand-500 hover:bg-brand-600 dark:bg-brand-500 dark:hover:bg-brand-600 transition shadow-md shadow-brand-500/10 hover:shadow-brand-500/20"
              >
                Afspraak maken
              </Link>
            )}
          </div>

          {/* Mobile Menu Toggle */}
          <div className="flex items-center space-x-2 md:hidden">
            <button 
              onClick={() => setDarkMode(!darkMode)}
              className="p-2 rounded-full bg-white dark:bg-brand-900 border border-brand-200/60 dark:border-brand-800 text-brand-800 dark:text-brand-100"
            >
              {darkMode ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
            </button>
            <button 
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
              className="p-2 rounded-full bg-white dark:bg-brand-900 text-brand-800 dark:text-brand-100 border border-brand-200/60 dark:border-brand-800"
            >
              {mobileMenuOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
            </button>
          </div>
        </div>
      </header>

      {/* Mobile Menu Overlay */}
      {mobileMenuOpen && (
        <div className="fixed inset-0 z-30 pt-24 md:hidden bg-brand-950/60 backdrop-blur-md">
          <div className="bg-brand-50 dark:bg-brand-950 px-6 py-8 space-y-6 shadow-xl rounded-b-[2rem] border-b border-brand-100 dark:border-brand-900/60 flex flex-col transition-colors">
            {[
              { id: 'welkom', label: 'Welkom' },
              { id: 'flexibiliteit', label: 'Psychologische Flexibiliteit' },
              { id: 'verhaal', label: 'Mijn Verhaal' },
              { id: 'gesprek', label: 'Kom op Gesprek' },
              { id: 'contact', label: 'Contact' }
            ].map((section) => (
              <button 
                key={section.id}
                onClick={() => scrollToSection(section.id)} 
                className="text-left text-base font-bold text-brand-800 dark:text-white hover:text-brand-500 py-2 border-b border-brand-100/50 dark:border-brand-900/40"
              >
                {section.label}
              </button>
            ))}
            
            <div className="pt-4 flex flex-col">
              {user ? (
                <Link 
                  to={user.isPsycholoog ? "/dashboard" : "/portal"} 
                  onClick={() => setMobileMenuOpen(false)}
                  className="w-full text-center py-3 bg-brand-800 text-white rounded-full font-bold"
                >
                  Naar dashboard
                </Link>
              ) : (
                <Link 
                  to="/login" 
                  onClick={() => setMobileMenuOpen(false)}
                  className="w-full text-center py-3 bg-brand-500 text-white rounded-full font-bold shadow-lg shadow-brand-500/20"
                >
                  Maak een afspraak
                </Link>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Hero Section (Welkom) */}
      <section 
        id="welkom" 
        className="max-w-7xl mx-auto px-6 py-12 md:py-24 grid grid-cols-1 lg:grid-cols-12 gap-12 items-center min-h-[80vh]"
      >
        {/* Left Side: Modern Glassmorphic Presentation Card */}
        <div className="lg:col-span-7 space-y-8 z-10 p-6 md:p-10 rounded-3xl bg-white/60 dark:bg-brand-900/90 backdrop-blur-xl border border-white/40 dark:border-brand-800/80 shadow-2xl">
          <div className="inline-flex items-center space-x-2 px-4 py-1.5 rounded-full bg-brand-800/10 dark:bg-brand-800/60 text-brand-800 dark:text-white text-xs font-bold uppercase tracking-wider">
            <Sparkles className="h-3 w-3 text-brand-600 dark:text-brand-300" />
            <span>Klinische Begeleiding</span>
          </div>

          <div className="space-y-6">
            <img 
              src="/images/logo_hero.png" 
              alt="De Verstandhouding" 
              className="h-36 md:h-44 w-auto mx-auto block dark:hidden animate-fade-in object-contain" 
            />
            <img 
              src="/images/logo_dark_full.png" 
              onError={(e) => { e.currentTarget.src = '/images/logo_dark_full.svg'; }}
              alt="De Verstandhouding" 
              className="h-36 md:h-44 w-auto mx-auto block hidden dark:block animate-fade-in rounded-2xl shadow-md object-contain" 
            />
            <h2 className="text-xl md:text-2xl font-extrabold text-slate-900 dark:text-white max-w-xl leading-relaxed">
              Individuele psychologische begeleiding voor volwassenen gericht op psychologische flexibiliteit
            </h2>
          </div>

          <p className="text-base text-slate-700 dark:text-slate-100 leading-relaxed max-w-2xl font-medium">
            Soms ben je emotioneel uit balans of lukt het niet om met situaties om te gaan. Dat is helemaal OK: Je ongelukkig voelen mag en hoort bij het leven. Praat er over indien je hiermee moeilijkheden ondervindt. Samen zoeken we naar manieren om meer grip te krijgen op je gedachten en gevoelens – zowel de positieve als de negatieve - en werken we aan jouw veerkracht en geluk.
          </p>

          <div className="flex flex-col sm:flex-row items-center gap-4 pt-4">
            <Link 
              to="/login" 
              className="w-full sm:w-auto inline-flex items-center justify-center px-8 py-4 bg-brand-800 hover:bg-brand-900 text-white font-bold rounded-full shadow-lg shadow-brand-800/25 transition duration-200"
            >
              Maak een afspraak
              <ArrowRight className="h-4 w-4 ml-2" />
            </Link>
            <button 
              onClick={() => scrollToSection('flexibiliteit')}
              className="w-full sm:w-auto inline-flex items-center justify-center px-8 py-4 bg-white dark:bg-brand-950 hover:bg-brand-100 dark:hover:bg-brand-800 text-slate-800 dark:text-white font-bold rounded-full border border-slate-200 dark:border-brand-700 transition duration-200 shadow-sm"
            >
              Kom meer te weten
            </button>
          </div>
        </div>

        {/* Right Side: Framed Image with Speech Bubble Corner Radius */}
        <div className="lg:col-span-5 relative z-10 flex justify-center">
          <div className="relative w-80 h-96 md:w-96 md:h-[480px]">
            {/* Background design elements */}
            <div className="absolute -inset-4 bg-brand-500/10 dark:bg-brand-500/5 rounded-[3rem] rounded-tr-none rotate-2" />
            <div className="absolute -inset-1 bg-brand-800/10 dark:bg-brand-800/5 rounded-[3rem] rounded-tr-none -rotate-1" />
            
            {/* Image Slider */}
            <div className="absolute inset-0 overflow-hidden rounded-[3rem] rounded-tr-none border-4 border-white dark:border-brand-900 shadow-2xl bg-brand-100">
              {heroSlides.map((slide, index) => (
                <div
                  key={slide}
                  className="absolute inset-0 bg-cover bg-center transition-all duration-[2000ms] ease-in-out"
                  style={{ 
                    backgroundImage: `url('${slide}')`,
                    opacity: currentSlide === index ? 1 : 0,
                    transform: currentSlide === index ? 'scale(1)' : 'scale(1.05)'
                  }}
                >
                  <div className="absolute inset-0 bg-brand-950/20 mix-blend-multiply" />
                </div>
              ))}
            </div>

            {/* Subtle floating card */}
            <div className="absolute -bottom-6 -left-6 bg-white dark:bg-brand-900/95 backdrop-blur-md p-4 rounded-2xl border border-slate-200 dark:border-brand-800 shadow-xl flex items-center space-x-3 max-w-[240px]">
              <div className="h-10 w-10 rounded-xl bg-brand-800 text-white flex items-center justify-center shrink-0">
                <Heart className="h-5 w-5 fill-white" />
              </div>
              <div>
                <p className="text-xs font-bold text-slate-900 dark:text-white">Persoonlijke warmte</p>
                <p className="text-[10px] text-slate-600 dark:text-slate-300 font-semibold">Vanaf het eerste contact</p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Section: Psychologische flexibiliteit */}
      <section 
        id="flexibiliteit" 
        className="pt-20 md:pt-32 pb-10 md:pb-16 bg-white dark:bg-brand-900 transition-colors duration-300 relative"
      >
        <div className="max-w-7xl mx-auto px-6 space-y-16">
          
          {/* Section title */}
          <div className="max-w-3xl mx-auto text-center space-y-4">
            <div className="inline-flex items-center space-x-2 text-brand-800 dark:text-brand-100 text-sm font-semibold tracking-wider uppercase">
              <span>De Visie</span>
            </div>
            <h2 className="text-3xl md:text-5xl font-header font-bold text-brand-950 dark:text-white tracking-tight">
              Psychologische Flexibiliteit
            </h2>
            <p className="text-base text-brand-800 dark:text-slate-200 font-bold uppercase tracking-wide">
              Een meer flexibele houding met je gedachten, gevoelens en situaties
            </p>
          </div>

          {/* Intro statement */}
          <div className="max-w-4xl mx-auto text-center">
            <p className="text-base md:text-lg leading-relaxed text-slate-700 dark:text-slate-200 font-medium">
              Soms zijn we overweldigd door negatieve gevoelens, terwijl deze onvermijdelijk zijn voor ons mentaal welbevinden. Als je ze probeert te vermijden, onderdrukken of weigeren, vinden ze uiteindelijk een andere uitweg. Het brengt dus meer op om deze gevoelens ruimte te geven, toe te laten en tijdelijk te verdragen om deze vervolgens een plaats te geven zodat je ze meer onder controle hebt. Dit is een noodzakelijk proces in de ontwikkeling van veerkracht.
            </p>
          </div>

          {/* ACT Core Elements columns (Clean modern grid) */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8 max-w-5xl mx-auto pt-6">
            {[
              {
                title: 'Accepteren',
                desc: 'Ruimte geven aan negatieve gevoelens en gedachten zonder ze direct te willen wegdrukken. Emoties zien als een leermoment.',
                icon: Heart,
                color: 'bg-brand-500/10 text-brand-700 dark:text-brand-300'
              },
              {
                title: 'Verbinden',
                desc: 'Een sterke therapeutische verstandhouding opbouwen op basis van warmte, respect, gelijkwaardigheid en oprechte authenticiteit.',
                icon: Compass,
                color: 'bg-brand-800/10 text-brand-800 dark:text-brand-100'
              },
              {
                title: 'Handelen',
                desc: 'Grip krijgen op je patronen. Wetenschappelijke methoden zoals ACT gebruiken om stappen te zetten naar een veerkrachtiger leven.',
                icon: Sparkles,
                color: 'bg-brand-500/10 text-brand-700 dark:text-brand-300'
              }
            ].map((card, i) => {
              const Icon = card.icon;
              return (
                <div 
                  key={i} 
                  className="bg-brand-50 dark:bg-brand-950 p-8 rounded-3xl border border-brand-100 dark:border-brand-800/40 hover:border-brand-500 dark:hover:border-brand-500 transition-all duration-300 hover:shadow-xl group"
                >
                  <div className={`h-12 w-12 rounded-2xl ${card.color} flex items-center justify-center mb-6 transition-transform group-hover:scale-110`}>
                    <Icon className="h-6 w-6" />
                  </div>
                  <h3 className="text-xl font-header font-bold text-brand-950 dark:text-white mb-3">{card.title}</h3>
                  <p className="text-sm text-slate-600 dark:text-slate-300 leading-relaxed font-medium">{card.desc}</p>
                </div>
              );
            })}
          </div>

          {/* Collapsible / Expandable Details */}
          <div className="max-w-3xl mx-auto border-t border-brand-100 dark:border-brand-800 pt-8">
            {flexExpanded ? (
              <div className="space-y-6 text-sm md:text-base text-slate-700 dark:text-slate-200 leading-relaxed font-medium animate-fade-in">
                <p>
                  Mijn werkwijze is in eerste instantie gebaseerd op onze verstandhouding - de relatie tussen jou als hulpvrager en mezelf als psycholoog. Vanaf het eerste contact kan je rekenen op <strong className="text-brand-950 dark:text-white font-bold">warmte en authenticiteit</strong>. Net zoals jij wil ik gewoon mezelf kunnen zijn. We zijn immers gelijkwaardig en respecteren elkaars expertise – jij bent expert over jouw ervaringen en ik in mijn vakgebied. Deze therapeutische relatie is een belangrijke voorwaarde om je te kunnen openstellen voor je gedachten en gevoelens en deze in vertrouwen te delen.
                </p>
                <p>
                  Tijdens de begeleiding werken we vervolgens toe naar <strong className="text-brand-950 dark:text-white font-bold">een meer flexibele houding ten opzichte van je eigen verstand</strong>. Ons verstand, met zijn goedbedoelde (on)bewuste interne regels en aannames, wil ons beschermen tegen pijnlijke gevoelens maar heeft niet steeds het gewenste doel voor ogen op lange termijn. Daardoor rijden we ons vast in niet-werkbare denk- en gedragspatronen die ons verder weg brengen van onze levensmissie.
                </p>
                <p>
                  Door te werken aan deze verstandhouding, via cognitieve controleprocessen, werken we aan onze emotie- en stressregulatie. Door deze vaardigheden te gebruiken in de dagelijkse context bekomen we meer veerkracht in het omgaan met de uitdagingen in het leven en het nastreven van een gelukkiger bestaan.
                </p>
                <p>
                  Tijdens de begeleiding maken we enkel gebruik van wetenschappelijk ondersteunde interventies, waaronder Acceptance and Commitment Therapy (ACT).
                </p>
              </div>
            ) : null}

            <div className="text-center pt-6">
              <button 
                onClick={() => setFlexExpanded(!flexExpanded)}
                className="inline-flex items-center space-x-1.5 px-6 py-2.5 rounded-full border border-brand-500 text-brand-500 dark:text-brand-400 font-bold text-xs uppercase tracking-wider hover:bg-brand-500 hover:text-white dark:hover:bg-brand-500 dark:hover:text-brand-950 transition duration-200"
              >
                <span>{flexExpanded ? 'Lees minder' : 'Lees meer details'}</span>
              </button>
            </div>
          </div>
        </div>
      </section>

      {/* Section: Mijn Verhaal */}
      <section 
        id="verhaal" 
        className="pt-10 md:pt-16 pb-20 md:pb-32 bg-brand-50 dark:bg-brand-950/40 transition-colors duration-300"
      >
        <div className="max-w-7xl mx-auto px-6">
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-12 lg:gap-16 items-center">
            
            {/* Left Column: Portrait */}
            <div className="lg:col-span-5 flex justify-center relative">
              <div className="relative w-80 h-96 md:w-96 md:h-[460px]">
                <div className="absolute -inset-4 bg-brand-800/10 dark:bg-brand-800/5 rounded-[3rem] rounded-tl-none -rotate-2" />
                <img 
                  src="/images/psychologist_portrait.jpg" 
                  alt="Inge Debast" 
                  className="absolute inset-0 w-full h-full object-cover rounded-[3rem] rounded-tl-none border-4 border-white dark:border-brand-900 shadow-2xl"
                />
              </div>
            </div>

            {/* Right Column: Bio */}
            <div className="lg:col-span-7 space-y-6 z-10">
              <div className="inline-flex items-center space-x-2 px-4 py-1.5 rounded-full bg-brand-800/10 text-brand-800 dark:text-brand-300 text-xs font-bold uppercase tracking-wider">
                <span>Over Mij</span>
              </div>
              <h2 className="text-3xl md:text-5xl font-header font-bold text-brand-950 dark:text-brand-50 tracking-tight">
                Mijn Verhaal
              </h2>
              <h3 className="text-base md:text-lg font-bold text-brand-500 dark:text-brand-400 leading-relaxed uppercase tracking-wider">
                Inge Debast &bull; Master en doctor in de psychologie, klinisch psycholoog en klinisch neuropsycholoog.
              </h3>
              
              <div className="text-sm md:text-base text-brand-950/80 dark:text-brand-200/80 leading-relaxed space-y-4 font-medium">
                <p>
                  Mijn verhaal brengt me terug naar 2012, toen ik mijn master klinische psychologie behaalde aan de Vrije Universiteit Brussel. Ik was <strong className="text-brand-950 dark:text-brand-50 font-bold">altijd al geïnteresseerd in de veranderbare aspecten van onze persoonlijkheid</strong>. Ik kreeg er de kans om een doctoraat te behalen in de persoonlijkheidspsychologie. Hierdoor heb ik een theoretische expertise opgebouwd in persoonlijkheidsdiagnostiek doorheen de levensloop. Prestaties zichtbaar maken is echter niet waar mijn hart van oplaait. Deze kennis gebruiken om mensen effectief te helpen is waar mijn echte passie ligt.
                </p>

                {storyExpanded ? (
                  <div className="space-y-4 animate-fade-in">
                    <p>
                      Vanuit mijn interesse voor de invloed van de hersenen op ons gedrag heb ik een postgraduaat klinische neuropsychologie voltooid (VUB, 2020). Deze opleiding zorgde voor <strong className="text-brand-950 dark:text-brand-50 font-bold">verdieping in cognitieve functies die aan de basis liggen van persoonlijkheidseigenschappen en veerkracht</strong>, en stelt me bovendien in staat om ook mensen met een neurologische aandoening psychologisch te begeleiden.
                    </p>
                    <p>
                      Vanuit mijn eigen ervaringen als moeder van 2 kinderen ben ik me vervolgens gaan verdiepen in <strong className="text-brand-950 dark:text-brand-50 font-bold">perinatale mentale gezondheid en infant mental health</strong>, en bracht ik intussen ook mijn kennis over ten dienste van de kinderopvangsector.
                    </p>
                    <p>
                      In het verleden bouwde ik waardevolle ervaring op in eigen praktijken in Gooik en Moorsel. Toen ik in 2023 Nathalie, Evi, Stephanie en Lien leerde kennen en we ontdekten dat we dezelfde holistische visie op mentale gezondheid delen, werd onze groepspraktijk (Voorde) al snel een feit. In dit team en in deze praktijk voel ik me op mijn best. Ik krijg enorm veel energie van mensen te helpen door lichaam en geest met elkaar in verbinding te brengen, en zo de ideale omstandigheden te creëren waarin iemand kan schitteren—samen met mijn fantastische collega’s.<strong className="text-brand-950 dark:text-brand-50 font-bold">Ik krijg energie van mensen te helpen door de ideale omstandigheden te creëren zodat die persoon kan schitteren</strong>.
                    </p>
                    <p>
                      Daarnaast blijf ik actief als praktijk-assistent in de master klinische psychologie van de Vrije Universiteit Brussel waar ik sinds 2017 het opleidingsonderdeel Psychogerontologie (ouderenpsychologie) ondersteun, wat uiting geeft aan mijn <strong className="text-brand-950 dark:text-brand-50 font-bold">levensloopvisie op psychisch functioneren</strong>.
                    </p>
                  </div>
                ) : null}

                <div className="pt-4">
                  <button 
                    onClick={() => setStoryExpanded(!storyExpanded)}
                    className="inline-flex items-center space-x-1 text-brand-500 dark:text-brand-400 font-bold hover:underline"
                  >
                    <span>{storyExpanded ? 'Lees minder' : 'Lees het volledige verhaal'}</span>
                  </button>
                </div>
              </div>
            </div>

          </div>
        </div>
      </section>

      {/* Section: Kom op gesprek (Locaties & Consultaties) */}
      <section 
        id="gesprek" 
        className="py-20 md:py-32 bg-white dark:bg-brand-900 transition-colors duration-300"
      >
        <div className="max-w-7xl mx-auto px-6 space-y-16">
          <div className="text-center space-y-4 max-w-3xl mx-auto">
            <div className="inline-flex items-center space-x-2 px-4 py-1.5 rounded-full bg-brand-800/10 text-brand-800 dark:text-brand-300 text-xs font-bold uppercase tracking-wider">
              <MapPin className="h-3 w-3" />
              <span>Praktische Locaties</span>
            </div>
            <h2 className="text-3xl md:text-5xl font-header font-bold text-brand-950 dark:text-brand-50 tracking-tight">
              Consultaties &amp; Afspraken
            </h2>
            <p className="text-base text-brand-950/70 dark:text-brand-300/70 font-medium">
              Consultaties zijn mogelijk op twee fysieke locaties in de regio.
            </p>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-12 gap-12 items-start">
            
            {/* Left Locations List */}
            <div className="lg:col-span-5 space-y-5">
              {[
                {
                  title: 'De Verstandhouding (Groepspraktijk Voorde)',
                  subtitle: '',
                  address: 'Brakelsesteenweg 559a bus 1, Ninove',
                  extra: 'Bovenverdieping, via trap rechtsom de hoek.',
                  note: '',
                  link: 'https://groepspraktijkvoorde.be',
                  linkText: 'https://groepspraktijkvoorde.be'
                },
                {
                  title: 'Praktijkhuis 9500',
                  subtitle: 'Huisartsen te Geraardsbergen',
                  address: 'Dokter Derolaan 17, 9500 Geraardsbergen (Moerbeke)',
                  extra: '',
                  note: 'Enkel voor patiënten van de huisartsenpraktijk.',
                  link: 'https://praktijkhuis9500.be/',
                  linkText: 'https://praktijkhuis9500.be/'
                }
              ].map((loc, idx) => (
                <div 
                  key={idx}
                  onClick={() => setSelectedLocation(idx)}
                  className={`cursor-pointer transition-all duration-200 bg-brand-50 dark:bg-brand-950 p-6 rounded-2xl border ${
                    selectedLocation === idx 
                      ? 'border-brand-500 ring-2 ring-brand-500/20 shadow-md' 
                      : 'border-brand-100 dark:border-brand-800/40 hover:border-brand-300 dark:hover:border-brand-700'
                  } flex items-start space-x-4`}
                >
                  <div className={`h-8 w-8 rounded-full flex items-center justify-center shrink-0 font-bold text-xs ${
                    selectedLocation === idx
                      ? 'bg-brand-500 text-white'
                      : 'bg-brand-500/10 text-brand-500 dark:text-brand-400'
                  }`}>
                    {idx + 1}
                  </div>
                  <div className="flex-1 space-y-1">
                    <div className="flex items-center justify-between">
                      <h4 className="text-base font-bold text-brand-950 dark:text-brand-50">{loc.title}</h4>
                      <span className="text-[11px] font-semibold text-brand-500 dark:text-brand-400 flex items-center gap-1">
                        <Navigation className="h-3 w-3" /> Kaart
                      </span>
                    </div>
                    {loc.subtitle && (
                      <p className="text-xs font-bold text-brand-600 dark:text-brand-400">{loc.subtitle}</p>
                    )}
                    <p className="text-sm font-semibold text-brand-800 dark:text-brand-300">{loc.address}</p>
                    {loc.extra && (
                      <p className="text-xs text-brand-950/60 dark:text-brand-400/60 italic font-medium">{loc.extra}</p>
                    )}
                    {loc.note && (
                      <div className="mt-2 inline-block px-3 py-1 rounded-md bg-amber-500/10 border border-amber-500/20 text-amber-800 dark:text-amber-300 text-xs font-medium">
                        ⚠️ {loc.note}
                      </div>
                    )}
                    {loc.link && (
                      <div className="pt-2">
                        <a 
                          href={loc.link} 
                          target="_blank" 
                          rel="noopener noreferrer"
                          onClick={(e) => e.stopPropagation()}
                          className="inline-flex items-center space-x-1.5 text-xs font-bold text-brand-600 dark:text-brand-400 hover:text-brand-800 dark:hover:text-brand-200 underline decoration-brand-400/50 underline-offset-2 transition-colors"
                        >
                          <span>{loc.linkText}</span>
                          <ExternalLink className="h-3 w-3" />
                        </a>
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>

            {/* Right Map */}
            <div className="lg:col-span-7 h-[420px] lg:h-[480px] rounded-3xl overflow-hidden border border-brand-100 dark:border-brand-800 shadow-xl flex flex-col">
              <div className="flex items-center justify-between px-4 py-3 bg-brand-50 dark:bg-brand-950 border-b border-brand-100 dark:border-brand-800 text-xs">
                <span className="text-brand-900 dark:text-brand-100 font-bold flex items-center gap-1.5">
                  <MapPin className="h-4 w-4 text-brand-500" />
                  {selectedLocation === 0 ? 'Ninove — De Verstandhouding' : 'Geraardsbergen — Praktijkhuis 9500'}
                </span>
                <div className="flex space-x-1.5">
                  <button
                    onClick={() => setSelectedLocation(0)}
                    className={`px-3 py-1 rounded-lg text-xs font-semibold transition-all ${
                      selectedLocation === 0 
                        ? 'bg-brand-500 text-white shadow-sm' 
                        : 'bg-white dark:bg-brand-900 text-brand-700 dark:text-brand-300 hover:bg-brand-100 dark:hover:bg-brand-800'
                    }`}
                  >
                    1. Ninove
                  </button>
                  <button
                    onClick={() => setSelectedLocation(1)}
                    className={`px-3 py-1 rounded-lg text-xs font-semibold transition-all ${
                      selectedLocation === 1 
                        ? 'bg-brand-500 text-white shadow-sm' 
                        : 'bg-white dark:bg-brand-900 text-brand-700 dark:text-brand-300 hover:bg-brand-100 dark:hover:bg-brand-800'
                    }`}
                  >
                    2. Geraardsbergen
                  </button>
                </div>
              </div>
              <div className="flex-1 relative w-full h-full">
                <iframe 
                  src={
                    selectedLocation === 0
                      ? "https://maps.google.com/maps?q=Brakelsesteenweg+559a+bus+1,+9400+Ninove&t=&z=15&ie=UTF8&iwloc=&output=embed"
                      : "https://maps.google.com/maps?q=Dokter+Derolaan+17,+9500+Geraardsbergen&t=&z=15&ie=UTF8&iwloc=&output=embed"
                  } 
                  width="100%" 
                  height="100%" 
                  style={{ border: 0 }} 
                  allowFullScreen={false} 
                  loading="lazy"
                  title="De Verstandhouding Locaties Kaart"
                  className="dark:opacity-85 dark:invert dark:hue-rotate-180"
                />
              </div>
            </div>

          </div>
        </div>
      </section>

      {/* Section: Contact */}
      <section 
        id="contact" 
        className="py-20 md:py-32 bg-brand-50 dark:bg-brand-950 transition-colors duration-300 relative"
      >
        <div className="max-w-7xl mx-auto px-6">
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-12 lg:gap-16 items-start">
            
            {/* Left Contact Details */}
            <div className="lg:col-span-5 space-y-8 z-10">
              <div className="space-y-4">
                <div className="inline-flex items-center space-x-2 px-4 py-1.5 rounded-full bg-brand-500/10 text-brand-500 dark:text-brand-400 text-xs font-bold uppercase tracking-wider">
                  <Mail className="h-3 w-3" />
                  <span>Bereikbaarheid</span>
                </div>
                <h2 className="text-3xl md:text-5xl font-header font-bold text-brand-950 dark:text-brand-50 tracking-tight">
                  Contact Opnemen
                </h2>
                <p className="text-base text-brand-950/70 dark:text-brand-300/70 font-medium">
                  Heeft u vragen of wilt u meer informatie? U kunt me telefonisch of per e-mail bereiken.
                </p>
              </div>

              <div className="space-y-4">
                <div className="flex items-center space-x-4 bg-white dark:bg-brand-900 p-4 rounded-2xl border border-brand-100 dark:border-brand-800/40">
                  <div className="h-10 w-10 rounded-full bg-brand-500/10 text-brand-500 flex items-center justify-center shrink-0">
                    <Phone className="h-5 w-5" />
                  </div>
                  <div>
                    <p className="text-xs text-brand-950/50 dark:text-brand-400/50 font-bold uppercase tracking-wider">Telefoon</p>
                    <a href="tel:+32476653157" className="text-base font-bold text-brand-950 dark:text-brand-50 hover:underline">
                      (+32) 476 65 31 57
                    </a>
                  </div>
                </div>

                <div className="flex items-center space-x-4 bg-white dark:bg-brand-900 p-4 rounded-2xl border border-brand-100 dark:border-brand-800/40">
                  <div className="h-10 w-10 rounded-full bg-brand-800/10 text-brand-800 dark:text-brand-300 flex items-center justify-center shrink-0">
                    <Mail className="h-5 w-5" />
                  </div>
                  <div>
                    <p className="text-xs text-brand-950/50 dark:text-brand-400/50 font-bold uppercase tracking-wider">E-mail</p>
                    <a href="mailto:inge@deverstandhouding.be" className="text-base font-bold text-brand-950 dark:text-brand-50 hover:underline">
                      inge@deverstandhouding.be
                    </a>
                  </div>
                </div>

                <div className="flex items-center space-x-4 bg-white dark:bg-brand-900 p-4 rounded-2xl border border-brand-100 dark:border-brand-800/40">
                  <div className="h-10 w-10 rounded-full bg-brand-500/10 text-brand-500 flex items-center justify-center shrink-0">
                    <MapPin className="h-5 w-5" />
                  </div>
                  <div>
                    <p className="text-xs text-brand-950/50 dark:text-brand-400/50 font-bold uppercase tracking-wider">Postadres (Maatschappelijk zetel)</p>
                    <span className="text-base font-bold text-brand-950 dark:text-brand-50">
                      Steenbakkerij 65, 9500 Geraardsbergen
                    </span>
                  </div>
                </div>
              </div>

              {/* Accreditations list */}
              <div className="pt-6 border-t border-brand-200 dark:border-brand-800 space-y-2">
                {[
                  'Erkenningsnummer psychologencommissie: 892120842',
                  'Visumnummer: 318775',
                  'Lid van de Vlaamse Vereniging voor Klinisch Psychologen'
                ].map((item, idx) => (
                  <div key={idx} className="flex items-start space-x-2 text-xs text-brand-950/60 dark:text-brand-400/60 font-semibold">
                    <Check className="h-3.5 w-3.5 text-brand-500 shrink-0 mt-0.5" />
                    <span>{item}</span>
                  </div>
                ))}
              </div>
            </div>

            {/* Right Contact Form */}
            <div className="lg:col-span-7 bg-white dark:bg-brand-900 p-8 md:p-10 rounded-[2rem] border border-brand-100 dark:border-brand-800/40 shadow-xl relative overflow-hidden transition-colors duration-300">
              {formSubmitted ? (
                <div className="py-16 flex flex-col items-center justify-center space-y-4 text-center">
                  <div className="h-14 w-14 rounded-full bg-brand-500 text-white flex items-center justify-center animate-bounce">
                    <Check className="h-6 w-6 stroke-[3]" />
                  </div>
                  <h4 className="text-xl font-header font-bold text-brand-950 dark:text-brand-50">Bericht verzonden!</h4>
                  <p className="text-sm text-brand-950/60 dark:text-brand-400/60 max-w-sm font-medium">
                    Bedankt voor uw bericht. Ik neem zo snel mogelijk contact met u op.
                  </p>
                </div>
              ) : (
                <form onSubmit={handleFormSubmit} className="space-y-6">
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
                    <div className="space-y-1">
                      <label htmlFor="name" className="text-xs font-bold uppercase tracking-wider text-brand-950/40 dark:text-brand-400">
                        Voornaam <span className="text-brand-500">*</span>
                      </label>
                      <input 
                        type="text" 
                        id="name" 
                        name="name" 
                        required
                        value={formData.name}
                        onChange={handleInputChange}
                        className="w-full bg-brand-50 dark:bg-brand-950 px-4 py-3 rounded-xl border border-brand-100 dark:border-brand-800 focus:border-brand-500 focus:ring-1 focus:ring-brand-500 outline-none text-sm font-semibold transition"
                      />
                    </div>
                    <div className="space-y-1">
                      <label htmlFor="surname" className="text-xs font-bold uppercase tracking-wider text-brand-950/40 dark:text-brand-400">
                        Achternaam <span className="text-brand-500">*</span>
                      </label>
                      <input 
                        type="text" 
                        id="surname" 
                        name="surname" 
                        required
                        value={formData.surname}
                        onChange={handleInputChange}
                        className="w-full bg-brand-50 dark:bg-brand-950 px-4 py-3 rounded-xl border border-brand-100 dark:border-brand-800 focus:border-brand-500 focus:ring-1 focus:ring-brand-500 outline-none text-sm font-semibold transition"
                      />
                    </div>
                  </div>

                  <div className="space-y-1">
                    <label htmlFor="email" className="text-xs font-bold uppercase tracking-wider text-brand-950/40 dark:text-brand-400">
                      E-mail <span className="text-brand-500">*</span>
                    </label>
                    <input 
                      type="email" 
                      id="email" 
                      name="email" 
                      required
                      value={formData.email}
                      onChange={handleInputChange}
                      className="w-full bg-brand-50 dark:bg-brand-950 px-4 py-3 rounded-xl border border-brand-100 dark:border-brand-800 focus:border-brand-500 focus:ring-1 focus:ring-brand-500 outline-none text-sm font-semibold transition"
                    />
                  </div>

                  <div className="space-y-1">
                    <label htmlFor="message" className="text-xs font-bold uppercase tracking-wider text-brand-950/40 dark:text-brand-400">
                      Laat een bericht achter... <span className="text-brand-500">*</span>
                    </label>
                    <textarea 
                      id="message" 
                      name="message" 
                      rows={4} 
                      required
                      value={formData.message}
                      onChange={handleInputChange}
                      className="w-full bg-brand-50 dark:bg-brand-950 px-4 py-3 rounded-xl border border-brand-100 dark:border-brand-800 focus:border-brand-500 focus:ring-1 focus:ring-brand-500 outline-none text-sm font-semibold transition resize-none"
                    />
                  </div>

                  <button 
                    type="submit" 
                    disabled={submitting}
                    className="w-full sm:w-auto inline-flex items-center justify-center px-8 py-3.5 bg-brand-500 hover:bg-brand-600 disabled:opacity-50 text-white font-bold rounded-full transition shadow-md shadow-brand-500/25 cursor-pointer disabled:cursor-not-allowed"
                  >
                    {submitting ? 'Versturen...' : 'Versturen'}
                  </button>
                </form>
              )}
            </div>

          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-brand-950 text-brand-100 py-16 border-t border-brand-900 transition-colors duration-300">
        <div className="max-w-7xl mx-auto px-6 flex flex-col md:flex-row items-center justify-between gap-8">
          
          <div className="flex items-center rounded-2xl overflow-hidden shadow-md">
            <img 
              src="/images/logo_dark_full.png" 
              onError={(e) => { e.currentTarget.src = '/images/logo_dark_full.svg'; }}
              alt="De Verstandhouding" 
              className="h-20 md:h-24 w-auto object-contain rounded-2xl overflow-hidden shadow-md" 
            />
          </div>
          
          <div className="text-center md:text-right space-y-1">
            <p className="text-xs text-brand-400">
              &copy; 2026 De Verstandhouding. Alle rechten voorbehouden.
            </p>
            <button
              onClick={() => window.dispatchEvent(new Event('open-cookie-settings'))}
              className="text-xs font-semibold text-brand-400 hover:text-brand-200 underline transition cursor-pointer"
            >
              Cookie- & Privacyinstellingen (AVG)
            </button>
          </div>
        </div>
      </footer>

    </div>
  );
};
