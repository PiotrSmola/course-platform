<template>
  <div class="instructor-dashboard">
    <h1>Instructor Dashboard</h1>
    <router-link :to="{ name: 'NewCourse' }">Create New Course</router-link>
  </div>
</template>

<style lang="scss" scoped>
.instructor-dashboard {
  padding: 2rem;
}
</style>
