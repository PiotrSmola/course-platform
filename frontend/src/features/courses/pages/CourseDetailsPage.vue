<template>
  <div class="course-details" v-if="course">
    <div class="container">
      <div class="hero-section">
        <div class="hero-content">
          <span class="eyebrow course-author glass">{{ course.instructorName }}</span>
          <h1>{{ course.title }}</h1>
          <div class="title-actions">
            <button
              v-if="showWishlistButton"
              type="button"
              class="wishlist-btn"
              :class="{ active: course.isOnWishlist }"
              :disabled="wishlistToggle.isPending.value"
              :aria-label="course.isOnWishlist ? 'Usuń z listy życzeń' : 'Dodaj do listy życzeń'"
              @click="toggleWishlist"
            >
              {{ course.isOnWishlist ? '♥' : '♡' }}
            </button>
          </div>
          <p class="description">{{ course.description }}</p>
          <div class="meta">
            <span class="badge level">{{ levelLabel }}</span>
            <span class="badge modules">{{ course.modules.length }} modułów</span>
            <span class="badge price">{{ course.price }} zł</span>
          </div>
          <div v-if="hasSubscriptionAccess" class="subscription-access glass-card">
            <div>
              <strong>Masz aktywny All-access</strong>
              <p>Ten kurs jest już dostępny w Twojej subskrypcji.</p>
            </div>
            <button
              v-if="canManageSubscription"
              type="button"
              class="btn btn-ghost"
              :disabled="billingPortalMutation.isPending.value"
              @click="openBillingPortal"
            >
              {{ billingPortalMutation.isPending.value ? 'Otwieramy...' : 'Zarządzaj subskrypcją' }}
            </button>
          </div>
          <div class="actions" v-if="!isInstructor && isPublished">
            <template v-if="authStore.isAuthenticated && !canAccessContent">
              <div v-if="isPaid" class="checkout-box">
                <button
                  type="button"
                  class="coupon-toggle"
                  :aria-expanded="showCouponField"
                  aria-controls="coupon-field"
                  @click="showCouponField = !showCouponField"
                >
                  {{ showCouponField ? 'Ukryj kod rabatowy' : 'Mam kod rabatowy' }}
                </button>
                <div v-if="showCouponField" id="coupon-field" class="coupon-row">
                  <input
                    v-model="couponCode"
                    type="text"
                    placeholder="Kod rabatowy"
                    :disabled="checkoutMutation.isPending.value || previewCouponMutation.isPending.value"
                  />
                  <button
                    type="button"
                    class="btn btn-ghost"
                    :disabled="!couponCode.trim() || previewCouponMutation.isPending.value"
                    @click="applyCoupon"
                  >
                    Zastosuj
                  </button>
                </div>
                <p v-if="couponPreview" class="coupon-preview">
                  Cena: {{ couponPreview.originalAmount }} zł →
                  {{ couponPreview.finalAmount }} zł
                  (−{{ couponPreview.discountAmount }} zł)
                </p>
                <div class="checkout-actions">
                  <button
                    class="btn btn-primary"
                  :disabled="checkoutMutation.isPending.value"
                  @click="buy"
                >
                  {{
                    checkoutMutation.isPending.value
                      ? 'Przekierowujemy...'
                      : `Kup teraz — ${displayPrice} zł`
                  }}
                </button>
                <button
                  type="button"
                  class="btn btn-ghost"
                  :disabled="subscriptionCheckoutMutation.isPending.value"
                  @click="startSubscription"
                >
                  {{
                    subscriptionCheckoutMutation.isPending.value
                      ? 'Przekierowujemy...'
                      : `All-access — ${monthlyPriceLabel}/mies.`
                  }}
                </button>
              </div>
                <div v-if="isPaid" class="gift-purchase">
                  <button
                    type="button"
                    class="gift-purchase__toggle"
                    :aria-expanded="showGiftForm"
                    aria-controls="gift-purchase-form"
                    @click="showGiftForm = !showGiftForm"
                  >
                    {{ showGiftForm ? 'Anuluj prezent' : 'Kup w prezencie' }}
                  </button>
                  <form v-if="showGiftForm" id="gift-purchase-form" @submit.prevent="buyGift">
                    <label for="gift-recipient-email">Email odbiorcy</label>
                    <div class="gift-purchase__fields">
                      <input
                        id="gift-recipient-email"
                        v-model.trim="giftRecipientEmail"
                        type="email"
                        autocomplete="email"
                        required
                        placeholder="email@example.com"
                        :disabled="giftCheckoutMutation.isPending.value"
                      />
                      <button
                        type="submit"
                        class="btn btn-ghost"
                        :disabled="!giftRecipientEmail || giftCheckoutMutation.isPending.value"
                      >
                        {{ giftCheckoutMutation.isPending.value ? 'Przekierowujemy...' : 'Kup prezent' }}
                      </button>
                    </div>
                  </form>
                </div>
              </div>
              <button v-if="!isPaid" class="btn btn-primary" @click="enroll">Zapisz się</button>
            </template>
            <router-link
              v-else-if="authStore.isAuthenticated && firstAccessibleLesson"
              class="btn btn-primary"
              :to="{ name: 'Learning', params: { courseId: course.id, lessonId: firstAccessibleLesson.id } }"
            >
              {{ hasSubscriptionAccess ? 'Otwórz kurs w All-access' : 'Przejdź do kursu' }}
            </router-link>
            <router-link class="btn btn-primary" :to="{ name: 'MyCourses' }" v-else-if="authStore.isAuthenticated && isEnrolled">Przejdź do kursu</router-link>
            <router-link class="btn btn-primary" :to="{ name: 'Login', query: { redirect: route.fullPath } }" v-else>Zaloguj się, aby zapisać</router-link>
          </div>
          <div class="waitlist-actions" v-if="showWaitlistSection">
            <button
              type="button"
              class="btn btn-primary"
              :disabled="waitlistToggle.isPending.value"
              @click="toggleWaitlist"
            >
              {{ course.isOnWaitlist ? 'Wypisz się z powiadomień' : 'Powiadom o starcie' }}
            </button>
          </div>
          <div class="waitlist-actions" v-else-if="showWaitlistLogin">
            <router-link class="btn btn-primary" :to="{ name: 'Login', query: { redirect: route.fullPath } }">
              Zaloguj się, aby dołączyć do listy oczekujących
            </router-link>
          </div>
        </div>
        <div class="hero-visual">
          <CourseThumbnail class="glass-card course-thumb" :url="course.thumbnailUrl" />
        </div>
      </div>

      <div class="content-grid">
        <div class="modules-section">
          <h2>Program kursu</h2>
          <div class="module-list">
            <div v-for="(module, mIdx) in course.modules" :key="module.id" class="module-item glass-card">
              <div class="module-header">
                <span class="module-num">{{ mIdx + 1 }}</span>
                <h3>{{ module.title }}</h3>
              </div>
              <div class="lessons-list">
                <template v-for="lesson in module.lessons" :key="lesson.id">
                  <router-link
                    v-if="canAccessContent && !lesson.isLocked"
                    class="lesson-item link"
                    :to="{ name: 'Learning', params: { courseId: course.id, lessonId: lesson.id } }"
                  >
                    <span class="lesson-icon">{{ lesson.isCompleted ? '✓' : '▶' }}</span>
                    <span class="lesson-title">{{ lesson.title }}</span>
                    <span class="lesson-duration">{{ lesson.duration }} min</span>
                  </router-link>
                  <div v-else-if="canAccessContent" class="lesson-item locked" :title="lesson.lockReason || undefined">
                    <span class="lesson-icon">🔒</span>
                    <span class="lesson-title">{{ lesson.title }}</span>
                    <span class="lesson-duration">{{ lesson.duration }} min</span>
                  </div>
                  <div v-else class="lesson-item">
                    <span class="lesson-icon">▶</span>
                    <span class="lesson-title">{{ lesson.title }}</span>
                    <span class="lesson-duration">{{ lesson.duration }} min</span>
                  </div>
                </template>
              </div>
            </div>
          </div>
        </div>

        <div class="reviews-section">
          <h2>Opinie</h2>

          <form v-if="course.canReview" class="review-form glass-card" @submit="onCreateSubmit">
            <h3>Dodaj opinię</h3>
            <div class="form-group">
              <label for="review-rating">Ocena</label>
              <select id="review-rating" v-model.number="createRating">
                <option v-for="n in 5" :key="n" :value="n">{{ n }} ★</option>
              </select>
              <span v-if="createErrors.rating" class="field-error">{{ createErrors.rating }}</span>
            </div>
            <div class="form-group">
              <label for="review-comment">Komentarz</label>
              <textarea id="review-comment" v-model="createComment" rows="3" placeholder="Twoja opinia o kursie..." />
              <span v-if="createErrors.comment" class="field-error">{{ createErrors.comment }}</span>
            </div>
            <button type="submit" class="btn btn-primary" :disabled="createReviewMutation.isPending.value || !createMeta.valid">
              Opublikuj opinię
            </button>
          </form>

          <form v-else-if="course.hasUserReviewed && course.userReviewId && isEditing" class="review-form glass-card" @submit="onUpdateSubmit">
            <h3>Edytuj opinię</h3>
            <div class="form-group">
              <label for="edit-rating">Ocena</label>
              <select id="edit-rating" v-model.number="editRating">
                <option v-for="n in 5" :key="n" :value="n">{{ n }} ★</option>
              </select>
              <span v-if="editErrors.rating" class="field-error">{{ editErrors.rating }}</span>
            </div>
            <div class="form-group">
              <label for="edit-comment">Komentarz</label>
              <textarea id="edit-comment" v-model="editComment" rows="3" placeholder="Twoja opinia o kursie..." />
              <span v-if="editErrors.comment" class="field-error">{{ editErrors.comment }}</span>
            </div>
            <div class="edit-actions">
              <button type="submit" class="btn btn-primary" :disabled="updateReviewMutation.isPending.value || !editMeta.valid">
                Zapisz zmiany
              </button>
              <button type="button" class="btn btn-ghost" @click="cancelEdit">Anuluj</button>
            </div>
          </form>

          <div class="reviews-list">
            <div v-for="review in course.reviews" :key="review.id" class="review-card glass-card">
              <div class="review-head">
                <div class="stars">{{ '★'.repeat(review.rating) }}</div>
                <div v-if="course.userReviewId === review.id" class="own-review-actions">
                  <button type="button" class="btn-link" @click="startEdit(review)">Edytuj</button>
                  <button
                    type="button"
                    class="btn-link btn-link-danger"
                    :disabled="deleteReviewMutation.isPending.value"
                    @click="confirmDeleteReview(review.id)"
                  >
                    Usuń
                  </button>
                </div>
              </div>
              <p>{{ review.comment }}</p>
              <div class="review-author">
                <span class="avatar">{{ review.authorName.charAt(0) }}</span>
                <span>{{ review.authorName }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  <div v-else-if="courseQuery.isLoading" class="loading">Ładowanie...</div>
  <div v-else-if="courseQuery.isError" class="error-state">
    <div class="container">
      <h2>Nie znaleziono kursu</h2>
      <p>Kurs nie istnieje lub nie masz do niego dostępu.</p>
      <router-link class="btn btn-primary" :to="{ name: 'Courses' }">Przeglądaj kursy</router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useCourseDetails } from '@/features/courses/composables/useCourses'
