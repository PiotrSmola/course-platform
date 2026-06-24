<template>
  <div class="layout">
    <AppHeader />
    <div class="dashboard">
      <AppSidebar />
      <main>
        <router-view />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import AppHeader from '@/shared/components/layout/AppHeader.vue'
import AppSidebar from '@/shared/components/layout/AppSidebar.vue'
</script>

<style lang="scss" scoped>
.layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.dashboard {
  display: flex;
  flex: 1;
}

main {
  flex: 1;
  padding: 2rem;
}
</style>
