<template>
  <section class="hero" id="start">
    <div class="container hero-grid">
      <div class="hero-copy">
        <span class="eyebrow">Sezon jesienny 2026</span>

        <h1 class="hero-title">
          Naucz się rzeczy,<br>
          <em>które mają sens.</em>
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

        <dl class="hero-ledger">
          <div>
            <dt>Kursy</dt>
            <dd>{{ stats?.totalCount ?? '—' }}</dd>
          </div>
          <div>
            <dt>Średnia ocen</dt>
            <dd>{{ featuredRating }}</dd>
          </div>
          <div>
            <dt>Ścieżki nauki</dt>
            <dd>{{ paths.length || '—' }}</dd>
          </div>
        </dl>
      </div>

      <div class="hero-stage" aria-hidden="true">
        <div
          v-for="(course, index) in stageCourses"
          :key="course.id"
          class="stage-cover"
          :class="`stage-cover--${index + 1}`"
        >
          <CourseThumbnail
            class="stage-thumb"
            :url="course.thumbnailUrl"
            :title="course.title"
            :label="course.technologyNames?.[0] ?? course.categoryNames?.[0]"
            :seed="course.id"
          />
        </div>

        <div v-if="spotlight" class="stage-lens glass">
          <span class="lens-label">Najczęściej wybierany</span>
          <p class="lens-title">{{ spotlight.title }}</p>
          <p class="lens-meta">
            <span>★ {{ spotlight.averageRating.toFixed(1) }}</span>
            <span>{{ spotlight.lessonCount }} lekcji</span>
            <span>{{ spotlight.price }} zł</span>
          </p>
        </div>
      </div>
    </div>
  </section>

  <section v-if="paths.length" class="paths" id="sciezki">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Ścieżki nauki</span>
        <h2>Wybierz kierunek, <em>resztę ułożyliśmy.</em></h2>
        <p>Spójne programy złożone z kilku kursów — od fundamentu po zaawansowane tematy.</p>
      </div>

      <ol class="paths-list">
        <li v-for="(path, index) in paths.slice(0, 6)" :key="path.id">
          <router-link class="path-row" :to="{ name: 'PathDetails', params: { slug: path.slug } }">
            <span class="path-num">{{ String(index + 1).padStart(2, '0') }}</span>
            <span class="path-main">
              <span class="path-title">{{ path.title }}</span>
              <span class="path-desc">{{ path.shortDescription }}</span>
            </span>
            <span class="path-meta">
              <span>{{ difficultyLabel(path.difficultyLevel) }}</span>
              <span>{{ coursesLabel(path.courseCount) }}</span>
              <span>{{ path.estimatedHours }} h</span>
            </span>
            <svg class="path-arrow" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round">
              <path d="M5 12h14M13 5l7 7-7 7"/>
            </svg>
          </router-link>
        </li>
      </ol>
    </div>
  </section>

  <section class="features" id="dlaczego">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Dlaczego CoursePlatform</span>
        <h2>Ucz się inaczej <em>niż gdziekolwiek indziej.</em></h2>
      </div>

      <div class="features-grid">
        <article class="feature">
          <span class="feature-num">01</span>
          <h3>Realne projekty, nie slajdy</h3>
          <p>Każdy kurs kończy się projektem, który możesz dodać do portfolio lub pokazać rekruterowi.</p>
        </article>

        <article class="feature">
          <span class="feature-num">02</span>
          <h3>Mentorzy z branży</h3>
          <p>Instruktorzy pracują na co dzień w firmach produktowych — uczą tego, czego naprawdę używają w pracy.</p>
        </article>

        <article class="feature">
          <span class="feature-num">03</span>
          <h3>Pytania pod każdą lekcją</h3>
          <p>Zadajesz pytanie przy konkretnej lekcji, a instruktor odpowiada w tym samym miejscu.</p>
        </article>

        <article class="feature">
          <span class="feature-num">04</span>
          <h3>Certyfikat ukończenia</h3>
          <p>Po zakończeniu kursu otrzymujesz certyfikat z numerem, który każdy może zweryfikować online.</p>
        </article>
      </div>
    </div>
  </section>

  <section v-if="featuredCourses.length" class="featured-section">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Polecane</span>
        <h2>Popularne kursy</h2>
        <p>
          Najczęściej wybierane w tym miesiącu.
          <router-link class="inline-link" :to="{ name: 'Courses' }">Cały katalog →</router-link>
        </p>
      </div>
      <div class="featured-grid">
        <CourseBrowseCard v-for="course in featuredCourses" :key="course.id" :course="course" />
      </div>
    </div>
  </section>

  <section class="cta-section" id="start-learning">
    <div class="container">
      <div class="cta-card glass-card">
        <div class="cta-content">
          <span class="eyebrow">Na start</span>
          <h2>Pierwszy kurs <em>przejdziesz za darmo.</em></h2>
          <p>Wybierz kurs z katalogu i sprawdź, jak wygląda nauka u nas, zanim za cokolwiek zapłacisz.</p>
        </div>
        <div class="cta-actions">
          <router-link class="btn btn-primary" :to="trialDestination">Wypróbuj za darmo</router-link>
          <router-link class="btn btn-ghost" :to="{ name: 'Courses' }">Wybierz kurs</router-link>
        </div>
      </div>
    </div>
  </section>

  <section class="home-faq">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Pomoc</span>
        <h2>Najczęściej zadawane pytania</h2>
        <p>Krótkie odpowiedzi przed wyborem kursu.</p>
      </div>
      <FaqList :compact="true" />
      <router-link class="inline-link faq-link" :to="{ name: 'Faq' }">Zobacz wszystkie odpowiedzi →</router-link>
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
import CourseBrowseCard from '@/features/courses/components/CourseBrowseCard.vue'
import { usePopularCourses } from '@/features/courses/composables/usePopularCourses'
import { useLearningPaths } from '@/features/learning-paths/composables/useLearningPaths'
import { PathDifficultyLevel } from '@/features/learning-paths/api/learningPaths.api'
import FaqList from '@/features/public/components/FaqList.vue'