import { useEnroll } from '@/features/enrollment/composables/useEnrollment'
import { useCreateGiftCheckout } from '@/features/gifts/composables/useGifts'
import {
  useCreateBillingPortalSession,
  useCreateCheckout,
  useCreateSubscriptionCheckout,
  useMySubscription,
  usePreviewCoupon,
  useSubscriptionOffer
} from '@/features/payments/composables/usePayments'
import { useCreateReview, useUpdateReview, useDeleteReview } from '@/features/reviews/composables/useReviews'
import { useToggleWishlist } from '@/features/wishlist/composables/useWishlist'
import { useToggleWaitlist } from '@/features/waitlist/composables/useWaitlist'
import { createReviewSchema, updateReviewSchema } from '@/features/reviews/schemas/review.schema'
import { CourseLevel, CourseStatus } from '@/features/courses/types/course.types'
import type { ReviewDto } from '@/features/courses/types/course.types'
import CourseThumbnail from '@/shared/components/media/CourseThumbnail.vue'

const route = useRoute()
const authStore = useAuthStore()
const courseQuery = useCourseDetails(() => route.params.id as string)
const course = computed(() => courseQuery.data.value)
const enrollMutation = useEnroll()
const courseId = () => route.params.id as string
const createReviewMutation = useCreateReview(courseId)
const updateReviewMutation = useUpdateReview(courseId)
const deleteReviewMutation = useDeleteReview(courseId)
const isEditing = ref(false)
const wishlistToggle = useToggleWishlist()
const waitlistToggle = useToggleWaitlist()

