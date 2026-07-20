<template>
  <ErrorPage
    code="403"
    title="Brak dostępu"
    description="Nie masz uprawnień do wyświetlenia tej strony. Jeśli uważasz, że to błąd, skontaktuj się z administratorem."
    primary-label="Strona główna"
    primary-to="Home"
    secondary-label="Przeglądaj kursy"
    secondary-to="Courses"
  />
</template>

<script setup lang="ts">
import ErrorPage from '@/shared/components/ui/ErrorPage.vue'
</script>
