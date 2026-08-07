import React from 'react';

interface LandingPageProps {
  onNavigate: (label: string) => void;
}

export const LandingPage: React.FC<LandingPageProps> = ({ onNavigate }) => {
  return (
    <div className="min-h-screen bg-slate-50 text-slate-800 font-sans selection:bg-indigo-500 selection:text-white">
      {/* --- HEADER --- */}
      <header className="sticky top-0 z-50 bg-white/80 backdrop-blur-md border-b border-slate-100">
        <div className="max-w-7xl mx-auto px-6 h-20 flex items-center justify-between">
          <div className="flex items-center gap-3">
            {/* Logo */}
            <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-indigo-600 to-purple-600 flex items-center justify-center shadow-lg shadow-indigo-200">
              <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M13 10V3L4 14h7v7l9-11h-7z" />
              </svg>
            </div>
            <span className="text-2xl font-black bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent tracking-tight">
              Tamkeen
            </span>
          </div>

          <nav className="hidden md:flex items-center gap-8 text-sm font-semibold text-slate-600">
            <a href="#accueil" className="hover:text-indigo-600 transition-colors">Accueil</a>
            <a href="#features" className="hover:text-indigo-600 transition-colors">Fonctionnalités</a>
            <a href="#how-it-works" className="hover:text-indigo-600 transition-colors">Comment ça marche</a>
            <a href="#contact" className="hover:text-indigo-600 transition-colors">Contact</a>
          </nav>

          <button
            onClick={() => onNavigate('Login')}
            className="bg-indigo-50/80 hover:bg-indigo-100 text-indigo-600 hover:text-indigo-700 text-sm font-bold px-5 py-2.5 rounded-xl border border-indigo-100 transition-all active:scale-95"
          >
            Accès Admin
          </button>
        </div>
      </header>

      {/* --- HERO SECTION --- */}
      <section id="accueil" className="relative pt-12 pb-24 md:pt-20 md:pb-32 overflow-hidden">
        <div className="max-w-7xl mx-auto px-6 grid grid-cols-1 lg:grid-cols-12 gap-16 items-center">
          {/* Left Column: Copy */}
          <div className="lg:col-span-7 space-y-8 text-center lg:text-left">
            <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-indigo-50 border border-indigo-100/50">
              <span className="w-2 h-2 rounded-full bg-indigo-600 animate-pulse"></span>
              <span className="text-xs font-bold text-indigo-600 uppercase tracking-wider">Disponible en Algérie</span>
            </div>
            
            <h1 className="text-4xl sm:text-5xl lg:text-6xl font-black text-slate-900 tracking-tight leading-tight sm:leading-none">
              Trouvez les meilleurs <br />
              <span className="bg-gradient-to-r from-indigo-600 via-purple-600 to-indigo-600 bg-clip-text text-transparent">
                prestataires de services
              </span> <br />
              autour de vous.
            </h1>

            <p className="text-lg text-slate-500 max-w-2xl mx-auto lg:mx-0 leading-relaxed">
              Plomberie, électricité, nettoyage, peinture ou menuiserie... Tous vos besoins quotidiens résolus en un clic. Discutez instantanément avec des professionnels vérifiés.
            </p>

            <div className="flex flex-col sm:flex-row items-center justify-center lg:justify-start gap-4">
              {/* Google Play Download Button */}
              <a 
                href="https://play.google.com/store" 
                target="_blank" 
                rel="noreferrer"
                className="bg-slate-900 hover:bg-slate-800 text-white flex items-center gap-3 px-6 py-3 rounded-2xl shadow-xl shadow-slate-900/10 transition-all hover:-translate-y-0.5 active:scale-95"
              >
                {/* Play Store Icon */}
                <svg className="w-6 h-6" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M3 5.277L14.773 17 3 21.723v-16.4M3 3a1.47 1.47 0 00-.73.2c.4.4 12.5 12.5 12.5 12.5L21 9.47 3 3m11.77 12.73L21 11.47l-6.23 4.26M3 22c.6 0 1.2-.3 1.77-.6L21 14.53l-6.23-1.8L3 22z"/>
                </svg>
                <div className="text-left">
                  <p className="text-[10px] text-slate-400 uppercase font-semibold leading-none">Disponible sur</p>
                  <p className="text-base font-bold leading-tight">Google Play</p>
                </div>
              </a>

              <a 
                href="#features"
                className="bg-white hover:bg-slate-50 text-slate-700 font-bold text-sm px-6 py-4 rounded-2xl border border-slate-200 transition-all hover:border-slate-300"
              >
                En savoir plus
              </a>
            </div>
          </div>

          {/* Right Column: Visual App Mockup */}
          <div className="lg:col-span-5 relative flex justify-center">
            {/* Ambient Glow */}
            <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-72 h-72 rounded-full bg-indigo-400/20 blur-3xl -z-10"></div>
            
            {/* Phone Frame */}
            <div className="w-[300px] h-[600px] rounded-[48px] bg-slate-900 p-3 shadow-2xl border-4 border-slate-800 relative">
              {/* Dynamic Notch */}
              <div className="absolute top-6 left-1/2 -translate-x-1/2 w-28 h-6 rounded-full bg-slate-950 flex items-center justify-center">
                <span className="w-2.5 h-2.5 rounded-full bg-slate-850 mr-4"></span>
                <span className="w-1.5 h-1.5 rounded-full bg-slate-850"></span>
              </div>

              {/* App Screen Mockup */}
              <div className="w-full h-full rounded-[38px] bg-indigo-950 overflow-hidden relative p-4 flex flex-col justify-between">
                {/* Header */}
                <div className="flex items-center justify-between pt-8 pb-4">
                  <div className="flex items-center gap-2">
                    <div className="w-6 h-6 rounded-md bg-indigo-600 flex items-center justify-center">
                      <span className="text-xs text-white font-bold">T</span>
                    </div>
                    <span className="text-xs font-bold text-white">Tamkeen</span>
                  </div>
                  <div className="w-6 h-6 rounded-full bg-white/10 flex items-center justify-center">
                    <span className="text-[10px] text-white">DZ</span>
                  </div>
                </div>

                {/* Main Card */}
                <div className="bg-white rounded-3xl p-4 shadow-xl space-y-4 my-auto">
                  <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-full bg-indigo-100 flex items-center justify-center">
                      <svg className="w-5 h-5 text-indigo-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                      </svg>
                    </div>
                    <div>
                      <p className="text-[10px] text-slate-400 font-semibold uppercase leading-none">Votre Localisation</p>
                      <p className="text-xs font-bold text-slate-800">Alger, Algérie</p>
                    </div>
                  </div>

                  <div className="relative">
                    <input 
                      type="text" 
                      placeholder="Plombier, Électricien..." 
                      disabled
                      className="w-full bg-slate-50 border border-slate-100 rounded-xl px-4 py-2.5 text-xs text-slate-400"
                    />
                    <div className="absolute right-3 top-2.5 w-6 h-6 bg-indigo-600 rounded-lg flex items-center justify-center">
                      <svg className="w-3.5 h-3.5 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                      </svg>
                    </div>
                  </div>

                  {/* Mock Provider Card */}
                  <div className="border border-slate-100 rounded-2xl p-3 flex items-center gap-3 bg-slate-50">
                    <div className="w-10 h-10 rounded-full bg-slate-200 overflow-hidden">
                      <img src="https://ui-avatars.com/api/?name=Ahmed+Benali&background=4A3AFF&color=fff" alt="Ahmed" className="w-full h-full object-cover" />
                    </div>
                    <div className="flex-1">
                      <div className="flex justify-between items-center">
                        <p className="text-xs font-bold text-slate-800">Ahmed Benali</p>
                        <span className="text-[9px] bg-green-100 text-green-700 px-1.5 py-0.5 rounded font-semibold">Disponible</span>
                      </div>
                      <p className="text-[9px] text-slate-400">Plomberie & Chauffage</p>
                      <div className="flex items-center gap-1 mt-1">
                        <span className="text-yellow-400 text-xs">★</span>
                        <span className="text-[10px] text-slate-600 font-bold">4.9</span>
                        <span className="text-[9px] text-slate-400">(42 avis)</span>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Footer Badges */}
                <div className="bg-white/10 backdrop-blur-md rounded-2xl p-3 flex justify-between items-center text-center">
                  <div className="flex-1 border-r border-white/10">
                    <p className="text-white font-bold text-sm">10k+</p>
                    <p className="text-[8px] text-indigo-200">Prestations</p>
                  </div>
                  <div className="flex-1 border-r border-white/10">
                    <p className="text-white font-bold text-sm">4.8</p>
                    <p className="text-[8px] text-indigo-200">Note moyenne</p>
                  </div>
                  <div className="flex-1">
                    <p className="text-white font-bold text-sm">100%</p>
                    <p className="text-[8px] text-indigo-200">Sécurisé</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* --- FEATURES SECTION --- */}
      <section id="features" className="py-24 bg-white border-y border-slate-100">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center max-w-2xl mx-auto space-y-4 mb-16">
            <h2 className="text-3xl font-extrabold text-slate-900 tracking-tight sm:text-4xl">
              Pourquoi choisir l'application Tamkeen ?
            </h2>
            <p className="text-slate-500 text-base leading-relaxed">
              Une plateforme moderne construite pour simplifier la vie des clients et booster l'activité des artisans en Algérie.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-12">
            {/* Feature 1 */}
            <div className="bg-slate-50/50 border border-slate-100 rounded-3xl p-8 space-y-6 hover:shadow-xl hover:shadow-slate-100 transition-all duration-300">
              <div className="w-12 h-12 rounded-2xl bg-indigo-50 border border-indigo-100 flex items-center justify-center text-indigo-600">
                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                </svg>
              </div>
              <h3 className="text-xl font-bold text-slate-900">Artisans Vérifiés</h3>
              <p className="text-slate-500 leading-relaxed text-sm">
                Tous les prestataires inscrits sur Tamkeen font l'objet d'une vérification stricte de leur identité et de leurs compétences pour garantir des services de haute qualité.
              </p>
            </div>

            {/* Feature 2 */}
            <div className="bg-slate-50/50 border border-slate-100 rounded-3xl p-8 space-y-6 hover:shadow-xl hover:shadow-slate-100 transition-all duration-300">
              <div className="w-12 h-12 rounded-2xl bg-indigo-50 border border-indigo-100 flex items-center justify-center text-indigo-600">
                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                </svg>
              </div>
              <h3 className="text-xl font-bold text-slate-900">Chat Instantané</h3>
              <p className="text-slate-500 leading-relaxed text-sm">
                Discutez, envoyez des photos des travaux et négociez le tarif en temps réel directement à travers notre module de messagerie sécurisé.
              </p>
            </div>

            {/* Feature 3 */}
            <div className="bg-slate-50/50 border border-slate-100 rounded-3xl p-8 space-y-6 hover:shadow-xl hover:shadow-slate-100 transition-all duration-300">
              <div className="w-12 h-12 rounded-2xl bg-indigo-50 border border-indigo-100 flex items-center justify-center text-indigo-600">
                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <h3 className="text-xl font-bold text-slate-900">Packs de Crédits Flexibles</h3>
              <p className="text-slate-500 leading-relaxed text-sm">
                Pour les prestataires, payez uniquement lorsque vous choisissez de débloquer les coordonnées d'un client. Un système de crédits transparent et sans engagement.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* --- FOOTER --- */}
      <footer id="contact" className="bg-slate-900 text-slate-400 py-16">
        <div className="max-w-7xl mx-auto px-6 grid grid-cols-1 md:grid-cols-3 gap-12 border-b border-slate-800 pb-12">
          {/* Logo & Slogan */}
          <div className="space-y-4">
            <div className="flex items-center gap-3">
              <div className="w-8 h-8 rounded-lg bg-indigo-600 flex items-center justify-center">
                <svg className="w-5 h-5 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M13 10V3L4 14h7v7l9-11h-7z" />
                </svg>
              </div>
              <span className="text-xl font-black text-white tracking-tight">Tamkeen</span>
            </div>
            <p className="text-sm text-slate-400 leading-relaxed">
              La plateforme leader pour connecter les professionnels et les clients en Algérie en toute sécurité.
            </p>
          </div>

          {/* Links */}
          <div className="space-y-4">
            <h4 className="text-white font-bold text-sm uppercase tracking-wider">Liens Utiles</h4>
            <ul className="space-y-2.5 text-sm">
              <li>
                <a href="/privacy-policy.html" target="_blank" className="hover:text-white transition-colors">
                  Politique de Confidentialité
                </a>
              </li>
              <li>
                <a href="#features" className="hover:text-white transition-colors">Fonctionnalités</a>
              </li>
              <li>
                <button onClick={() => onNavigate('Login')} className="hover:text-white transition-colors">
                  Administration
                </button>
              </li>
            </ul>
          </div>

          {/* Support */}
          <div className="space-y-4">
            <h4 className="text-white font-bold text-sm uppercase tracking-wider">Contact & Support</h4>
            <p className="text-sm">
              Des questions ou besoin d'assistance ? Écrivez-nous à :
            </p>
            <p className="text-indigo-400 font-bold text-base hover:text-indigo-300 transition-colors">
              tamkeenapp16@gmail.com
            </p>
          </div>
        </div>

        <div className="max-w-7xl mx-auto px-6 pt-8 flex flex-col sm:flex-row justify-between items-center text-xs text-slate-500 gap-4">
          <p>© {new Date().getFullYear()} Tamkeen. Tous droits réservés.</p>
          <p>Conçu pour simplifier les services en Algérie.</p>
        </div>
      </footer>
    </div>
  );
};
