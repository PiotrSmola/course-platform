<template>
  <ErrorPage
    code="501"
    title="Funkcja niedostępna"
    description="Ta funkcja nie jest jeszcze dostępna. Pracujemy nad nią — wróć wkrótce lub przejdź do katalogu kursów."
    primary-label="Strona główna"
    primary-to="Home"
    secondary-label="Przeglądaj kursy"
    secondary-to="Courses"
  />
</template>

<script setup lang="ts">
import ErrorPage from '@/shared/components/ui/ErrorPage.vue'
</script>
