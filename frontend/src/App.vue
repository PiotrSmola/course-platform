<template>
  <div class="app-wrapper">
    <div class="bg" aria-hidden="true">
      <div class="orb orb-1"></div>
      <div class="orb orb-2"></div>
      <div class="orb orb-3"></div>
      <div class="orb orb-4"></div>
    </div>
    <router-view />
  </div>
</template>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.app-wrapper {
  position: relative;
  min-height: 100vh;
}

.bg {
  position: fixed;
  inset: 0;
  z-index: -3;
  background:
    radial-gradient(1150px 800px at 82% -12%, rgba(124, 58, 237, 0.55), transparent 62%),
    radial-gradient(1000px 760px at -12% 32%, rgba(8, 145, 178, 0.5), transparent 60%),
    radial-gradient(950px 720px at 52% 118%, rgba(190, 24, 93, 0.45), transparent 64%),
  linear-gradient(180deg, #131a44, #0d1230 48%, #181244);

  &::after {
    content: "";
    position: absolute;
    inset: 0;
    background-image: radial-gradient(rgba(255, 255, 255, 0.16) 1px, transparent 1.6px);
    background-size: 26px 26px;
    opacity: 0.55;
    -webkit-mask-image: radial-gradient(130% 120% at 50% 0%, #000 30%, transparent 95%);
    mask-image: radial-gradient(130% 120% at 50% 0%, #000 30%, transparent 95%);
  }
}

.orb {
  position: fixed;
  z-index: -2;
  border-radius: 50%;
  filter: blur(75px);
  opacity: 0.65;
  animation: drift 26s ease-in-out infinite alternate;

  &-1 { width: 540px; height: 540px; background: #7c3aed; top: -150px; left: -110px; }
  &-2 { width: 480px; height: 480px; background: #0ea5e9; top: 30%; right: -170px; animation-delay: -8s; animation-duration: 30s; }
  &-3 { width: 440px; height: 440px; background: #db2777; bottom: -170px; left: 20%; animation-delay: -16s; animation-duration: 34s; }
  &-4 { width: 300px; height: 300px; background: #f59e0b; top: 56%; left: 44%; opacity: 0.35; animation-delay: -22s; animation-duration: 38s; }
}

@keyframes drift {
  from { transform: translate(0, 0) scale(1); }
  to   { transform: translate(90px, 60px) scale(1.15); }
}
</style>
