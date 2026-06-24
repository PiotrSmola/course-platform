<template>
  <div class="auth-layout">
    <div class="auth-container">
      <router-view />
    </div>
  </div>
</template>

<style lang="scss" scoped>
.auth-layout {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  background: #1a1a2e;
}

.auth-container {
  width: 100%;
  max-width: 480px;
  padding: 2rem;
}
</style>
