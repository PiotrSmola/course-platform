<template>
  <div>
    <!-- Hero Section -->
    <section class="hero" id="start">
      <div class="container hero-grid">
        <div class="hero-copy">
          <span class="eyebrow">✦ Rozwijaj swoje umiejętności</span>
          <h1>Ucz się od<br>najlepszych <span class="grad-text">ekspertów</span></h1>
          <p class="lead">
            Kursy online z zakresu programowania, designu i biznesu. 
            Ucz się w swoim tempie, zdobywaj certyfikaty i rozwijaj karierę.
          </p>
          <div class="hero-cta">
            <router-link class="btn btn-primary" :to="{ name: 'Courses' }">Przeglądaj kursy</router-link>
            <router-link class="btn btn-ghost" :to="{ name: 'Register' }" v-if="!authStore.isAuthenticated">Dołącz za darmo</router-link>
          </div>
          <ul class="hero-stats">
            <li class="glass"><strong>50+</strong> kursów</li>
            <li class="glass"><strong>10k+</strong> studentów</li>
            <li class="glass"><strong>4.9/5</strong> ocena</li>
          </ul>
        </div>

        <div class="hero-visual" aria-hidden="true">
          <div class="glow"></div>
          <div class="glass-card hero-card">
            <div class="card-content">
              <div class="card-icon">🎓</div>
              <div class="card-title">Vue 3 Mastery</div>
              <div class="card-progress">
                <div class="progress-bar">
                  <div class="progress-fill" style="width: 65%"></div>
                </div>
                <span>65% ukończone</span>
              </div>
            </div>
          </div>
          <div class="glass-card hero-card-2">
            <div class="card-content">
              <div class="card-icon">⚡</div>
              <div class="card-title">Nowy kurs</div>
              <div class="card-badge">Dostępny</div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Courses Section -->
    <section class="products" id="courses">
      <div class="container">
        <div class="section-head">
          <span class="eyebrow">Katalog</span>
          <h2>Dostępne kursy</h2>
          <p>Wybierz kurs i rozpocznij naukę już dziś.</p>
        </div>

        <div v-if="isLoading" class="loading">Ładowanie kursów...</div>
        <div v-else-if="isError" class="error">Błąd: {{ error?.message }}</div>
        <div v-else-if="data" class="products-grid">
          <CourseCard v-for="course in data.items" :key="course.id" :course="course" />
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useCourses } from '@/features/courses/composables/useCourses'
import CourseCard from '@/features/courses/components/CourseCard.vue'

const authStore = useAuthStore()
const { isLoading, isError, error, data } = useCourses()
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.hero {
  min-height: 100vh;
  display: flex;
  align-items: center;
  padding: calc($header-height + 70px) 0 80px;

  @media (max-width: 880px) {
    padding-top: calc($header-height + 50px);
    min-height: 0;
  }
}

.hero-grid {
  display: grid;
  grid-template-columns: 1.05fr 0.95fr;
  align-items: center;
  gap: 60px;

  @media (max-width: 880px) {
    grid-template-columns: 1fr;
    gap: 40px;
  }
}

.hero h1 {
  font-size: clamp(2.5rem, 5.4vw, 4.3rem);
  line-height: 1.06;
  letter-spacing: -0.02em;
  margin: 26px 0 20px;
  text-shadow: 0 2px 24px rgba(3, 6, 24, 0.45);
}

.lead {
  font-size: 1.08rem;
  color: $color-muted;
  max-width: 480px;
}

.hero-cta {
  display: flex;
  flex-wrap: wrap;
  gap: 14px;
  margin: 32px 0 40px;
}

.hero-stats {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;

  li {
    --lg-r: 16px;
    --lg-blur: 0px;
    padding: 10px 18px;
    font-size: 0.88rem;
    color: $color-muted;
    box-shadow: 0 6px 18px rgba(3, 6, 24, 0.3);

    strong {
      color: $color-ink;
      margin-right: 6px;
      font-size: 1rem;
    }
  }
}

.hero-visual {
  position: relative;
  display: flex;
  justify-content: center;
  min-height: 400px;
}

.glow {
  position: absolute;
  inset: 8% 16%;
  background: radial-gradient(closest-side, rgba(139, 92, 246, 0.55), rgba(34, 211, 238, 0.2), transparent);
  filter: blur(48px);
  z-index: -1;
}

.hero-card,
.hero-card-2 {
  position: absolute;
  width: 280px;
  padding: 24px;
  animation: float 7s ease-in-out infinite;
}

.hero-card {
  top: 10%;
  left: 10%;
}

.hero-card-2 {
  top: 40%;
  right: 5%;
  animation-delay: -3.5s;
  animation-duration: 9s;
}

.card-content {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.card-icon {
  font-size: 2rem;
}

.card-title {
  font-size: 1.15rem;
  font-weight: 600;
}

.card-progress {
  display: flex;
  flex-direction: column;
  gap: 6px;

  span {
    font-size: 0.82rem;
    color: $color-muted;
  }
}

.progress-bar {
  height: 6px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.1);
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, #a78bfa, #22d3ee);
}

.card-badge {
  display: inline-block;
  padding: 4px 12px;
  border-radius: 999px;
  font-size: 0.78rem;
  font-weight: 600;
  background: rgba(16, 185, 129, 0.25);
  color: #6ee7b7;
  box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.35);
}

@keyframes float {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-16px); }
}

.products {
  padding: 110px 0;
}

.products-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(290px, 1fr));
  gap: 26px;
}

.loading,
.error {
  text-align: center;
  padding: 60px;
  font-size: 1.1rem;
  color: $color-muted;
}
</style>
