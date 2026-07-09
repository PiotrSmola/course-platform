<script setup lang="ts">
import { useLiquidGlass } from '@/shared/composables/useLiquidGlass'

useLiquidGlass()
</script>

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
    radial-gradient(1100px 800px at 78% -8%, rgba(245, 158, 11, 0.45), transparent 62%),
    radial-gradient(950px 720px at -10% 30%, rgba(34, 211, 238, 0.35), transparent 60%),
    radial-gradient(900px 700px at 55% 112%, rgba(236, 72, 153, 0.38), transparent 64%),
    linear-gradient(180deg, #0f172a, $color-bg 48%, #0c1a2e);

  &::after {
    content: "";
    position: absolute;
    inset: 0;
    background-image: radial-gradient(rgba(255, 255, 255, 0.12) 1px, transparent 1.6px);
    background-size: 26px 26px;
    opacity: 0.45;
    -webkit-mask-image: radial-gradient(130% 120% at 50% 0%, #000 30%, transparent 95%);
    mask-image: radial-gradient(130% 120% at 50% 0%, #000 30%, transparent 95%);
  }
}

.orb {
  position: fixed;
  z-index: -2;
  border-radius: 50%;
  filter: blur(75px);
  opacity: 0.6;
  animation: drift 26s ease-in-out infinite alternate;

  &-1 { width: 520px; height: 520px; background: #f59e0b; top: -140px; left: -100px; }
  &-2 { width: 460px; height: 460px; background: #22d3ee; top: 28%; right: -160px; animation-delay: -8s; animation-duration: 30s; }
  &-3 { width: 420px; height: 420px; background: #ec4899; bottom: -160px; left: 22%; animation-delay: -16s; animation-duration: 34s; }
  &-4 { width: 280px; height: 280px; background: #14b8a6; top: 54%; left: 46%; opacity: 0.3; animation-delay: -22s; animation-duration: 38s; }
}

@keyframes drift {
  from { transform: translate(0, 0) scale(1); }
  to   { transform: translate(90px, 60px) scale(1.15); }
}
</style>
