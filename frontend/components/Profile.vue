<template>
  <div class="profile-page">
    <div class="user-info">
      <UAvatar 
        size="3xl" 
        :alt=usernameFirstLetter
        :src="profilePicture" 
        class="avatar"
      />
      <h1 class="username">{{ authStore.user?.username }}</h1>
      <p class="email">E-mail: {{ authStore.user?.email }}</p>

      <button @click="editProfile" class="edit-button">Edit Profile</button>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '~/stores/useAuthStore';
import { useCookie } from '#app';

const authStore = useAuthStore();
const router = useRouter();

// Use the same key to retrieve the cookie value
const profilePictureKey = computed(() => `profilePicture_${authStore.user?.username}`);
const profilePictureCookie = useCookie(profilePictureKey.value);

// Reactive profile picture
const profilePicture = computed(() => {
  return profilePictureCookie.value || '';
});

const usernameFirstLetter = computed(() => {
  return authStore.user?.username?.charAt(0).toUpperCase();
});

// Navigate to edit profile page
const editProfile = () => {
  router.push('/edit-profile');
};

onMounted(() => {
  authStore.initialize();
})
</script>


<style scoped>
  .profile-page {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100vh;
    background-color: #f3f4f6;
    color: #333;
    padding: 20px;
  }
  
  .user-info {
    text-align: center;
    background: #fff;
    padding: 30px;
    border-radius: 8px;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    width: 100%;
    max-width: 400px;
  }

  .username {
    font-size: 1.2em;
    font-weight: bold;
    color: #333;
    margin-bottom: 0.5em;
  }
  
  .email {
    font-size: 1.2em;
    color: #666;
    margin-bottom: 1em;
  }
  
  .edit-button {
    padding: 10px 20px;
    font-size: 1em;
    color: #fff;
    background-color: #007bff;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    transition: background-color 0.3s;
  }
  
  .edit-button:hover {
    background-color: #0056b3;
  }
  .text-button {
  background: none;
  border: none;
  color: #007bff;
  cursor: pointer;
  text-decoration: underline;
  padding: 0;
  font: inherit; 
}

.text-button:hover {
  color: #0056b3;
  background: none;
}
</style>  