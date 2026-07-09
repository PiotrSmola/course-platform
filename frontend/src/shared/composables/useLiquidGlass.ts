import { onMounted, onUnmounted } from 'vue'

const SVG_NS = 'http://www.w3.org/2000/svg'
const XLINK_NS = 'http://www.w3.org/1999/xlink'
const LENS_MARGIN = 12
const LENS_TARGETS = [
  '.glass',
  '.glass-card',
  '.btn',
  '.eyebrow',
  '.plan-cta',
  '.sort-trigger',
  '.sort-menu',
  '.page-btn',
  '.search-box',
  '.search-btn',
  '.file-upload-btn',
  '.select-trigger',
  '.select-menu',
  '.mega-dropdown',
  '.search-trigger',
  '.search-dropdown',
  '.status-banner',
  '.Toastify__toast'
]
const LENS_SELECTOR = LENS_TARGETS.map((target) => `${target}:not(.no-warp)`).join(', ')
const REBUILD_DEBOUNCE_MS = 150

interface LensRecord {
  filter: SVGFilterElement
  image: SVGFEImageElement
  bend: SVGFEDisplacementMapElement
  w: number
  h: number
}

function smoothstep(a: number, b: number, x: number): number {
  const t = Math.min(1, Math.max(0, (x - a) / (b - a)))
  return t * t * (3 - 2 * t)
}

function makeLensMap(w: number, h: number): string {
  const W = w + LENS_MARGIN * 2
  const H = h + LENS_MARGIN * 2
  const ds = 2
  const cw = Math.max(2, Math.round(W / ds))
  const ch = Math.max(2, Math.round(H / ds))
  const canvas = document.createElement('canvas')
  canvas.width = cw
  canvas.height = ch
  const ctx = canvas.getContext('2d')
  if (!ctx) return ''
  const img = ctx.createImageData(cw, ch)
  const px = img.data
  let i = 0
  for (let cy = 0; cy < ch; cy++) {
    const y = cy * ds + ds / 2 - LENS_MARGIN
    const ny = (y / h) * 2 - 1
    for (let cx = 0; cx < cw; cx++) {
      const x = cx * ds + ds / 2 - LENS_MARGIN
      const nx = (x / w) * 2 - 1
      let r = 128
      let g = 128
      if (nx >= -1 && nx <= 1 && ny >= -1 && ny <= 1) {
        const ax = Math.abs(nx)
        const ay = Math.abs(ny)
        const d = Math.pow(ax ** 3 + ay ** 3, 1 / 3)
        const t = smoothstep(0.42, 1.12, d)
        const wave = Math.sin(t * Math.PI * 0.5) ** 2
        if (wave > 0) {
          const gx = nx * ax
          const gy = ny * ay
          const len = Math.hypot(gx, gy) || 1
          r -= (gx / len) * wave * 110
          g -= (gy / len) * wave * 110
        }
      }
      px[i++] = r
      px[i++] = g
      px[i++] = 128
      px[i++] = 255
    }
  }
  ctx.putImageData(img, 0, 0)
  return canvas.toDataURL()
}

function fe<T extends SVGElement>(name: string, attrs: Record<string, string | number>): T {
  const el = document.createElementNS(SVG_NS, name) as T
  for (const [key, value] of Object.entries(attrs)) {
    el.setAttribute(key, String(value))
  }
  return el
}

