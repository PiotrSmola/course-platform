<template>
  <div class="course-details" v-if="course">
    <div class="container">
      <div class="hero-section">
        <div class="hero-content">
          <span class="eyebrow">{{ course.technologyNames?.[0] ?? course.categoryNames?.[0] ?? 'Kurs' }}</span>
          <h1>{{ course.title }}</h1>
          <div class="title-actions">
            <p class="byline">
              <span class="byline-label">Prowadzi —</span>
              <span class="byline-name">{{ course.instructorName }}</span>
            </p>
            <button
              v-if="showWishlistButton"
              type="button"
              class="wishlist-btn glass"
              :class="{ active: course.isOnWishlist }"
              :disabled="wishlistToggle.isPending.value"
              :aria-label="course.isOnWishlist ? 'Usuń z listy życzeń' : 'Dodaj do listy życzeń'"
              @click="toggleWishlist"
            >
              {{ course.isOnWishlist ? '♥' : '♡' }}
            </button>
          </div>
          <p class="description">{{ course.description }}</p>
          <dl class="spec">
            <div class="spec-item">
              <dt>Poziom</dt>
              <dd>{{ levelLabel }}</dd>
            </div>
            <div class="spec-item">
              <dt>Moduły</dt>
              <dd>{{ course.modules.length }}</dd>
            </div>
            <div class="spec-item">
              <dt>Lekcje</dt>
              <dd>{{ lessonCount }}</dd>
            </div>
            <div class="spec-item">
              <dt>Cena</dt>
              <dd class="spec-price">{{ course.price }} zł</dd>
            </div>
          </dl>
          <div v-if="hasSubscriptionAccess" class="subscription-access glass-card">
            <div>
              <span class="subscription-label">All-access</span>
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
                <div class="checkout-links">
                  <button
                    type="button"
                    class="text-toggle"
                    :aria-expanded="showCouponField"
                    aria-controls="coupon-field"
                    @click="showCouponField = !showCouponField"
                  >
                    {{ showCouponField ? 'Ukryj kod rabatowy' : 'Mam kod rabatowy' }}
                  </button>
                  <button
                    type="button"
                    class="text-toggle"
                    :aria-expanded="showGiftForm"
                    aria-controls="gift-purchase-form"
                    @click="showGiftForm = !showGiftForm"
                  >
                    {{ showGiftForm ? 'Anuluj prezent' : 'Kup w prezencie' }}
                  </button>
                </div>
                <div v-if="showCouponField" id="coupon-field" class="inline-field">
                  <input
                    v-model="couponCode"
                    type="text"
                    placeholder="Kod rabatowy"
                    aria-label="Kod rabatowy"
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
                  Cena: <s>{{ couponPreview.originalAmount }} zł</s> →
                  <strong>{{ couponPreview.finalAmount }} zł</strong>
                  (−{{ couponPreview.discountAmount }} zł)
                </p>
                <form v-if="showGiftForm" id="gift-purchase-form" class="gift-form" @submit.prevent="buyGift">
                  <label for="gift-recipient-email">Email odbiorcy</label>
                  <div class="inline-field">
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
          <CourseThumbnail class="course-thumb" :url="course.thumbnailUrl" :title="course.title" :label="course.technologyNames?.[0] ?? course.categoryNames?.[0]" :seed="course.id" />
          <div v-if="course.categoryNames.length || course.technologyNames.length" class="hero-tags">
            <span v-for="cat in course.categoryNames" :key="cat" class="tag">{{ cat }}</span>
            <span v-for="tech in course.technologyNames" :key="tech" class="tag tech">{{ tech }}</span>
          </div>
        </div>
      </div>

      <div class="content-grid">
        <section class="modules-section">
          <header class="block-head">
            <span class="eyebrow">Spis treści</span>
            <h2>Program <em>kursu</em></h2>
          </header>
          <ol class="module-list">
            <li v-for="(module, mIdx) in course.modules" :key="module.id" class="module-item">
              <div class="module-header">
                <span class="module-num">{{ String(mIdx + 1).padStart(2, '0') }}</span>
                <h3>{{ module.title }}</h3>
                <span class="module-count">{{ module.lessons.length }} lekcji</span>
              </div>
              <div class="lessons-list">
                <template v-for="(lesson, lIdx) in module.lessons" :key="lesson.id">
                  <router-link
                    v-if="canAccessContent && !lesson.isLocked"
                    class="lesson-item link"
                    :class="{ done: lesson.isCompleted }"
                    :to="{ name: 'Learning', params: { courseId: course.id, lessonId: lesson.id } }"
                  >
                    <span class="lesson-num">{{ mIdx + 1 }}.{{ lIdx + 1 }}</span>
                    <span class="lesson-title">{{ lesson.title }}</span>
                    <span v-if="lesson.isCompleted" class="lesson-state">✓</span>
                    <span class="lesson-duration">{{ lesson.duration }} min</span>
                  </router-link>
                  <div v-else-if="canAccessContent" class="lesson-item locked" :title="lesson.lockReason || undefined">
                    <span class="lesson-num">{{ mIdx + 1 }}.{{ lIdx + 1 }}</span>
                    <span class="lesson-title">{{ lesson.title }}</span>
                    <svg class="lesson-state" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-label="Zablokowana"><rect x="4" y="11" width="16" height="10" rx="2"/><path d="M8 11V7a4 4 0 0 1 8 0v4"/></svg>
                    <span class="lesson-duration">{{ lesson.duration }} min</span>
                  </div>
                  <div v-else class="lesson-item">
                    <span class="lesson-num">{{ mIdx + 1 }}.{{ lIdx + 1 }}</span>
                    <span class="lesson-title">{{ lesson.title }}</span>
                    <span class="lesson-duration">{{ lesson.duration }} min</span>
                  </div>
                </template>
              </div>
            </li>
          </ol>
        </section>

        <section class="reviews-section">
          <header class="block-head">
            <span class="eyebrow">Opinie</span>
            <h2>Co mówią <em>kursanci</em></h2>
            <p v-if="course.reviewCount" class="rating-summary">
              <span class="star">★</span> {{ course.averageRating.toFixed(1) }} / {{ course.reviewCount }} opinii
            </p>
          </header>

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
            <figure v-for="review in course.reviews" :key="review.id" class="review-card">
              <div class="review-head">
                <div class="stars" :aria-label="`Ocena ${review.rating} z 5`">
                  {{ '★'.repeat(review.rating) }}<span class="stars-empty">{{ '★'.repeat(5 - review.rating) }}</span>
                </div>
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
              <blockquote>{{ review.comment }}</blockquote>
              <figcaption class="review-author">— {{ review.authorName }}</figcaption>
            </figure>
          </div>
        </section>
      </div>
    </div>
  </div>
  <div v-else-if="courseQuery.isLoading" class="loading">Ładowanie...</div>
  <div v-else-if="courseQuery.isError" class="error-state">
    <div class="container">
      <span class="eyebrow">Katalog</span>
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
const lessonCount = computed(
  () => course.value?.modules.reduce((sum, module) => sum + module.lessons.length, 0) ?? 0
)
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
  padding: 32px 0 96px;
}

