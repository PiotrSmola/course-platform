<template>
  <section class="hero" id="start">
    <div class="container">
      <div class="hero-grid">
        <div class="hero-copy">
          <span class="eyebrow">
            <span class="eyebrow-dot"></span>
            Nowa edycja jesienna 2026
          </span>

          <h1 class="hero-title">
            Naucz się <span class="grad-text">rzeczy,</span><br>
            które mają sens.
          </h1>

          <p class="hero-lead">
            Kursy online prowadzone przez praktyków z branży IT, designu i biznesu.
            Ucz się we własnym tempie, rozwiązuj realne zadania i buduj projekty do portfolio.
          </p>

          <div class="hero-cta">
            <router-link class="btn btn-primary" :to="{ name: 'Courses' }">
              <span>Przeglądaj kursy</span>
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
                <path d="M5 12h14M13 5l7 7-7 7"/>
              </svg>
            </router-link>
            <router-link class="btn btn-ghost" :to="trialDestination">
              Wypróbuj za darmo
            </router-link>
          </div>

          <ul class="hero-metrics">
            <li>
              <strong>{{ stats?.totalCount ?? '—' }}</strong>
              <span>kursów</span>
            </li>
            <li class="divider"></li>
            <li>
              <strong>{{ featuredRating }}</strong>
              <span>średnia ocen</span>
            </li>
            <li class="divider"></li>
            <li>
              <strong>{{ featuredCount }}</strong>
              <span>na stronie głównej</span>
            </li>
          </ul>
        </div>

        <aside class="hero-visual">
          <div class="float-card card-1 glass-card">
            <div class="card-header">
              <span class="card-badge live">Live</span>
              <span class="card-time">19:00</span>
            </div>
            <p class="card-title">React Server Components w praktyce</p>
            <p class="card-meta">Anna K. · 45 min · za 2 godz.</p>
          </div>

          <div class="float-card card-2 glass-card">
            <div class="card-progress">
              <div class="card-progress-ring">
                <svg viewBox="0 0 56 56">
                  <circle cx="28" cy="28" r="24" fill="none" stroke="rgba(255,255,255,0.1)" stroke-width="4"/>
                  <circle cx="28" cy="28" r="24" fill="none" stroke="url(#ring-g)" stroke-width="4" stroke-linecap="round" stroke-dasharray="150.8" stroke-dashoffset="52"/>
                  <defs>
                    <linearGradient id="ring-g" x1="0" y1="0" x2="1" y2="1">
                      <stop offset="0" stop-color="#f59e0b"/>
                      <stop offset="1" stop-color="#ec4899"/>
                    </linearGradient>
                  </defs>
                </svg>
                <span>65%</span>
              </div>
              <div>
                <p class="card-eyebrow">W toku</p>
                <p class="card-title-sm">Vue 3 Mastery</p>
                <p class="card-meta-sm">10/16 modułów</p>
              </div>
            </div>
          </div>

          <div class="float-card card-3 glass-card">
            <div class="card-cert">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6">
                <circle cx="12" cy="8" r="6"/>
                <path d="M9 13l-2 8 5-3 5 3-2-8"/>
              </svg>
              <div>
                <p class="card-eyebrow">Certyfikat</p>
                <p class="card-title-sm">UI Design Fundamentals</p>
                <p class="card-meta-sm">ukończono 28.10</p>
              </div>
            </div>
          </div>
        </aside>
      </div>
    </div>
  </section>

  <section class="paths" id="sciezki">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Ścieżki kariery</span>
        <h2>Wybierz kierunek, w którym chcesz się rozwijać</h2>
        <p>Spójne programy złożone z kilku kursów — od fundamentu po zaawansowane tematy.</p>
      </div>

      <div class="paths-grid">
        <article class="path-card glass-card">
          <span class="path-icon" style="--accent: #f59e0b">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="16 18 22 12 16 6"/>
              <polyline points="8 6 2 12 8 18"/>
            </svg>
          </span>
          <h3>Frontend Developer</h3>
          <p>HTML, CSS, JavaScript, Vue 3 i React. Od statycznych stron po aplikacje SPA.</p>
          <div class="path-meta">
            <span>6 kursów</span>
            <span>52 godz.</span>
          </div>
        </article>

        <article class="path-card glass-card">
          <span class="path-icon" style="--accent: #22d3ee">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">
              <ellipse cx="12" cy="5" rx="9" ry="3"/>
              <path d="M3 5v6c0 1.7 4 3 9 3s9-1.3 9-3V5"/>
              <path d="M3 11v6c0 1.7 4 3 9 3s9-1.3 9-3v-6"/>
            </svg>
          </span>
          <h3>Backend Developer</h3>
          <p>C#, .NET, Node.js, bazy danych, REST API. Architektura systemów i dobre praktyki.</p>
          <div class="path-meta">
            <span>5 kursów</span>
            <span>48 godz.</span>
          </div>
        </article>

        <article class="path-card glass-card">
          <span class="path-icon" style="--accent: #ec4899">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="13" cy="6" r="2"/>
              <circle cx="6" cy="18" r="2"/>
              <circle cx="20" cy="18" r="2"/>
              <path d="M13 8v4l4 4M6 16v-2l4-4"/>
            </svg>
          </span>
          <h3>UI/UX Designer</h3>
          <p>Figma, design systemy, prototypowanie. Badania użytkowników i testy użyteczności.</p>
          <div class="path-meta">
            <span>4 kursy</span>
            <span>36 godz.</span>
          </div>
        </article>

        <article class="path-card glass-card">
          <span class="path-icon" style="--accent: #a78bfa">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">
              <rect x="3" y="3" width="18" height="18" rx="2"/>
              <path d="M3 9h18M9 21V9"/>
            </svg>
          </span>
          <h3>Data &amp; AI</h3>
          <p>Python, SQL, machine learning, analiza danych. Praca z realnymi zbiorami danych.</p>
          <div class="path-meta">
            <span>5 kursów</span>
            <span>44 godz.</span>
          </div>
        </article>

        <article class="path-card glass-card">
          <span class="path-icon" style="--accent: #14b8a6">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12 2L4 7v6c0 5 3.5 9 8 11 4.5-2 8-6 8-11V7l-8-5z"/>
              <path d="M9 12l2 2 4-4"/>
            </svg>
          </span>
          <h3>DevOps &amp; Cloud</h3>
          <p>Docker, Kubernetes, CI/CD, AWS. Od lokalnego środowiska po produkcję.</p>
          <div class="path-meta">
            <span>4 kursy</span>
            <span>38 godz.</span>
          </div>
        </article>

        <article class="path-card glass-card">
          <span class="path-icon" style="--accent: #f472b6">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">
              <rect x="6" y="2" width="12" height="20" rx="3"/>
              <line x1="12" y1="18" x2="12" y2="18"/>
            </svg>
          </span>
          <h3>Mobile Developer</h3>
          <p>React Native i Flutter. Budowanie aplikacji na iOS i Androida z jednego kodu.</p>
          <div class="path-meta">
            <span>4 kursy</span>
            <span>34 godz.</span>
          </div>
        </article>
      </div>
    </div>
  </section>

  <section class="features" id="dlaczego">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Dlaczego CoursePlatform</span>
        <h2>Ucz się inaczej niż gdziekolwiek indziej</h2>
      </div>

      <div class="features-grid">
        <article class="feature-card glass-card">
          <div class="feature-num">01</div>
          <h3>Realne projekty, nie slajdy</h3>
          <p>Każdy kurs kończy się projektem, który możesz dodać do portfolio lub pokazać rekruterowi.</p>
        </article>

        <article class="feature-card glass-card">
          <div class="feature-num">02</div>
          <h3>Mentorzy z branży</h3>
          <p>Instruktorzy pracują na co dzień w firmach produktowych — uczą tego, co naprawdę używają w pracy.</p>
        </article>

        <article class="feature-card glass-card">
          <div class="feature-num">03</div>
          <h3>Code review i feedback</h3>
          <p>Twoje rozwiązania są recenzowane przez mentorów. Dostajesz konkretne wskazówki, nie ogólniki.</p>
        </article>

        <article class="feature-card glass-card">
          <div class="feature-num">04</div>
          <h3>Certyfikat ukończenia</h3>
          <p>Po zakończeniu kursu otrzymujesz certyfikat z podpisem mentora i weryfikowalnym linkiem.</p>
        </article>
      </div>
    </div>
  </section>

  <section class="featured-section" v-if="featuredCourses.length">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Polecane</span>
        <h2>Popularne kursy</h2>
      </div>
      <div class="featured-grid">
        <router-link
          v-for="course in featuredCourses"
          :key="course.id"
          class="featured-card glass-card"
          :to="{ name: 'CourseDetails', params: { id: course.id } }"
        >
          <CourseThumbnail class="thumb" :url="course.thumbnailUrl" />
          <div class="info">
            <h3>{{ course.title }}</h3>
            <p>{{ course.instructorName }} · {{ course.averageRating.toFixed(1) }} ★</p>
          </div>
        </router-link>
      </div>
    </div>
  </section>

  <section class="cta-section" id="start-learning">
    <div class="container">
      <div class="cta-card glass-card">
        <div class="cta-content">
          <h2>Gotowy, żeby zacząć?</h2>
          <p>Dołącz do 18 000 studentów, którzy uczą się z nami. Pierwszy kurs dostępny za darmo.</p>
        </div>
        <div class="cta-actions">
          <router-link class="btn btn-primary" :to="{ name: 'Courses' }">Wybierz kurs</router-link>
          <router-link class="btn btn-ghost" :to="trialDestination">Wypr&#243;buj za darmo</router-link>
        </div>
      </div>
    </div>
  </section>
  <section class="home-faq">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Pomoc</span>
        <h2>Najcz&#281;&#347;ciej zadawane pytania</h2>
        <p>Kr&#243;tkie odpowiedzi przed wyborem kursu.</p>
      </div>
      <FaqList :compact="true" />
      <router-link class="faq-link" :to="{ name: 'Faq' }">Zobacz wszystkie odpowiedzi</router-link>
    </div>
  </section>

