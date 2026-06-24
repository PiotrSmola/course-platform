<template>
  <div class="auth-layout">
    <div class="auth-container">
      <div class="auth-card glass">
        <div class="auth-header">
          <svg class="logo-mark" viewBox="0 0 64 64" aria-hidden="true">
            <defs>
              <linearGradient id="logo-g" x1="0" y1="0" x2="1" y2="1">
                <stop offset="0" stop-color="#8b5cf6"/>
                <stop offset="1" stop-color="#22d3ee"/>
              </linearGradient>
            </defs>
            <rect x="14" y="6" width="36" height="52" rx="10" fill="url(#logo-g)"/>
            <rect x="26" y="11" width="12" height="3" rx="1.5" fill="rgba(255,255,255,.85)"/>
          </svg>
          <h1>CoursePlatform</h1>
        </div>
        <router-view />
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.auth-layout {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  padding: 2rem;
}

.auth-container {
  width: 100%;
  max-width: 440px;
}

.auth-card {
  --lg-r: 36px;
  --lg-blur: 1.5px;
  padding: 48px 40px;
}

.auth-header {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  margin-bottom: 40px;

  .logo-mark {
    width: 48px;
    height: 48px;
  }

  h1 {
    font-size: 1.5rem;
    font-weight: 700;
    letter-spacing: 0.02em;
  }
}
</style>