.error-state {
  padding: 64px 0 80px;

  h2 {
    margin: 16px 0 12px;
  }

  p {
    color: $color-muted;
    margin-bottom: 24px;
  }
}

.hero-section {
  display: grid;
  grid-template-columns: minmax(0, 1.15fr) minmax(0, 0.85fr);
  gap: clamp(36px, 5vw, 80px);
  align-items: start;
  padding-bottom: 56px;
  margin-bottom: 64px;
  border-bottom: 1px solid $color-hairline;

  @media (max-width: 880px) {
    grid-template-columns: 1fr;
  }
}

.hero-content {
  min-width: 0;

  h1 {
    font-size: clamp(2.3rem, 4.6vw, 3.9rem);
    margin: 18px 0 18px;
  }
}

.title-actions {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 22px;
}

.byline {
  font-family: $font-mono;
  font-size: 0.78rem;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  margin: 0;

  .byline-label {
    color: $color-faint;
    margin-right: 6px;
  }

  .byline-name {
    color: $color-ink;
  }
}

.wishlist-btn {
  --lg-r: 999px;
  --lg-blur: 2px;
  --lg-tint: rgba(255, 255, 255, 0.05);
  width: 40px;
  height: 40px;
  flex-shrink: 0;
  border: none;
  background: none;
  color: $color-muted;
  font-size: 1.2rem;
  line-height: 1;
  cursor: pointer;
  transition: color 0.2s, transform 0.3s $ease-jelly;

  &.active {
    --lg-tint: #{rgba($color-coral, 0.18)};
    color: $color-coral;
  }

  &:hover:not(:disabled) {
    transform: scale(1.06);
  }

  &:disabled {
    cursor: not-allowed;
    color: $color-faint;
  }
}