const authStore = useAuthStore()

const { data: stats } = useQuery({
  queryKey: queryKeys.catalogStats(),
  queryFn: () => getCourses({ pageSize: 1 })
})

const { data: featuredData } = usePopularCourses()
const { data: pathsData } = useLearningPaths()

const paths = computed(() => pathsData.value?.items ?? [])

const featuredCourses = computed(() => {
  const courses = featuredData.value ?? []
  if (courses.length >= 8) return courses.slice(0, 8)
  return courses.length >= 4 ? courses.slice(0, 4) : []
})

const stageCourses = computed(() => (featuredData.value ?? []).slice(0, 3))
const spotlight = computed(() => stageCourses.value[0] ?? null)

const trialDestination = computed(() =>
  authStore.isAuthenticated
    ? { name: 'Trial' }
    : { name: 'Register', query: { redirect: '/trial' } }
)

const featuredRating = computed(() => {
  const items = featuredData.value ?? []
  if (!items.length) return '—'
  const avg = items.reduce((sum, c) => sum + c.averageRating, 0) / items.length
  return avg.toFixed(1)
})

function difficultyLabel(level: PathDifficultyLevel): string {
  switch (level) {
    case PathDifficultyLevel.Beginner:
      return 'Początkujący'
    case PathDifficultyLevel.Intermediate:
      return 'Średni'
    case PathDifficultyLevel.Advanced:
      return 'Zaawansowany'
    default:
      return ''
  }
}

function coursesLabel(count: number): string {
  const lastTwo = count % 100
  const last = count % 10
  if (count === 1) return '1 kurs'
  if (last >= 2 && last <= 4 && (lastTwo < 12 || lastTwo > 14)) return `${count} kursy`
  return `${count} kursów`
}
</script>

<style lang="scss" scoped>
.hero {
  padding: 48px 0 96px;

  @media (max-width: 640px) {
    padding: 16px 0 56px;
  }
}

.hero-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.05fr) minmax(0, 0.95fr);
  align-items: center;
  gap: 56px;

  @media (max-width: 980px) {
    grid-template-columns: 1fr;
    gap: 40px;
  }
}

.hero-title {
  margin: 26px 0 24px;
  font-size: clamp(2.8rem, 5.8vw, 5rem);
  font-weight: 500;
  line-height: 1.06;
  letter-spacing: 0;

  em {
    display: inline-block;
    padding-right: 0.08em;
  }
}

.hero-lead {
  max-width: 520px;
  margin-bottom: 34px;
  font-size: 1.1rem;
  line-height: 1.65;
  color: $color-muted;
}

.hero-cta {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;

  .btn svg {
    width: 18px;
    height: 18px;
  }
}

.hero-ledger {
  display: grid;
  grid-template-columns: repeat(3, auto);
  justify-content: start;
  gap: 0;
  margin-top: 56px;
  border-top: 1px solid $color-hairline;

  > div {
    padding: 16px 32px 0 0;
    margin-right: 32px;
    border-right: 1px solid $color-hairline;

    &:last-child {
      border-right: 0;
      margin-right: 0;
    }
  }

  dt {
    @include mono-label;
    font-size: 0.64rem;
    color: $color-faint;
  }

  dd {
    margin-top: 6px;
    font-family: $font-display;
    font-size: 2.2rem;
    line-height: 1;
  }

  @media (max-width: 520px) {
    > div {
      padding-right: 16px;
      margin-right: 16px;
    }

    dd {
      font-size: 1.7rem;
    }
  }
}

.hero-stage {
  position: relative;
  height: 520px;

  @media (max-width: 980px) {
    height: 420px;
  }

  @media (max-width: 520px) {
    height: 320px;
  }
}

