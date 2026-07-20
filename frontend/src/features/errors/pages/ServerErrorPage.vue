<template>
  <ErrorPage
    code="500"
    title="Coś poszło nie tak"
    description="Wystąpił nieoczekiwany błąd serwera. Spróbuj ponownie za chwilę lub wróć na stronę główną."
    primary-label="Strona główna"
    primary-to="Home"
    secondary-label="Przeglądaj kursy"
    secondary-to="Courses"
  />
</template>

<script setup lang="ts">
import ErrorPage from '@/shared/components/ui/ErrorPage.vue'
</script>