</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { getCourses } from '@/features/courses/api/courses.api'
import { queryKeys } from '@/shared/queryKeys'
import CourseThumbnail from '@/shared/components/media/CourseThumbnail.vue'
import { usePopularCourses } from '@/features/courses/composables/usePopularCourses'
import FaqList from '@/features/public/components/FaqList.vue'

const authStore = useAuthStore()

const { data: stats } = useQuery({
  queryKey: queryKeys.catalogStats(),
  queryFn: () => getCourses({ pageSize: 1 })
})

const { data: featuredData } = usePopularCourses()

const featuredCourses = computed(() => {
  const courses = featuredData.value ?? []
  if (courses.length >= 8) return courses.slice(0, 8)
  return courses.length >= 4 ? courses.slice(0, 4) : []
})

const trialDestination = computed(() =>
  authStore.isAuthenticated
    ? { name: 'Trial' }
    : { name: 'Register', query: { redirect: '/trial' } }
)

const featuredRating = computed(() => {
  const items = featuredCourses.value
  if (!items.length) return '—'
  const avg = items.reduce((sum, c) => sum + c.averageRating, 0) / items.length
  return avg.toFixed(1)
})

const featuredCount = computed(() => featuredCourses.value.length || '—')
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.hero {
  padding: calc($header-height + 50px) 0 60px;
  position: relative;
}

