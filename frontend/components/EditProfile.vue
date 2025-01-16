<template>
  <div class="edit-profile-page">
    <div class="user-info">
      <UAvatar 
        size="3xl" 
        :src="profilePicture || initialsAvatar" 
        alt="Avatar" 
        class="avatar"
      />

      <div class="profile-picture-options">
        <h3>Select a profile picture:</h3>
        <div class="picture-grid">
          <img 
            v-for="picture in predefinedPictures" 
            :key="picture" 
            :src="picture" 
            class="profile-option" 
            @click="selectPredefinedPicture(picture)" 
          />
        </div>
      </div>

      <button @click="saveProfile" class="save-button">Save Changes</button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '~/stores/useAuthStore';
import { useCookie } from '#app';

// Store and router
const authStore = useAuthStore();
const router = useRouter();

// Use a computed property for the cookie key based on the username or email
const profilePictureKey = computed(() => `profilePicture_${authStore.user?.username || 'guest'}`);

// Reactive profile picture cookie
const profilePictureCookie = useCookie(profilePictureKey.value, { sameSite: 'strict' });

// Reactive profile picture state
const profilePicture = ref('');

// List of predefined profile pictures
const predefinedPictures = [
  '/img/profile/monalisa.jpg',
  '/img/profile/sea.jpg',
  '/img/profile/davinci.jpg',
  '/img/profile/irises.jpg',
  '/img/profile/kiss.jpg',
  '/img/profile/renoir.jpg',
];

// Generate initials-based avatar
const initialsAvatar = computed(() => {
  const name = authStore.user?.username || 'U';
  const initials = name
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase();
  const bgColor = '#007bff';
  const textColor = '#fff';

  return `https://ui-avatars.com/api/?name=${initials}&background=${bgColor.replace(
    '#',
    ''
  )}&color=${textColor.replace('#', '')}`;
});

// Select predefined profile picture
const selectPredefinedPicture = (picture) => {
  profilePicture.value = picture; // Update selected picture
};

// Load existing profile picture
onMounted(() => {
  profilePicture.value = authStore.user?.profilePicture || profilePictureCookie.value || '';
});

// Save the profile picture
const saveProfile = async () => {
  const finalProfilePicture = profilePicture.value || initialsAvatar.value;

  // Update the store
  authStore.user.profilePicture = finalProfilePicture;

  // Save to the account-specific cookie
  profilePictureCookie.value = finalProfilePicture;

  // Navigate back to the profile page
  router.push('/profile');
};
</script>

<style scoped>
.edit-profile-page {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  background-color: #f9fafb;
  padding: 20px;
  font-family: 'Arial', sans-serif;
}

.user-info {
  background: #ffffff;
  padding: 30px;
  border-radius: 12px;
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 500px;
  text-align: center;
}

.avatar {
  margin-bottom: 1.5rem;
  width: 100px;
  height: 100px;
  object-fit: cover;
  border-radius: 50%;
  border: 2px solid #ddd;
}

.profile-picture-options {
  margin-top: 1.5rem;
}

.picture-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(60px, 1fr));
  gap: 10px;
  justify-content: center;
}

.profile-option {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  cursor: pointer;
  object-fit: cover;
  transition: transform 0.3s, box-shadow 0.3s;
}

.profile-option:hover {
  transform: scale(1.1);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
}

.save-button {
  margin-top: 20px;
  padding: 12px 20px;
  background-color: #38a169;
  color: white;
  font-size: 16px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

.save-button:hover {
  background-color: #2f855a;
}
</style>
