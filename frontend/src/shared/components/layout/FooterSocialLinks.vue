<template>
  <div class="social-links" aria-label="CoursePlatform w mediach społecznościowych">
    <a href="https://www.facebook.com/" target="_blank" rel="noreferrer"><span>Facebook</span><span class="arrow" aria-hidden="true">↗</span></a>
    <a href="https://www.instagram.com/" target="_blank" rel="noreferrer"><span>Instagram</span><span class="arrow" aria-hidden="true">↗</span></a>
    <a href="https://x.com/" target="_blank" rel="noreferrer"><span>X</span><span class="arrow" aria-hidden="true">↗</span></a>
    <a href="https://www.youtube.com/" target="_blank" rel="noreferrer"><span>YouTube</span><span class="arrow" aria-hidden="true">↗</span></a>
  </div>
</template>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.social-links {
  display: flex;
  flex-direction: column;
  gap: 11px;
}

a {
  display: inline-flex;
  align-items: baseline;
  gap: 8px;
  width: fit-content;
  color: $color-muted;
  font-size: 0.93rem;
  text-decoration: none;
  transition: color 160ms ease;

  &:hover {
    color: $color-ink;

    .arrow {
      color: $color-accent;
      transform: translate(2px, -2px);
    }
  }
}

.arrow {
  display: inline-block;
  font-family: $font-mono;
  font-size: 0.78rem;
  color: $color-faint;
  transition: color 160ms ease, transform 200ms $ease-out;
}
</style>
