import { onMounted, onUnmounted } from 'vue'

const SVG_NS = 'http://www.w3.org/2000/svg'
const XLINK_NS = 'http://www.w3.org/1999/xlink'
const LENS_MARGIN = 12
const LENS_TARGETS = [
  '.glass',
  '.glass-card',
  '.btn',
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
  '.search-dropdown',
  '.status-banner',
  '.Toastify__toast'
]
const LENS_SELECTOR = LENS_TARGETS.map((target) => `${target}:not(.no-warp)`).join(', ')
const LIGHT_SELECTOR = LENS_TARGETS.join(', ')
const REBUILD_DEBOUNCE_MS = 150
const CHROMA_SPREAD = 0.09

interface LensRecord {
  filter: SVGFilterElement
  image: SVGFEImageElement
  bends: SVGFEDisplacementMapElement[]
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
        const d = Math.pow(nx ** 4 + ny ** 4, 0.25)
        const t = smoothstep(0.62, 1, Math.min(d, 1))
        if (t > 0) {
          const gx = nx ** 3
          const gy = ny ** 3
          const len = Math.hypot(gx, gy) || 1
          r -= (gx / len) * t * 127
          g -= (gy / len) * t * 127
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

const CHANNELS = [
  { key: 'r', matrix: '1 0 0 0 0  0 0 0 0 0  0 0 0 0 0  0 0 0 1 0', spread: 1 + CHROMA_SPREAD },
  { key: 'g', matrix: '0 0 0 0 0  0 1 0 0 0  0 0 0 0 0  0 0 0 1 0', spread: 1 },
  { key: 'b', matrix: '0 0 0 0 0  0 0 0 0 0  0 0 1 0 0  0 0 0 1 0', spread: 1 - CHROMA_SPREAD }
]

export function useLiquidGlass() {
  let defs: SVGSVGElement | null = null
  let resizeObserver: ResizeObserver | null = null
  let mutationObserver: MutationObserver | null = null
  let rebuildTimer: number | undefined
  let lightFrame = 0
  let litElement: HTMLElement | null = null
  let lensCounter = 0
  const registry = new Map<HTMLElement, LensRecord>()
  const rebuildQueue = new Set<HTMLElement>()

  const createFilter = (id: string): LensRecord => {
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
      stdDeviation: 1,
      result: 'smap'
    })
    filter.append(image, smooth)

    const bends: SVGFEDisplacementMapElement[] = []
    for (const channel of CHANNELS) {
      const bend = fe<SVGFEDisplacementMapElement>('feDisplacementMap', {
        in: 'SourceGraphic',
        in2: 'smap',
        xChannelSelector: 'R',
        yChannelSelector: 'G',
        result: `bent-${channel.key}`
      })
      const isolate = fe<SVGFEColorMatrixElement>('feColorMatrix', {
        in: `bent-${channel.key}`,
        type: 'matrix',
        values: channel.matrix,
        result: `only-${channel.key}`
      })
      bends.push(bend)
      filter.append(bend, isolate)
    }
    filter.append(
      fe('feBlend', { in: 'only-r', in2: 'only-g', mode: 'screen', result: 'rg' }),
      fe('feBlend', { in: 'rg', in2: 'only-b', mode: 'screen' })
    )
    return { filter, image, bends, w: 0, h: 0 }
  }

  const buildLens = (el: HTMLElement) => {
    if (!defs || !el.isConnected) return
    const w = el.offsetWidth
    const h = el.offsetHeight
    if (w < 24 || h < 24) return
    let rec = registry.get(el)
    if (rec && Math.abs(rec.w - w) < 2 && Math.abs(rec.h - h) < 2) return

    if (!rec) {
      const id = `lens-${lensCounter++}`
      rec = createFilter(id)
      defs.appendChild(rec.filter)
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
    const scale = Math.min(36, Math.min(w, h) * 0.35)
    rec.bends.forEach((bend, index) => {
      bend.setAttribute('scale', String(scale * CHANNELS[index].spread))
    })
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

  const releaseLight = () => {
    if (!litElement) return
    litElement.style.removeProperty('--mx')
    litElement.style.removeProperty('--my')
    litElement = null
  }

  const onPointerMove = (event: PointerEvent) => {
    if (event.pointerType !== 'mouse') return
    cancelAnimationFrame(lightFrame)
    lightFrame = requestAnimationFrame(() => {
      const target = (event.target as Element | null)?.closest?.<HTMLElement>(LIGHT_SELECTOR) ?? null
      if (target !== litElement) releaseLight()
      if (!target) return
      const rect = target.getBoundingClientRect()
      target.style.setProperty('--mx', `${((event.clientX - rect.left) / rect.width) * 100}%`)
      target.style.setProperty('--my', `${((event.clientY - rect.top) / rect.height) * 100}%`)
      litElement = target
    })
  }

  onMounted(() => {
    defs = fe<SVGSVGElement>('svg', { width: 0, height: 0, 'aria-hidden': 'true', focusable: 'false' })
    defs.classList.add('lg-defs')
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
    document.addEventListener('pointermove', onPointerMove, { passive: true })
    document.addEventListener('pointerleave', releaseLight)
  })

  onUnmounted(() => {
    mutationObserver?.disconnect()
    resizeObserver?.disconnect()
    window.clearTimeout(rebuildTimer)
    cancelAnimationFrame(lightFrame)
    document.removeEventListener('pointermove', onPointerMove)
    document.removeEventListener('pointerleave', releaseLight)
    registry.clear()
    defs?.remove()
    defs = null
  })
}