const {
  handleSubmit: handleCreateSubmit,
  defineField: defineCreateField,
  errors: createErrors,
  meta: createMeta,
  resetForm: resetCreateForm
} = useForm({
  validationSchema: toTypedSchema(createReviewSchema),
  initialValues: { rating: 5, comment: '' }
})

const [createRating] = defineCreateField('rating')
const [createComment] = defineCreateField('comment')

const {
  handleSubmit: handleUpdateSubmit,
  defineField: defineEditField,
  errors: editErrors,
  meta: editMeta,
  resetForm: resetEditForm,
  setValues: setEditValues
} = useForm({
  validationSchema: toTypedSchema(updateReviewSchema),
  initialValues: { rating: 5, comment: '' }
})

const [editRating] = defineEditField('rating')
const [editComment] = defineEditField('comment')

const checkoutMutation = useCreateCheckout()
const giftCheckoutMutation = useCreateGiftCheckout()
const subscriptionCheckoutMutation = useCreateSubscriptionCheckout()
const billingPortalMutation = useCreateBillingPortalSession()
const subscriptionQuery = useMySubscription(() => authStore.isAuthenticated)
const subscriptionOfferQuery = useSubscriptionOffer()
const previewCouponMutation = usePreviewCoupon()
const couponCode = ref('')
const showCouponField = ref(false)
const showGiftForm = ref(false)
const giftRecipientEmail = ref('')
const couponPreview = ref<{
  originalAmount: number
  discountAmount: number
  finalAmount: number
  code: string
} | null>(null)

