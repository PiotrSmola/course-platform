import { toast as originalToast, type ToastOptions } from 'vue3-toastify'

const defaultOptions: ToastOptions = {
  position: 'top-right',
  autoClose: 4000,
  hideProgressBar: false,
  closeOnClick: true,
  pauseOnHover: true,
  theme: 'dark'
}

type ToastFn = (content: unknown, options?: ToastOptions) => unknown

function wrap(fn: ToastFn): ToastFn {
  return (content: unknown, options?: ToastOptions) => {
    const merged = { ...defaultOptions, ...(options ?? {}) }
    return fn(content, merged)
  }
}

export const toast = {
  success: wrap(originalToast.success.bind(originalToast)),
  error: wrap(originalToast.error.bind(originalToast)),
  info: wrap(originalToast.info.bind(originalToast)),
  warning: wrap(originalToast.warning.bind(originalToast))
}