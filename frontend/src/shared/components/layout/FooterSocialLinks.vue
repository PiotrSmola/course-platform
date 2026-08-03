<template>
  <div class="social-links" aria-label="CoursePlatform w mediach społecznościowych">
    <a href="https://www.facebook.com/" target="_blank" rel="noreferrer">Facebook</a>
    <a href="https://www.instagram.com/" target="_blank" rel="noreferrer">Instagram</a>
    <a href="https://x.com/" target="_blank" rel="noreferrer">X</a>
    <a href="https://www.youtube.com/" target="_blank" rel="noreferrer">YouTube</a>
  </div>
</template>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.social-links {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

a {
  display: inline-flex;
  min-height: 36px;
  align-items: center;
  justify-content: center;
  padding: 0 12px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.05);
  box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.26), inset 0 0 0 1px rgba(255, 255, 255, 0.1);
  color: $color-muted;
  font-size: 0.78rem;
  font-weight: 700;
  text-decoration: none;
  transition: color 160ms ease, background 160ms ease, transform 160ms ease;

  &:hover {
    transform: translateY(-2px);
    background: rgba(255, 255, 255, 0.1);
    color: $color-ink;
  }
}
</style>