.description {
  max-width: 62ch;
  color: $color-muted;
  font-size: 1.04rem;
  line-height: 1.7;
  margin-bottom: 32px;
}

.spec {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  margin: 0 0 32px;
  border-top: 1px solid $color-hairline;
  border-bottom: 1px solid $color-hairline;

  @media (max-width: 560px) {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

.spec-item {
  padding: 14px 16px 16px;
  min-width: 0;

  & + & {
    border-left: 1px solid $color-hairline;
  }

  &:first-child {
    padding-left: 0;
  }

  @media (max-width: 560px) {
    &:nth-child(3) {
      border-left: none;
      padding-left: 0;
      border-top: 1px solid $color-hairline;
    }

    &:nth-child(4) {
      border-top: 1px solid $color-hairline;
    }
  }

  dt {
    @include mono-label;
    font-size: 0.62rem;
    color: $color-faint;
    margin-bottom: 8px;
  }

  dd {
    margin: 0;
    font-family: $font-mono;
    font-size: 0.92rem;
    color: $color-ink;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .spec-price {
    font-family: $font-display;
    font-weight: 600;
    font-size: 1.5rem;
    line-height: 1;
  }
}

.subscription-access {
  --lg-blur: 10px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  max-width: 660px;
  padding: 16px 18px;
  margin-bottom: 24px;

  &:hover {
    transform: none;
  }

  .subscription-label {
    @include tag;
    color: $color-on-accent;
    background: $color-accent;
    box-shadow: none;
    margin-bottom: 8px;
  }

  strong {
    display: block;
    font-family: $font-display;
    font-weight: 500;
    font-size: 1.1rem;
    margin-bottom: 4px;
  }

  p {
    margin: 0;
    color: $color-muted;
    font-size: 0.9rem;
  }
}

.actions .btn {
  padding: 14px 30px;
}

.waitlist-actions {
  margin-top: 16px;
}

.checkout-box {
  display: flex;
  flex-direction: column;
  gap: 14px;
  align-items: flex-start;
  max-width: 560px;
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

.checkout-links {
  display: flex;
  flex-wrap: wrap;
  gap: 20px;
}

.text-toggle {
  @include mono-label;
  appearance: none;
  border: 0;
  padding: 0 0 3px;
  background: transparent;
  color: $color-muted;
  font-size: 0.66rem;
  border-bottom: 1px dashed $color-hairline;
  cursor: pointer;
  transition: color 0.2s, border-color 0.2s;

  &:hover,
  &[aria-expanded="true"] {
    color: $color-accent;
    border-bottom-color: rgba($color-accent, 0.5);
  }
}

.inline-field {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  width: 100%;

  input {
    flex: 1 1 220px;
    min-width: 0;
    min-height: 46px;
    padding: 10px 14px;
    border: none;
    border-radius: $radius-md;
    background: rgba(13, 15, 19, 0.45);
    box-shadow: inset 0 0 0 1px $color-hairline;
    color: $color-ink;
    font: inherit;
    font-size: 0.95rem;
    outline: none;
    transition: box-shadow 0.2s;

    &::placeholder {
      color: $color-faint;
    }

    &:focus {
      box-shadow:
        inset 0 0 0 1px $color-hairline,
        0 0 0 2px rgba($color-accent, 0.6);
    }
  }

  .btn {
    flex: 0 1 auto;
    padding: 11px 22px;
  }
}

.coupon-preview {
  margin: 0;
  font-family: $font-mono;
  font-size: 0.8rem;
  color: $color-muted;

  s {
    color: $color-faint;
  }

  strong {
    color: $color-accent;
    font-weight: 500;
  }
}

.gift-form {
  display: flex;
  flex-direction: column;
  gap: 8px;
  width: 100%;

  label {
    @include mono-label;
    font-size: 0.62rem;
    color: $color-faint;
  }
}

.hero-visual {
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-width: 0;
}

.course-thumb {
  position: relative;
  width: 100%;
  aspect-ratio: 4 / 3;
  border-radius: $radius-card;
  box-shadow:
    inset 0 0 0 1px rgba(255, 255, 255, 0.08),
    0 40px 80px -30px rgba(0, 0, 0, 0.7);
}

.hero-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;

  .tag.tech {
    color: $color-accent;
    box-shadow: inset 0 0 0 1px rgba($color-accent, 0.35);
  }
}

.content-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.25fr) minmax(0, 0.75fr);
  gap: clamp(40px, 5vw, 80px);
  align-items: start;

  @media (max-width: 960px) {
    grid-template-columns: 1fr;
  }
}

.block-head {
  margin-bottom: 28px;

  h2 {
    font-size: clamp(1.8rem, 3vw, 2.4rem);
    margin-top: 14px;
  }
}

.rating-summary {
  margin-top: 10px;
  font-family: $font-mono;
  font-size: 0.78rem;
  color: $color-muted;

  .star {
    color: $color-accent;
  }
}

.module-list {
  list-style: none;
  margin: 0;
  padding: 0;
  border-top: 1px solid $color-ink;
}

.module-item {
  padding: 22px 0 18px;
  border-bottom: 1px solid $color-hairline;
}

.module-header {
  display: grid;
  grid-template-columns: 52px minmax(0, 1fr) auto;
  align-items: baseline;
  gap: 12px;
  margin-bottom: 10px;

  .module-num {
    font-family: $font-display;
    font-weight: 600;
    font-size: 1.5rem;
    line-height: 1;
    color: $color-accent;
  }

  h3 {
    font-size: 1.25rem;
    font-weight: 500;
  }

  .module-count {
    @include mono-label;
    font-size: 0.62rem;
    color: $color-faint;
  }
}

.lessons-list {
  display: flex;
  flex-direction: column;
  padding-left: 64px;

  @media (max-width: 560px) {
    padding-left: 0;
  }
}

.lesson-item {
  display: flex;
  align-items: baseline;
  gap: 14px;
  padding: 10px 6px;
  font-size: 0.94rem;
  color: $color-ink;
  border-top: 1px dashed $color-hairline;

  &:first-child {
    border-top: none;
  }

  .lesson-num {
    width: 30px;
    flex-shrink: 0;
    font-family: $font-mono;
    font-size: 0.7rem;
    color: $color-faint;
  }

  .lesson-title {
    flex: 1;
    min-width: 0;
  }

  .lesson-state {
    flex-shrink: 0;
    font-family: $font-mono;
    font-size: 0.8rem;
    color: $color-accent;
    align-self: center;
  }

  .lesson-duration {
    flex-shrink: 0;
    font-family: $font-mono;
    font-size: 0.72rem;
    color: $color-faint;
  }
}

.lesson-item.link {
  text-decoration: none;
  border-radius: 6px;
  transition: background 0.2s, color 0.2s;

  &:hover {
    background: rgba($color-ink, 0.05);

    .lesson-title {
      color: $color-accent;
    }
  }
}

.lesson-item.done .lesson-title {
  color: $color-muted;
}

.lesson-item.locked {
  color: $color-faint;
  cursor: not-allowed;

  .lesson-state {
    color: $color-faint;
  }
}

.review-form {
  --lg-blur: 10px;
  padding: 26px;
  margin-bottom: 32px;

  &:hover {
    transform: none;
  }

  h3 {
    font-size: 1.15rem;
    font-weight: 500;
    margin-bottom: 18px;
  }

  .form-group {
    display: flex;
    flex-direction: column;
    gap: 8px;
    margin-bottom: 14px;

    label {
      @include mono-label;
      font-size: 0.62rem;
      color: $color-faint;
    }
  }

  textarea,
  select {
    border: none;
    border-radius: $radius-md;
    background: rgba(13, 15, 19, 0.45);
    box-shadow: inset 0 0 0 1px $color-hairline;
    color: $color-ink;
    font: inherit;
    outline: none;
    transition: box-shadow 0.2s;

    &:focus {
      box-shadow:
        inset 0 0 0 1px $color-hairline,
        0 0 0 2px rgba($color-accent, 0.6);
    }
  }

  textarea {
    width: 100%;
    padding: 12px 14px;
    resize: vertical;

    &::placeholder {
      color: $color-faint;
    }
  }

  select {
    width: min(100%, 190px);
    min-height: 44px;
    padding: 9px 13px;
    cursor: pointer;

    option {
      background: $color-bg;
      color: $color-ink;
    }
  }

  .field-error {
    color: $color-danger;
    font-size: 0.82rem;
  }

  .edit-actions {
    display: flex;
    gap: 12px;
    flex-wrap: wrap;
  }
}

.reviews-list {
  display: flex;
  flex-direction: column;
  border-top: 1px solid $color-ink;
}

.review-card {
  margin: 0;
  padding: 24px 0 26px;
  border-bottom: 1px solid $color-hairline;

  .review-head {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    margin-bottom: 14px;
  }

  .stars {
    font-size: 0.8rem;
    letter-spacing: 0.18em;
    color: $color-accent;
  }

  .stars-empty {
    color: $color-hairline;
  }

  blockquote {
    position: relative;
    margin: 0 0 16px;
    padding-left: 28px;
    font-family: $font-display;
    font-size: 1.06rem;
    font-weight: 400;
    line-height: 1.5;
    color: $color-ink;

    &::before {
      content: "“";
      position: absolute;
      left: 0;
      top: -0.12em;
      font-size: 2.2rem;
      line-height: 1;
      color: $color-accent;
    }
  }

  .review-author {
    padding-left: 28px;
    font-family: $font-mono;
    font-size: 0.72rem;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    color: $color-faint;
  }

  .own-review-actions {
    display: flex;
    gap: 14px;
  }

  .btn-link {
    @include mono-label;
    background: none;
    border: none;
    padding: 0;
    font-size: 0.62rem;
    color: $color-sky;
    cursor: pointer;

    &:hover {
      text-decoration: underline;
    }

    &:disabled {
      color: $color-faint;
      cursor: not-allowed;
    }
  }

  .btn-link-danger {
    color: $color-danger;
  }
}

.loading {
  padding: 64px 0 80px;
  text-align: center;
  font-family: $font-mono;
  font-size: 0.8rem;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: $color-faint;
}

@media (max-width: 560px) {
  .checkout-actions .btn,
  .inline-field .btn {
    flex-basis: 100%;
  }

  .subscription-access {
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>
