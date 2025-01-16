<template>
    <AddInjury title="My Injuries" />
</template>

<script setup lang="ts">
    definePageMeta({
        middleware: 'auth'
    });
</script>
  