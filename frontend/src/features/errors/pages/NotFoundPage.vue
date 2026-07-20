<template>
  <ErrorPage
    code="404"
    title="Strona nie istnieje"
    description="Adres, którego szukasz, nie istnieje lub został przeniesiony. Sprawdź link albo wróć do katalogu kursów."
    primary-label="Strona główna"
    primary-to="Home"
    secondary-label="Przeglądaj kursy"
    secondary-to="Courses"
  />
</template>

<script setup lang="ts">
import ErrorPage from '@/shared/components/ui/ErrorPage.vue'
</script>
