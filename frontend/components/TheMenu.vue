<template>
  <div class="navigation-container">
    <!-- Main navigation links -->
    <UHorizontalNavigation :links="activeLinks" class="border-b border-gray-200 dark:border-gray-800" />

    <div class="user-navigation">
      <UHorizontalNavigation :links="userLinks" class="border-b border-gray-200 dark:border-gray-800" />
    </div>
    
    <UAlert  
      v-if="alertVisible"
      :title="alertTitle"
      :description="alertMessage"
      :close-button="{ icon: 'i-heroicons-x-mark-20-solid', color: 'gray', variant: 'link', padded: false }"
      @close="alertVisible = false"
      color="blue"
      variant="subtle"
      class="centered-alert"
    />
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '~/stores/useAuthStore';

const router = useRouter();
const authStore = useAuthStore();

const alertVisible = ref(false);
const alertTitle = ref('');
const alertMessage = ref('');

const links = [
  { label: "Home", to: "/" },
  { label: "My Recovery Plans", to: "/landingpage" },
  { label: "Injury List", to: "/injuries" },
  { label: "My Injuries", to: "/add-injury" },
  { label: "Recovery Exercises", to: "/recovery-exercises" },
  { label: "Injury Statistics", to: "/statistics" }
];

const userLinks = computed(() => {
  if (authStore.isAuthenticated) {
    return [
      { label: "Profile", to: "/profile" },
      { label: "Logout", click: handleLogout }
    ];
  } else {
    return [
      { label: "Login", to: "/login" },
      { label: "Register", to: "/register" }
    ];
  }
});

const doctorLinks = [
  { label: "Home", to: "/" },
  { label: "Patients", to: "/patients" },
  { label: "Create Recovery Plan", to: "/doctor-create-plan" },
  { label: "Patient's Recovery Plans", to: "/doctor-plan-landing" },
];

const activeLinks = computed(() => {
  if (authStore.isAuthenticated && authStore.isDoctor) {
    return [...doctorLinks];
  }
  return links;
});

const showAlert = (title, message) => {
  alertTitle.value = title;
  alertMessage.value = message;
  alertVisible.value = true;
  setTimeout(() => {
    alertVisible.value = false;
  }, 2000);
};

const handleLogout = () => {
  authStore.logout();
  router.push('/');
  showAlert("Success", "You have logged out successfully.");
};

watch(
  () => authStore.isAuthenticated,
  (isAuthenticated) => {
    if (isAuthenticated) {
      showAlert("Welcome!", "You are now logged in.");
    }
  }
);
</script>

<style scoped>
.navigation-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.main-navigation {
  flex: 1; 
}

.user-navigation {
  display: flex;
  justify-content: flex-end;
}

.centered-alert {
  position: fixed;
  top: 20px;
  left: 50%;
  transform: translateX(-50%);
  max-width: 300px;
  z-index: 1000;
}
</style>