import { defineNuxtPlugin } from '#app';
import { useAuthStore } from '~/stores/useAuthStore';

export default defineNuxtPlugin(() => {
  const authStore = useAuthStore();
  authStore.initialize();
});