.hero-grid {
  display: grid;
  grid-template-columns: 1.15fr 0.85fr;
  align-items: center;
  gap: 60px;

  @media (max-width: 980px) {
    grid-template-columns: 1fr;
    gap: 48px;
  }
}

.hero-copy {
  max-width: 580px;
}

.eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 6px 14px 6px 12px;
  font-size: 0.78rem;
  font-weight: 600;
  letter-spacing: 0.04em;
  color: $color-ink;
  box-shadow: 0 6px 18px rgba(3, 6, 24, 0.3);
}

.eyebrow-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: $color-grad;
  box-shadow: 0 0 12px rgba(245, 158, 11, 0.7);
  animation: pulse 2s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% { transform: scale(1); opacity: 1; }
  50% { transform: scale(1.3); opacity: 0.7; }
}

.hero-title {
  font-family: $font-display;
  font-size: clamp(2.5rem, 5.4vw, 4.3rem);
  font-weight: 700;
  line-height: 1.05;
  letter-spacing: -0.035em;
  margin: 22px 0 22px;
}

.hero-lead {
  font-size: 1.08rem;
  line-height: 1.65;
  color: $color-muted;
  max-width: 500px;
  margin-bottom: 32px;
}

.hero-cta {
  display: flex;
  flex-wrap: wrap;
  gap: 14px;
  margin-bottom: 44px;
}

.hero-cta .btn {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  padding: 14px 26px;

  svg {
    width: 18px;
    height: 18px;
  }
}