export function useLiquidGlass() {
  let defs: SVGSVGElement | null = null
  let resizeObserver: ResizeObserver | null = null
  let mutationObserver: MutationObserver | null = null
  let rebuildTimer: number | undefined
  let lensCounter = 0
  const registry = new Map<HTMLElement, LensRecord>()
  const rebuildQueue = new Set<HTMLElement>()

  const buildLens = (el: HTMLElement) => {
    if (!defs || !el.isConnected) return
    const w = el.offsetWidth
    const h = el.offsetHeight
    if (w < 24 || h < 24) return
    let rec = registry.get(el)
    if (rec && Math.abs(rec.w - w) < 2 && Math.abs(rec.h - h) < 2) return

    if (!rec) {
      const id = `lens-${lensCounter++}`
      const filter = fe<SVGFilterElement>('filter', {
        id,
        x: '-5%',
        y: '-5%',
        width: '110%',
        height: '110%',
        'color-interpolation-filters': 'sRGB'
      })
      const image = fe<SVGFEImageElement>('feImage', {
        x: 0,
        y: 0,
        result: 'map',
        preserveAspectRatio: 'none'
      })
      const smooth = fe<SVGFEGaussianBlurElement>('feGaussianBlur', {
        in: 'map',
        stdDeviation: 2.4,
        result: 'smap'
      })
      const bend = fe<SVGFEDisplacementMapElement>('feDisplacementMap', {
        in: 'SourceGraphic',
        in2: 'smap',
        xChannelSelector: 'R',
        yChannelSelector: 'G'
      })
      filter.append(image, smooth, bend)
      defs.appendChild(filter)
      rec = { filter, image, bend, w: 0, h: 0 }
      registry.set(el, rec)
      el.style.setProperty('--lens', `url(#${id})`)
    }

    rec.w = w
    rec.h = h
    rec.image.setAttribute('width', String(w + LENS_MARGIN * 2))
    rec.image.setAttribute('height', String(h + LENS_MARGIN * 2))
    const url = makeLensMap(w, h)
    rec.image.setAttribute('href', url)
    rec.image.setAttributeNS(XLINK_NS, 'xlink:href', url)
    rec.bend.setAttribute('scale', String(Math.min(26, Math.min(w, h) * 0.28)))
  }

  const scheduleRebuild = () => {
    window.clearTimeout(rebuildTimer)
    rebuildTimer = window.setTimeout(() => {
      rebuildQueue.forEach(buildLens)
      rebuildQueue.clear()
    }, REBUILD_DEBOUNCE_MS)
  }

  const attach = (el: HTMLElement) => {
    resizeObserver?.observe(el)
  }

  const detach = (el: HTMLElement) => {
    resizeObserver?.unobserve(el)
    rebuildQueue.delete(el)
    const rec = registry.get(el)
    if (rec) {
      rec.filter.remove()
      registry.delete(el)
    }
  }

  const scanAdded = (root: HTMLElement) => {
    if (root.matches?.(LENS_SELECTOR)) attach(root)
    root.querySelectorAll?.<HTMLElement>(LENS_SELECTOR).forEach(attach)
  }

  const scanRemoved = (root: HTMLElement) => {
    for (const el of [...registry.keys()]) {
      if (root === el || root.contains(el)) detach(el)
    }
  }

  const onSplash = (e: PointerEvent) => {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return
    const target = e.target as Element | null
    const btn = target?.closest?.('.btn')
    if (!(btn instanceof HTMLElement)) return
    const rect = btn.getBoundingClientRect()
    const drop = document.createElement('span')
    drop.className = 'splash'
    drop.style.left = `${e.clientX - rect.left}px`
    drop.style.top = `${e.clientY - rect.top}px`
    btn.appendChild(drop)
    drop.addEventListener('animationend', () => drop.remove())
  }

  onMounted(() => {
    defs = fe<SVGSVGElement>('svg', { width: 0, height: 0, 'aria-hidden': 'true', focusable: 'false' })
    defs.classList.add('lg-defs')
    defs.innerHTML = `
      <filter id="liquid-lens" x="-35%" y="-35%" width="170%" height="170%" color-interpolation-filters="sRGB">
        <feTurbulence type="fractalNoise" baseFrequency="0.004 0.008" numOctaves="2" seed="14" result="noise"/>
        <feGaussianBlur in="noise" stdDeviation="4" result="soft"/>
        <feDisplacementMap in="SourceGraphic" in2="soft" scale="70" xChannelSelector="R" yChannelSelector="G"/>
      </filter>`
    document.body.appendChild(defs)

    resizeObserver = new ResizeObserver((entries) => {
      entries.forEach((entry) => rebuildQueue.add(entry.target as HTMLElement))
      scheduleRebuild()
    })

    mutationObserver = new MutationObserver((mutations) => {
      for (const mutation of mutations) {
        mutation.addedNodes.forEach((node) => {
          if (node instanceof HTMLElement) scanAdded(node)
        })
        mutation.removedNodes.forEach((node) => {
          if (node instanceof HTMLElement) scanRemoved(node)
        })
      }
    })

    scanAdded(document.body)
    mutationObserver.observe(document.body, { childList: true, subtree: true })
    document.addEventListener('pointerdown', onSplash)
  })

  onUnmounted(() => {
    document.removeEventListener('pointerdown', onSplash)
    mutationObserver?.disconnect()
    resizeObserver?.disconnect()
    window.clearTimeout(rebuildTimer)
    registry.clear()
    defs?.remove()
    defs = null
  })
}
