<template> 
  <LandingPage/>
</template>

<script setup lang="ts">
  definePageMeta({
      middleware: 'auth'
  });
</script> 