const isInstructor = computed(() => authStore.user?.id === course.value?.instructorId)
const isEnrolled = computed(() => course.value?.isEnrolled ?? false)
const canAccessContent = computed(() => course.value?.canAccessContent ?? false)
const hasSubscriptionAccess = computed(() => course.value?.hasSubscriptionAccess ?? false)
const canManageSubscription = computed(() => subscriptionQuery.data.value?.canManageInPortal ?? false)
const monthlyPriceLabel = computed(() => {
  const price = subscriptionOfferQuery.data.value?.monthlyPricePln ?? 399
  return `${price} zł`
})
const isPaid = computed(() => (course.value?.price ?? 0) > 0)
const isPublished = computed(() => course.value?.status === CourseStatus.Published)
const showWishlistButton = computed(
  () => authStore.isAuthenticated && isPublished.value && !isInstructor.value && !canAccessContent.value
)
const showWaitlistSection = computed(
  () =>
    authStore.isAuthenticated &&
    !isPublished.value &&
    !isInstructor.value &&
    !canAccessContent.value &&
    ((course.value?.canJoinWaitlist ?? false) || (course.value?.isOnWaitlist ?? false))
)
const showWaitlistLogin = computed(
  () => !authStore.isAuthenticated && !isPublished.value && !canAccessContent.value
)
const displayPrice = computed(
  () => couponPreview.value?.finalAmount ?? course.value?.price ?? 0
)
const firstAccessibleLesson = computed(() => {
  const lessons = course.value?.modules.flatMap((module) => module.lessons) ?? []
  return lessons.find((lesson) => !lesson.isLocked) ?? lessons[0] ?? null
})

const levelLabel = computed(() => {
  if (!course.value) return ''
  switch (course.value.level) {
    case CourseLevel.Beginner: return 'Początkujący'
    case CourseLevel.Intermediate: return 'Średni'
    case CourseLevel.Advanced: return 'Zaawansowany'
    default: return ''
  }
})

const enroll = () => {
  if (!course.value) return
  enrollMutation.mutate(course.value.id)
}

const applyCoupon = async () => {
  if (!course.value || !couponCode.value.trim()) return
  couponPreview.value = await previewCouponMutation.mutateAsync({
    courseId: course.value.id,
    couponCode: couponCode.value.trim()
  })
}

const buy = () => {
  if (!course.value) return
  checkoutMutation.mutate({
    courseId: course.value.id,
    couponCode: couponPreview.value?.code ?? (couponCode.value.trim() || null)
  })
}

