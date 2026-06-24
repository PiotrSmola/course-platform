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
            <router-link class="btn btn-ghost" :to="{ name: 'Register' }" v-if="!authStore.isAuthenticated">
              Wypróbuj za darmo
            </router-link>
          </div>

          <ul class="hero-metrics">
            <li>
              <strong>240+</strong>
              <span>kursów</span>
            </li>
            <li class="divider"></li>
            <li>
              <strong>18k</strong>
              <span>studentów</span>
            </li>
            <li class="divider"></li>
            <li>
              <strong>4.8</strong>
              <span>średnia ocen</span>
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
</template>

<script setup lang="ts">
import { useAuthStore } from '@/features/auth/stores/auth.store'

const authStore = useAuthStore()
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
  height: 460px;

  @media (max-width: 980px) {
    height: 380px;
  }
}

.float-card {
  --lg-r: 22px;
  --lg-blur: 0px;
  position: absolute;
  padding: 18px 20px;
  animation: float 7s ease-in-out infinite;
}

.card-1 {
  top: 0;
  left: 0;
  width: 78%;
  z-index: 3;
  animation-delay: 0s;
}

.card-2 {
  top: 40%;
  right: 0;
  width: 64%;
  z-index: 2;
  animation-delay: -2.5s;
}

.card-3 {
  bottom: 0;
  left: 8%;
  width: 56%;
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
</style>
