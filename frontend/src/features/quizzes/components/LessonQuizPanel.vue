<template>
  <section v-if="quiz" class="lesson-quiz glass">
    <div class="quiz-head">
      <h2>{{ quiz.title }}</h2>
      <p>Próg zaliczenia: {{ quiz.passThresholdPercent }}%</p>
    </div>

    <form v-if="!result" class="quiz-form" @submit.prevent="onSubmit">
      <fieldset v-for="question in quiz.questions" :key="question.id" class="question">
        <legend>{{ question.prompt }}</legend>
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
@use "@/assets/styles/abstracts/variables" as *;

.lesson-quiz {
  --lg-r: 28px;
  --lg-blur: 0px;
  padding: 28px;
  display: flex;
  flex-direction: column;
  gap: 20px;

  h2,
  h3 {
    margin: 0;
  }

  p {
    margin: 4px 0 0;
    color: $color-muted;
  }
}

.quiz-form {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.question {
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 16px;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 10px;

  legend {
    padding: 0 6px;
    font-weight: 600;
  }
}

.option {
  display: flex;
  gap: 10px;
  align-items: flex-start;
  cursor: pointer;
}

.result {
  border-radius: 16px;
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 10px;

  &.passed {
    background: rgba(34, 197, 94, 0.12);
  }

  &.failed {
    background: rgba(248, 113, 113, 0.12);
  }
}

.attempts ul {
  margin: 8px 0 0;
  padding-left: 18px;
  color: $color-muted;
}

.btn {
  width: fit-content;
  padding: 12px 18px;
}
</style>