const buyGift = () => {
  if (!course.value || !giftRecipientEmail.value.trim()) return
  giftCheckoutMutation.mutate({
    courseId: course.value.id,
    recipientEmail: giftRecipientEmail.value.trim()
  })
}

const startSubscription = () => {
  subscriptionCheckoutMutation.mutate()
}

const openBillingPortal = () => {
  billingPortalMutation.mutate()
}

const toggleWishlist = () => {
  if (!course.value) return
  wishlistToggle.toggle(course.value.id, course.value.isOnWishlist)
}

const toggleWaitlist = () => {
  if (!course.value) return
  waitlistToggle.toggle(course.value.id, course.value.isOnWaitlist)
}

const onCreateSubmit = handleCreateSubmit((values) => {
  createReviewMutation.mutate(values, {
    onSuccess: () => {
      resetCreateForm({ values: { rating: 5, comment: '' } })
    }
  })
})

function startEdit(review: ReviewDto) {
  isEditing.value = true
  setEditValues({ rating: review.rating, comment: review.comment })
}

function cancelEdit() {
  isEditing.value = false
  resetEditForm({ values: { rating: 5, comment: '' } })
}

const onUpdateSubmit = handleUpdateSubmit((values) => {
  const reviewId = course.value?.userReviewId
  if (!reviewId) return
  updateReviewMutation.mutate(
    { reviewId, ...values },
    {
      onSuccess: () => {
        isEditing.value = false
        resetEditForm({ values: { rating: 5, comment: '' } })
      }
    }
  )
})

function confirmDeleteReview(reviewId: string) {
  if (window.confirm('Czy na pewno chcesz usunąć swoją opinię?')) {
    deleteReviewMutation.mutate(reviewId, {
      onSuccess: () => {
        isEditing.value = false
      }
    })
  }
}