.stage-cover {
  position: absolute;
  width: 62%;
  border-radius: 16px 16px 16px 4px;
  box-shadow:
    0 30px 60px -20px rgba(0, 0, 0, 0.7),
    0 6px 14px rgba(0, 0, 0, 0.35);
  overflow: hidden;
  animation: settle 1.1s $ease-out backwards;

  &--1 {
    top: 2%;
    left: 4%;
    z-index: 3;
    transform: rotate(-5deg);
  }

  &--2 {
    top: 20%;
    right: 0;
    z-index: 2;
    transform: rotate(4deg);
    animation-delay: 0.08s;
  }

  &--3 {
    bottom: 6%;
    left: 14%;
    z-index: 1;
    transform: rotate(-1.5deg);
    animation-delay: 0.16s;
  }
}

@keyframes settle {
  from {
    translate: 0 40px;
    rotate: 6deg;
  }
}

.stage-thumb {
  position: relative;
  width: 100%;
  aspect-ratio: 4 / 3;
}

.stage-lens {
  --lg-r: 28px 28px 28px 8px;
  --lg-blur: 0px;
  --lg-tint: rgba(13, 15, 19, 0.12);
  position: absolute;
  z-index: 4;
  right: 4%;
  bottom: 14%;
  width: min(330px, 70%);
  padding: 22px 24px;
}

.lens-label {
  @include mono-label;
  font-size: 0.62rem;
  color: $color-accent;
}

.lens-title {
  margin: 10px 0 12px;
  font-family: $font-display;
  font-size: 1.35rem;
  line-height: 1.15;
  text-shadow: 0 1px 12px rgba(0, 0, 0, 0.45);
}

.lens-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 14px;
  font-family: $font-mono;
  font-size: 0.74rem;
  color: $color-ink;
}

.paths,
.features,
.featured-section,
.home-faq {
  padding: 72px 0;
}

.paths-list {
  list-style: none;
  border-top: 1px solid $color-hairline;
}

.path-row {
  display: grid;
  grid-template-columns: 72px minmax(0, 1fr) auto 28px;
  align-items: center;
  gap: 24px;
  padding: 24px 8px;
  color: inherit;
  text-decoration: none;
  border-bottom: 1px solid $color-hairline;
  transition: background 0.3s, padding 0.4s $ease-out;

  &:hover {
    padding-left: 18px;
    background: linear-gradient(90deg, rgba($color-ink, 0.05), transparent 70%);

    .path-arrow {
      color: $color-accent;
      transform: translateX(4px);
    }
  }

  @media (max-width: 760px) {
    grid-template-columns: 44px minmax(0, 1fr) 20px;

    .path-meta {
      grid-column: 2;
      grid-row: 2;
    }

    .path-arrow {
      grid-row: 1;
      grid-column: 3;
    }
  }
}

.path-num {
  font-family: $font-display;
  font-size: 2rem;
    color: $color-accent;
  line-height: 1;
}

.path-main {
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 0;
}

.path-title {
  font-family: $font-display;
  font-size: 1.5rem;
  line-height: 1.15;
}

.path-desc {
  color: $color-muted;
  font-size: 0.92rem;
}

.path-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;

  span {
    @include tag;
  }
}

.path-arrow {
  width: 24px;
  height: 24px;
  color: $color-faint;
  transition: transform 0.4s $ease-out, color 0.3s;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0;

  @media (max-width: 980px) {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  @media (max-width: 560px) {
    grid-template-columns: 1fr;
  }
}

.feature {
  padding: 8px 28px 28px 0;
  margin-right: 28px;
  border-right: 1px solid $color-hairline;

  &:last-child {
    border-right: 0;
  }

  @media (max-width: 980px) {
    &:nth-child(2n) {
      border-right: 0;
    }

    &:nth-child(-n + 2) {
      border-bottom: 1px solid $color-hairline;
      margin-bottom: 28px;
    }
  }

  @media (max-width: 560px) {
    border-right: 0;
    margin-right: 0;
    border-bottom: 1px solid $color-hairline;
    margin-bottom: 24px;
  }

  h3 {
    margin: 22px 0 10px;
    font-size: 1.35rem;
  }

  p {
    color: $color-muted;
    font-size: 0.94rem;
  }
}

.feature-num {
  font-family: $font-display;
    font-size: 3.4rem;
  line-height: 1;
  color: $color-accent;
}

.featured-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 20px;

  @media (max-width: 1180px) {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }

  @media (max-width: 860px) {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  @media (max-width: 560px) {
    grid-template-columns: 1fr;
  }
}

.inline-link {
  display: inline-block;
  margin-top: 6px;
  font-family: $font-mono;
  font-size: 0.78rem;
  letter-spacing: 0.06em;
  color: $color-accent;
  text-decoration: none;

  &:hover {
    text-decoration: underline;
    text-underline-offset: 4px;
  }
}

.cta-section {
  padding: 48px 0 72px;
}

.cta-card {
  --lg-r: 32px 32px 32px 8px;
  --lg-blur: 4px;
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  align-items: end;
  gap: 40px;
  padding: 56px;

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
    padding: 32px 24px;
  }

  &:hover {
    transform: none;
  }
}

.cta-content {
  h2 {
    margin: 18px 0 14px;
    font-size: clamp(2.2rem, 4.6vw, 3.6rem);
  }

  p {
    max-width: 520px;
    color: $color-muted;
  }
}

.cta-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.faq-link {
  margin-top: 28px;
}
</style>
