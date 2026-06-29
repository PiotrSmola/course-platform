<template>
  <div class="admin-dashboard">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Panel administratora</span>
        <h2>Zarządzanie platformą</h2>
        <p class="lead">Moderacja treści, użytkownicy i role — funkcje administracyjne Etapu 1.</p>
      </div>

      <div class="cards-grid">
        <article class="glass-card admin-card">
          <h3>Użytkownicy</h3>
          <p>Przegląd kont, blokowanie użytkowników i nadawanie ról.</p>
          <span class="badge">Wkrótce</span>
        </article>
        <article class="glass-card admin-card">
          <h3>Moderacja kursów</h3>
          <p>Przegląd zgłoszeń, ukrywanie kursów i weryfikacja publikacji.</p>
          <span class="badge">Wkrótce</span>
        </article>
        <article class="glass-card admin-card">
          <h3>Recenzje</h3>
          <p>Moderacja opinii i usuwanie nieodpowiednich treści.</p>
          <span class="badge">Wkrótce</span>
        </article>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.admin-dashboard {
  padding: calc($header-height + 40px) 0 80px;
}

.section-head {
  margin-bottom: 40px;

  .lead {
    margin-top: 12px;
    color: $color-muted;
    max-width: 52ch;
  }
}

.cards-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 24px;
}

.admin-card {
  padding: 28px;
  display: flex;
  flex-direction: column;
  gap: 12px;

  h3 {
    font-size: 1.1rem;
  }

  p {
    color: $color-muted;
    font-size: 0.92rem;
    flex: 1;
  }
}

.badge {
  align-self: flex-start;
  padding: 6px 12px;
  border-radius: 999px;
  font-size: 0.78rem;
  font-weight: 600;
  color: $color-gold;
  background: rgba(245, 158, 11, 0.12);
}
</style>