watch(() => course.value?.userReviewId, () => {
  isEditing.value = false
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.course-details {
  padding: calc($header-height + 40px) 0 80px;
}

.error-state {
  padding: calc($header-height + 80px) 0 80px;
  text-align: center;

  h2 {
    font-size: 1.5rem;
    margin-bottom: 12px;
  }

  p {
    color: $color-muted;
    margin-bottom: 24px;
  }
}

.hero-section {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(420px, 0.95fr);
  gap: clamp(36px, 5vw, 72px);
  align-items: center;
  margin-bottom: 80px;

  @media (max-width: 880px) {
    grid-template-columns: 1fr;
  }
}

.hero-content {
  h1 {
    font-size: clamp(2rem, 4vw, 3.2rem);
    margin: 16px 0 20px;
  }

  .title-actions {
    display: flex;
    align-items: center;
    gap: 12px;
    margin: -8px 0 12px;
  }

  .wishlist-btn {
    width: 44px;
    height: 44px;
    border-radius: 50%;
    border: none;
    background: rgba(255, 255, 255, 0.08);
    color: $color-muted;
    font-size: 1.4rem;
    cursor: pointer;
    transition: color 0.2s, background 0.2s, transform 0.2s;

    &.active {
      color: #f87171;
      background: rgba(248, 113, 113, 0.15);
    }

    &:hover:not(:disabled) {
      transform: scale(1.05);
    }

    &:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
  }

  .description {
    color: $color-muted;
    font-size: 1.05rem;
    line-height: 1.7;
    margin-bottom: 24px;
  }
}

.meta {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-bottom: 32px;
}

.badge {
  @include liquid-glass;
  --lg-r: 999px;
  --lg-blur: 12px;
  --lg-tint: rgba(255, 255, 255, 0.05);
  padding: 6px 14px;
  font-size: 0.82rem;
  font-weight: 600;
}

.price {
  --lg-tint: rgba(139, 92, 246, 0.2);
  color: #d6c9ff;
}

.course-author {
  --lg-r: 999px;
  --lg-blur: 12px;
  --lg-tint: rgba(139, 92, 246, 0.14);
  display: inline-flex;
  width: fit-content;
  padding: 6px 13px;
  font-size: 0.8rem;
  font-weight: 600;
}

.actions .btn {
  padding: 14px 32px;
}

.waitlist-actions {
  margin-top: 16px;
}

.subscription-access {
  --lg-r: 20px;
  --lg-blur: 12px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 14px 16px;
  margin-bottom: 24px;

  max-width: 660px;
  strong {
    display: block;
    margin-bottom: 6px;
  }

  p {
    margin: 0;
    color: $color-muted;
    font-size: 0.92rem;
  }
}

.checkout-box {
  display: flex;
  flex-direction: column;
  gap: 12px;
  align-items: flex-start;
}

.coupon-row {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;

  input {
    min-width: 180px;
    border-radius: 12px;
    border: 1px solid rgba(255, 255, 255, 0.12);
    background: rgba(255, 255, 255, 0.04);
    color: inherit;
    padding: 10px 12px;
  }
}

.coupon-preview {
  margin: 0;
  color: $color-muted;
  font-size: 0.9rem;
}

.hero-visual {
  display: flex;
  justify-content: center;
}

.course-thumb {
  width: 100%;
  max-width: 420px;
  aspect-ratio: 16/10;
  border-radius: 28px;
  box-shadow: 0 40px 90px rgba(3, 6, 24, 0.6), 0 4px 14px rgba(3, 6, 24, 0.35);
}

.content-grid {
  display: grid;
  grid-template-columns: 1fr 0.4fr;
  gap: 40px;

  @media (max-width: 880px) {
    grid-template-columns: 1fr;
  }
}

.reviews-section h2 {
  font-size: 1.5rem;
  margin-bottom: 28px;
}

.review-form {
  padding: 24px;
  margin-bottom: 24px;

  h3 {
    font-size: 1rem;
    margin-bottom: 16px;
  }

  .form-group {
    display: flex;
    flex-direction: column;
    gap: 8px;
    margin-bottom: 12px;

    label {
      font-size: 0.88rem;
      font-weight: 600;
      color: $color-muted;
    }
  }

  textarea {
    width: 100%;
    padding: 12px;
    border-radius: 12px;
    border: none;
    background: rgba(255, 255, 255, 0.04);
    color: $color-ink;
    font: inherit;
    resize: vertical;
  }

  select {
    width: fit-content;
    padding: 6px 12px;
    border-radius: 8px;
    background: rgba(255, 255, 255, 0.06);
    color: $color-ink;
    border: none;
  }

  .field-error {
    color: #f87171;
    font-size: 0.82rem;
  }

  .edit-actions {
    display: flex;
    gap: 12px;
    flex-wrap: wrap;
  }
}

.lesson-item.link {
  text-decoration: none;
  color: inherit;
  transition: background 0.2s;

  &:hover {
    background: rgba(255, 255, 255, 0.06);
  }
}

.modules-section h2,
.reviews-section h2 {
  font-size: 1.5rem;
  margin-bottom: 28px;
}

.module-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.module-item {
  padding: 24px;
}

.module-header {
  display: flex;
  align-items: center;
  gap: 14px;
  margin-bottom: 16px;

  .module-num {
    width: 32px;
    height: 32px;
    display: grid;
    place-items: center;
    border-radius: 50%;
    background: rgba(139, 92, 246, 0.25);
    font-size: 0.85rem;
    font-weight: 700;
  }

  h3 {
    font-size: 1.1rem;
  }
}

.lessons-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding-left: 46px;
}

.lesson-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 14px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.03);
  font-size: 0.92rem;

  .lesson-icon {
    color: $color-gold;
    font-size: 0.8rem;
  }

  .lesson-title {
    flex: 1;
  }

  .lesson-duration {
    color: $color-faint;
    font-size: 0.82rem;
  }
}

.lesson-item.locked {
  opacity: 0.7;
  cursor: not-allowed;
}

.reviews-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.review-card {
  padding: 24px;

  .review-head {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    margin-bottom: 12px;
  }

  .stars {
    color: #fcd34d;
    font-size: 1.1rem;
  }

  .own-review-actions {
    display: flex;
    gap: 12px;
  }

  .btn-link {
    background: none;
    border: none;
    padding: 0;
    font: inherit;
    font-size: 0.85rem;
    font-weight: 600;
    color: $color-cyan;
    cursor: pointer;

    &:hover {
      text-decoration: underline;
    }

    &:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
  }

  .btn-link-danger {
    color: #f87171;
  }

  p {
    color: $color-muted;
    font-size: 0.95rem;
    margin-bottom: 16px;
  }

  .review-author {
    display: flex;
    align-items: center;
    gap: 10px;

    .avatar {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      display: grid;
      place-items: center;
      font-size: 0.8rem;
      font-weight: 700;
      background: linear-gradient(135deg, rgba(139, 92, 246, 0.35), rgba(34, 211, 238, 0.25));
    }

    span:last-child {
      font-size: 0.9rem;
      color: $color-muted;
    }
  }
}

