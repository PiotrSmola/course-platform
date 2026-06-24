<template>
  <div class="instructor-dashboard">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Panel instruktora</span>
        <h2>Twoje kursy</h2>
      </div>

      <div class="actions">
        <router-link class="btn btn-primary" :to="{ name: 'NewCourse' }">
          + Nowy kurs
        </router-link>
      </div>

      <div class="courses-grid">
        <div class="course-card glass-card">
          <div class="course-placeholder">
            <span>Przykładowy kurs</span>
          </div>
          <div class="course-info">
            <h3>Introduction to Vue 3</h3>
            <div class="course-stats">
              <span>12 studentów</span>
              <span class="status published">Opublikowany</span>
            </div>
            <router-link class="btn btn-ghost" :to="{ name: 'EditCourse', params: { id: 'sample' } }">
              Edytuj
            </router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.instructor-dashboard {
  padding: calc($header-height + 40px) 0 80px;
}

.actions {
  margin-bottom: 40px;
}

.courses-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 24px;
}

.course-card {
  padding: 0;
  overflow: hidden;
}

.course-placeholder {
  height: 180px;
  display: grid;
  place-items: center;
  background: linear-gradient(160deg, rgba(139, 92, 246, 0.2), rgba(34, 211, 238, 0.1));

  span {
    font-size: 1.1rem;
    font-weight: 600;
  }
}

.course-info {
  padding: 24px;

  h3 {
    font-size: 1.15rem;
    margin-bottom: 12px;
  }
}

.course-stats {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  font-size: 0.85rem;
  color: $color-muted;

  .status {
    padding: 4px 12px;
    border-radius: 999px;
    font-size: 0.78rem;
    font-weight: 600;

    &.published {
      background: rgba(16, 185, 129, 0.2);
      color: #6ee7b7;
    }
  }
}

.btn-ghost {
  padding: 10px 20px;
  font-size: 0.9rem;
}
</style>
