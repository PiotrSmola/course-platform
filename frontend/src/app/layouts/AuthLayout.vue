<template>
  <div class="auth-layout">
    <div class="auth-container">
      <div class="auth-card glass">
        <div class="auth-header">
          <router-link class="auth-wordmark" :to="{ name: 'Home' }" aria-label="CoursePlatform — strona główna">
            Course<em>Platform</em>
          </router-link>
          <span class="auth-header__label">Konto</span>
        </div>
        <router-view />
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.auth-layout {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  padding: 2rem 1rem;
}

.auth-container {
  width: 100%;
  max-width: 460px;
}

.auth-card {
  --lg-r: #{$radius-card};
  --lg-blur: 10px;
  --lg-sat: 150%;
  --lg-tint: rgba(13, 15, 19, 0.3);
  --lg-glow: 0.06;
  padding: 32px 40px 36px;

  @media (max-width: 480px) {
    padding: 26px 22px 28px;
  }
}

.auth-header {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 32px;
  padding-bottom: 20px;
  border-bottom: 1px solid $color-hairline;
}

.auth-wordmark {
  font-family: $font-display;
  font-size: 1.3rem;
  font-weight: 600;
  letter-spacing: -0.01em;
  line-height: 1;
  color: $color-ink;
  text-decoration: none;

  em {
    font-style: normal;
    color: $color-muted;
  }
}

.auth-header__label {
  @include mono-label;
  font-size: 0.62rem;
  color: $color-faint;
}
</style>