.loading {
  text-align: center;
  padding: 120px;
  color: $color-muted;
}
.coupon-toggle {
  appearance: none;
  border: 0;
  padding: 0;
  background: transparent;
  color: $color-gold;
  font: inherit;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;

  &:hover,
  &:focus-visible {
    color: #fcd34d;
    text-decoration: underline;
  }
}

.coupon-row input {
  min-width: min(100%, 260px);
  min-height: 46px;
  border-radius: 14px;
  padding: 10px 12px;
  font: inherit;
  font-size: 1rem;
  outline: none;

  &:focus {
    border-color: rgba(245, 158, 11, 0.58);
    box-shadow: 0 0 0 3px rgba(245, 158, 11, 0.14);
  }
}

.checkout-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  width: 100%;

  .btn {
    flex: 1 1 210px;
    white-space: nowrap;
  }
}

.course-thumb {
  max-width: 560px;
  aspect-ratio: 16 / 9;
  border-radius: 32px;
  --lg-blur: 14px;
}

.content-grid {
  grid-template-columns: minmax(0, 0.8fr) minmax(340px, 1.2fr);
  gap: 48px;
}

.review-form,
.review-card {
  --lg-blur: 12px;
  padding: 28px;
}

.review-form select {
  width: min(100%, 190px);
  min-height: 44px;
  padding: 9px 38px 9px 13px;
  border: 1px solid rgba(255, 255, 255, 0.16);
  border-radius: 13px;
  background-color: rgba(15, 23, 42, 0.72);
  color: $color-ink;
  font: inherit;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  outline: none;
  box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.18), 0 6px 16px rgba(3, 6, 24, 0.22);

  &:focus {
    border-color: rgba(245, 158, 11, 0.6);
    box-shadow: 0 0 0 3px rgba(245, 158, 11, 0.14);
  }

  option {
    background: #111827;
    color: $color-ink;
  }
}

.module-item {
  --lg-blur: 10px;
  padding: 18px;
}

.lessons-list {
  gap: 8px;
}

.lesson-item {
  padding: 9px 12px;
}

@media (max-width: 880px) {
  .content-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 560px) {
  .checkout-actions .btn {
    flex-basis: 100%;
  }

  .subscription-access {
    align-items: flex-start;
    flex-direction: column;
  }

  .coupon-row {
    width: 100%;

    input,
    .btn {
      width: 100%;
    }
  }
}

.gift-purchase {
  display: flex;
  flex-direction: column;
  gap: 10px;
  width: 100%;
  padding-top: 4px;
}

.gift-purchase__toggle {
  align-self: flex-start;
  border: 0;
  padding: 0;
  background: transparent;
  color: $color-cyan;
  font: inherit;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;

  &:hover,
  &:focus-visible {
    color: #67e8f9;
    text-decoration: underline;
  }
}

.gift-purchase form {
  display: flex;
  flex-direction: column;
  gap: 8px;
  max-width: 560px;

  label {
    color: $color-muted;
    font-size: 0.85rem;
    font-weight: 600;
  }
}

.gift-purchase__fields {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;

  input {
    flex: 1 1 220px;
    min-height: 46px;
    min-width: 0;
    padding: 10px 12px;
    border: 1px solid rgba(255, 255, 255, 0.12);
    border-radius: 14px;
    background: rgba(255, 255, 255, 0.04);
    color: $color-ink;
    font: inherit;
    font-size: 1rem;
    outline: none;

    &:focus {
      border-color: rgba(34, 211, 238, 0.58);
      box-shadow: 0 0 0 3px rgba(34, 211, 238, 0.14);
    }
  }

  .btn {
    flex: 0 1 auto;
  }
}

@media (max-width: 560px) {
  .gift-purchase__fields .btn {
    width: 100%;
  }
}

</style>
