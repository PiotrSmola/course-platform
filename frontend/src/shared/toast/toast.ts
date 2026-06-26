import { toast as originalToast, type ToastOptions } from 'vue3-toastify'

const defaultOptions: ToastOptions = {
  position: 'top-right',
  autoClose: 4000,
  hideProgressBar: false,
  closeOnClick: true,
  pauseOnHover: true,
  theme: 'dark'
}

function wrap<T extends (...args: any[]) => any>(fn: T): T {
  return ((...args: any[]) => {
    const [content, options] = args
    const merged = { ...defaultOptions, ...(options ?? {}) }
    return fn(content, merged)
  }) as T
}

export const toast = {
  success: wrap(originalToast.success.bind(originalToast)),
  error: wrap(originalToast.error.bind(originalToast)),
  info: wrap(originalToast.info.bind(originalToast)),
  warning: wrap(originalToast.warning.bind(originalToast))
}