.hero-metrics {
  display: flex;
  align-items: center;
  gap: 24px;
  padding: 0;
  margin: 0;

  li {
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  .divider {
    width: 1px;
    height: 36px;
    background: $color-hairline;
  }

  strong {
    font-family: $font-display;
    font-size: 1.5rem;
    font-weight: 700;
    letter-spacing: -0.02em;
    color: $color-ink;
  }

  span {
    font-size: 0.82rem;
    color: $color-faint;
    letter-spacing: 0.02em;
  }
}

.hero-visual {
  position: relative;
  height: 520px;

  @media (max-width: 980px) {
    height: 420px;
  }
}

.float-card {
  --lg-r: 22px;
  --lg-blur: 1.5px;
  position: absolute;
  padding: 24px 26px;
  animation: float 7s ease-in-out infinite;
}

.card-1 {
  top: 0;
  left: 0;
  width: 84%;
  z-index: 3;
  animation-delay: 0s;
}

.card-2 {
  top: 40%;
  right: 0;
  width: 70%;
  z-index: 2;
  animation-delay: -2.5s;
}

.card-3 {
  bottom: 0;
  left: 8%;
  width: 62%;
  z-index: 1;
  animation-delay: -4.5s;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.card-badge {
  padding: 4px 10px;
  border-radius: 999px;
  font-size: 0.7rem;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;

  &.live {
    background: rgba(239, 68, 68, 0.18);
    color: #fca5a5;
    box-shadow: inset 0 0 0 1px rgba(239, 68, 68, 0.4);

    &::before {
      content: "";
      display: inline-block;
      width: 6px;
      height: 6px;
      border-radius: 50%;
      background: #ef4444;
      margin-right: 6px;
      vertical-align: middle;
      animation: pulse 1.5s ease-in-out infinite;
    }
  }
}

.card-time {
  font-family: $font-display;
  font-size: 0.85rem;
  color: $color-muted;
  font-variant-numeric: tabular-nums;
}

.card-title {
  font-family: $font-display;
  font-size: 1.05rem;
  font-weight: 600;
  margin-bottom: 6px;
  line-height: 1.25;
}

.card-meta {
  font-size: 0.78rem;
  color: $color-faint;
}

.card-progress {
  display: flex;
  align-items: center;
  gap: 14px;
}

.card-progress-ring {
  position: relative;
  width: 56px;
  height: 56px;
  flex-shrink: 0;

  svg {
    transform: rotate(-90deg);
  }

  span {
    position: absolute;
    inset: 0;
    display: grid;
    place-items: center;
    font-family: $font-display;
    font-weight: 700;
    font-size: 0.85rem;
  }
}

.card-eyebrow {
  font-size: 0.7rem;
  font-weight: 600;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: $color-faint;
  margin-bottom: 4px;
}

.card-title-sm {
  font-family: $font-display;
  font-size: 0.95rem;
  font-weight: 600;
  line-height: 1.2;
}

.card-meta-sm {
  font-size: 0.76rem;
  color: $color-faint;
  margin-top: 2px;
}

.card-cert {
  display: flex;
  align-items: center;
  gap: 12px;

  svg {
    width: 28px;
    height: 28px;
    flex-shrink: 0;
    color: $color-gold;
  }
}

@keyframes float {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-10px); }
}

.section-head {
  text-align: center;
  max-width: 640px;
  margin: 0 auto 48px;

  h2 {
    font-family: $font-display;
    font-size: clamp(1.8rem, 3.2vw, 2.4rem);
    font-weight: 600;
    line-height: 1.15;
    letter-spacing: -0.025em;
    margin: 16px 0 12px;
  }

  p {
    color: $color-muted;
    font-size: 1rem;
    line-height: 1.6;
  }
}

.paths {
  padding: 80px 0 60px;
}

.paths-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 18px;

  @media (max-width: 880px) {
    grid-template-columns: repeat(2, 1fr);
  }

  @media (max-width: 540px) {
    grid-template-columns: 1fr;
  }
}

.path-card {
  --lg-r: 22px;
  --lg-blur: 1.5px;
  padding: 26px 24px;
  cursor: pointer;
  transition: transform 0.35s, box-shadow 0.35s;

  &:hover {
    --lg-tint: rgba(255, 255, 255, 0.05);
    transform: translateY(-4px);
    box-shadow: 0 24px 50px rgba(3, 6, 24, 0.45), 0 4px 12px rgba(3, 6, 24, 0.25);
  }

  h3 {
    font-family: $font-display;
    font-size: 1.08rem;
    font-weight: 600;
    margin: 18px 0 8px;
    letter-spacing: -0.01em;
  }

  p {
    font-size: 0.88rem;
    color: $color-muted;
    line-height: 1.55;
    margin-bottom: 18px;
  }
}

