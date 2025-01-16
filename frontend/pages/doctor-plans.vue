<template><DoctorPlans /></template>
<script setup lang="ts">
    definePageMeta({
        middleware: 'auth',
        requiresDoctor: true,
    });
</script>