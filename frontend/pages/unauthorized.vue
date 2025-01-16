<template>
    <h1>Access Denied</h1>
    <p>You do not have permission to view this page.</p>
    <UButton @click=navigateHome()>Return to Home</UButton>
</template>

<script setup lang="ts">
const router = useRouter();
const navigateHome = () => router.push('/');
</script>