.path-icon {
  display: grid;
  place-items: center;
  width: 46px;
  height: 46px;
  border-radius: 14px;
  background: color-mix(in srgb, var(--accent, #f59e0b) 18%, transparent);
  box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.3), inset 0 0 0 1px color-mix(in srgb, var(--accent, #f59e0b) 30%, transparent);
  color: var(--accent, #f59e0b);

  svg {
    width: 22px;
    height: 22px;
  }
}

.path-meta {
  display: flex;
  gap: 16px;
  font-size: 0.78rem;
  color: $color-faint;
  font-weight: 500;
  letter-spacing: 0.02em;

  span {
    display: flex;
    align-items: center;
    gap: 6px;

    &::before {
      content: "";
      width: 4px;
      height: 4px;
      border-radius: 50%;
      background: $color-faint;
      opacity: 0.5;
    }

    &:first-child::before {
      display: none;
    }
  }
}

.features {
  padding: 60px 0 80px;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;

  @media (max-width: 980px) {
    grid-template-columns: repeat(2, 1fr);
  }

  @media (max-width: 540px) {
    grid-template-columns: 1fr;
  }
}

.feature-card {
  --lg-r: 22px;
  --lg-blur: 1.5px;
  padding: 28px 24px;
  display: flex;
  flex-direction: column;
  gap: 14px;
  transition: transform 0.35s, box-shadow 0.35s;

  &:hover {
    --lg-tint: rgba(255, 255, 255, 0.05);
    transform: translateY(-4px);
    box-shadow: 0 22px 46px rgba(3, 6, 24, 0.42), 0 4px 12px rgba(3, 6, 24, 0.25);
  }
}

.feature-num {
  font-family: $font-display;
  font-size: 0.78rem;
  font-weight: 600;
  letter-spacing: 0.18em;
  color: $color-faint;
}

.feature-card h3 {
  font-family: $font-display;
  font-size: 1.08rem;
  font-weight: 600;
  letter-spacing: -0.01em;
  line-height: 1.25;
}

.feature-card p {
  font-size: 0.88rem;
  color: $color-muted;
  line-height: 1.6;
}

.featured-section {
  padding: 40px 0 60px;

  .section-head {
    margin-bottom: 28px;

    h2 {
      font-size: 1.6rem;
      margin-top: 8px;
    }
  }
}

.featured-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 20px;

  @media (max-width: 980px) {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  @media (max-width: 540px) {
    grid-template-columns: 1fr;
  }
}

.featured-card {
  text-decoration: none;
  color: inherit;
  overflow: hidden;
  padding: 0;
  transition: transform 0.2s;

  &:hover {
    transform: translateY(-4px);
  }

  .thumb {
    height: 140px;
    background-size: cover;
    background-position: center;
    background-color: rgba(139, 92, 246, 0.15);
  }

  .info {
    padding: 16px;

    h3 {
      font-size: 1rem;
      margin-bottom: 6px;
    }

    p {
      font-size: 0.85rem;
      color: $color-muted;
    }
  }
}

.cta-section {
  padding: 30px 0 90px;
}

.cta-card {
  --lg-r: 32px;
  --lg-blur: 1.5px;
  padding: 50px 56px;
  display: flex;
  align-items: center;
  justify-content: flex-start;
  flex-wrap: wrap;
  gap: 32px;
  position: relative;
  overflow: hidden;

  @media (max-width: 760px) {
    flex-direction: column;
    align-items: flex-start;
    padding: 36px 32px;
  }
}

.cta-content {
  position: relative;
  z-index: 1;
  flex: 1 1 300px;
  max-width: 560px;

  h2 {
    font-family: $font-display;
    font-size: clamp(1.6rem, 2.6vw, 2rem);
    font-weight: 600;
    letter-spacing: -0.025em;
    margin-bottom: 10px;
  }

  p {
    color: $color-muted;
    font-size: 0.95rem;
    line-height: 1.55;
  }
}

.cta-actions {
  position: relative;
  z-index: 1;
  display: flex;
  gap: 12px;
  flex-shrink: 0;
  margin-left: auto;

  @media (max-width: 760px) {
    margin-left: 0;
    width: 100%;

    .btn {
      flex: 1;
      justify-content: center;
    }
  }
}

.home-faq {
  padding: 16px 0 72px;

  .section-head {
    margin-bottom: 28px;
  }
}

.faq-link {
  display: flex;
  width: fit-content;
  margin: 22px auto 0;
  color: $color-gold;
  font-weight: 600;
  text-decoration: none;

  &:hover {
    color: $color-ink;
  }
}

</style>
