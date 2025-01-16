export default defineNuxtRouteMiddleware((to)=>{
  if (import.meta.client){
    const authStore = useAuthStore();
    authStore.initialize();

    if (!authStore.isAuthenticated) {
      return navigateTo('/register');
    }

    if (to.meta.requiresDoctor && !authStore.isDoctor) {
      return navigateTo('/unauthorized', {replace: true});
    }
  }
});