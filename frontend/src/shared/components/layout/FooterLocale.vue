<template>
  <span class="footer-locale" aria-label="Polska, waluta złoty polski">
    <svg viewBox="0 0 24 16" aria-hidden="true">
      <rect width="24" height="8" fill="#ffffff" />
      <rect y="8" width="24" height="8" fill="#dc143c" />
      <rect x="0.5" y="0.5" width="23" height="15" rx="2" fill="none" stroke="rgba(255,255,255,0.3)" />
    </svg>
    <span>PL · PLN</span>
  </span>
</template>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.footer-locale {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  color: $color-faint;
}

svg {
  width: 21px;
  height: 14px;
  overflow: hidden;
  border-radius: 3px;
  box-shadow: 0 1px 5px rgba(3, 6, 24, 0.35);
}
</style>
