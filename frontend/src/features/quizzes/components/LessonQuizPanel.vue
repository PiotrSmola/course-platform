<template>
  <section v-if="quiz" class="lesson-quiz glass">
    <div class="quiz-head">
      <h2>{{ quiz.title }}</h2>
      <p>Próg zaliczenia: {{ quiz.passThresholdPercent }}%</p>
    </div>

    <form v-if="!result" class="quiz-form" @submit.prevent="onSubmit">
      <fieldset v-for="(question, qIndex) in quiz.questions" :key="question.id" class="question">
        <legend><span class="q-no">{{ String(qIndex + 1).padStart(2, '0') }}</span>{{ question.prompt }}</legend>
        <label v-for="option in question.options" :key="option.id" class="option">
          <input
            v-model="selections[question.id]"
            type="radio"
            :name="question.id"
            :value="option.id"
          />
          <span>{{ option.text }}</span>
        </label>
      </fieldset>
      <button
        class="btn btn-primary"
        type="submit"
        :disabled="submitAttempt.isPending.value || !allAnswered"
      >
        Oddaj quiz
      </button>
    </form>

    <div v-else class="result" :class="{ passed: result.passed, failed: !result.passed }">
      <h3>{{ result.passed ? 'Zaliczone' : 'Niezaliczone' }}</h3>
      <p>Wynik: {{ result.scorePercent }}%</p>
      <button type="button" class="btn btn-ghost" @click="retry">Spróbuj ponownie</button>
    </div>

    <div v-if="attempts.length" class="attempts">
      <h3>Twoje próby</h3>
      <ul>
        <li v-for="attempt in attempts" :key="attempt.id">
          {{ formatDate(attempt.submittedAt) }} —
          {{ attempt.scorePercent }}%
          ({{ attempt.passed ? 'zaliczone' : 'niezaliczone' }})
        </li>
      </ul>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import {
  useLessonQuiz,
  useMyQuizAttempts,
  useSubmitQuizAttempt
} from '@/features/quizzes/composables/useQuizzes'
import type { QuizAttemptResultDto } from '@/features/quizzes/types/quiz.types'

const props = defineProps<{
  courseId: string
  lessonId: string
}>()

const quizQuery = useLessonQuiz(() => props.courseId, () => props.lessonId)
const attemptsQuery = useMyQuizAttempts(() => props.courseId, () => props.lessonId)
const submitAttempt = useSubmitQuizAttempt(() => props.courseId, () => props.lessonId)

const quiz = computed(() => quizQuery.data.value)
const attempts = computed(() => attemptsQuery.data.value ?? [])
const selections = reactive<Record<string, string>>({})
const result = ref<QuizAttemptResultDto | null>(null)

watch(
  () => props.lessonId,
  () => {
    Object.keys(selections).forEach((key) => delete selections[key])
    result.value = null
  }
)

const allAnswered = computed(() => {
  if (!quiz.value) return false
  return quiz.value.questions.every((q) => !!selections[q.id])
})

async function onSubmit() {
  if (!quiz.value) return
  const answers = quiz.value.questions.map((q) => ({
    questionId: q.id,
    selectedOptionId: selections[q.id]
  }))
  result.value = await submitAttempt.mutateAsync(answers)
}

function retry() {
  result.value = null
  Object.keys(selections).forEach((key) => delete selections[key])
}

function formatDate(value: string) {
  return new Date(value).toLocaleString('pl-PL', {
    dateStyle: 'medium',
    timeStyle: 'short'
  })
}
</script>

<style lang="scss" scoped>
.lesson-quiz {
  --lg-r: #{$radius-card};
  --lg-blur: 12px;
  --lg-tint: rgba(13, 15, 19, 0.28);
  padding: 28px;
  display: flex;
  flex-direction: column;
  gap: 22px;

  h2,
  h3 {
    margin: 0;
  }
}

.quiz-head {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
  padding-bottom: 18px;
  border-bottom: 1px solid $color-hairline;

  h2 {
    font-size: 1.35rem;
    font-weight: 600;
  }

  p {
    @include tag;
    margin: 0;
  }
}

.quiz-form {
  display: flex;
  flex-direction: column;
  gap: 22px;
}

.question {
  margin: 0;
  padding: 0;
  border: none;
  display: flex;
  flex-direction: column;
  gap: 8px;

  legend {
    display: flex;
    gap: 12px;
    align-items: baseline;
    padding: 0;
    margin-bottom: 10px;
    font-weight: 600;
    line-height: 1.4;
  }
}

.q-no {
  font-family: $font-display;
  font-size: 1.05rem;
  font-weight: 600;
  color: $color-accent;
}

.option {
  display: flex;
  gap: 12px;
  align-items: center;
  padding: 11px 14px;
  border-radius: $radius-sm;
  box-shadow: inset 0 0 0 1px $color-hairline;
  color: $color-muted;
  cursor: pointer;
  transition: background 0.2s, box-shadow 0.2s, color 0.2s;

  input {
    accent-color: $color-accent;
  }

  &:hover {
    color: $color-ink;
    background: rgba($color-ink, 0.04);
  }

  &:has(input:checked) {
    color: $color-ink;
    background: rgba($color-accent, 0.08);
    box-shadow: inset 0 0 0 1px rgba($color-accent, 0.45);
  }
}

.result {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px 20px;
  padding: 4px 0 4px 18px;
  box-shadow: inset 2px 0 0 $color-accent;

  &.failed {
    box-shadow: inset 2px 0 0 $color-danger;
  }

  h3 {
    @include mono-label;
    color: $color-accent;
  }

  &.failed h3 {
    color: $color-danger;
  }

  p {
    margin: 0;
    font-family: $font-display;
    font-size: 1.6rem;
    font-weight: 600;
    line-height: 1;
  }

  .btn {
    margin-left: auto;
  }
}

.attempts {
  padding-top: 18px;
  border-top: 1px dashed $color-hairline;

  h3 {
    @include mono-label;
    color: $color-faint;
  }

  ul {
    list-style: none;
    margin: 10px 0 0;
    padding: 0;
    font-family: $font-mono;
    font-size: 0.76rem;
    color: $color-muted;

    li {
      padding: 6px 0;
      border-bottom: 1px solid $color-hairline;

      &:last-child {
        border-bottom: none;
      }
    }
  }
}

.btn {
  width: fit-content;
  padding: 11px 20px;
  font-size: 0.88rem;
}
</style>
