<script setup lang="ts">
import { useLiquidGlass } from '@/shared/composables/useLiquidGlass'
import { useRealtime } from '@/shared/composables/useRealtime'
import BackToTop from '@/shared/components/ui/BackToTop.vue'

useLiquidGlass()
useRealtime()
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
    <BackToTop />
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
    radial-gradient(1100px 760px at 84% -10%, rgba(74, 52, 214, 0.5), transparent 62%),
    radial-gradient(950px 720px at -12% 34%, rgba(14, 122, 127, 0.45), transparent 60%),
    radial-gradient(900px 680px at 52% 116%, rgba(184, 68, 31, 0.42), transparent 64%),
    linear-gradient(180deg, #11131a, $color-bg 50%, #0f1012);

  &::before {
    content: "";
    position: absolute;
    inset: -50%;
    z-index: 1;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='180' height='180'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='.85' numOctaves='3' stitchTiles='stitch'/%3E%3CfeColorMatrix values='0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 0 0 0 .55 0'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    opacity: 0.08;
    animation: grain 1.2s steps(6) infinite;
    pointer-events: none;
  }

  &::after {
    content: "";
    position: absolute;
    inset: 0;
    background-image:
      linear-gradient(rgba(242, 239, 230, 0.07) 1px, transparent 1px),
      linear-gradient(90deg, rgba(242, 239, 230, 0.07) 1px, transparent 1px);
    background-size: 32px 32px;
    -webkit-mask-image: radial-gradient(140% 120% at 50% 0%, #000 25%, transparent 92%);
    mask-image: radial-gradient(140% 120% at 50% 0%, #000 25%, transparent 92%);
  }
}

.orb {
  position: fixed;
  z-index: -2;
  border-radius: 50%;
  filter: blur(80px);
  opacity: 0.55;
  animation: drift 26s ease-in-out infinite alternate;

  &-1 { width: 540px; height: 540px; background: #4a34d6; top: -160px; right: 4%; }
  &-2 { width: 480px; height: 480px; background: #0e7a7f; top: 30%; left: -180px; animation-delay: -8s; animation-duration: 30s; }
  &-3 { width: 440px; height: 440px; background: #b8441f; bottom: -180px; left: 34%; animation-delay: -16s; animation-duration: 34s; }
  &-4 { width: 260px; height: 260px; background: $color-accent; top: 58%; right: 12%; opacity: 0.18; animation-delay: -22s; animation-duration: 38s; }
}

@keyframes drift {
  from { transform: translate(0, 0) scale(1); }
  to   { transform: translate(-80px, 60px) scale(1.12); }
}

@keyframes grain {
  0%   { transform: translate(0, 0); }
  20%  { transform: translate(-3%, 2%); }
  40%  { transform: translate(2%, -4%); }
  60%  { transform: translate(-4%, -1%); }
  80%  { transform: translate(3%, 3%); }
  100% { transform: translate(0, 0); }
}
</style>
