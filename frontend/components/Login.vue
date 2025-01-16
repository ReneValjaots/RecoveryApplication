<template>
  <div class="login-container">
    <form @submit.prevent="loginUser">
      <div class="form-group">
        <label for="usernameOrEmail">Username or Email</label>
        <input v-model="usernameOrEmail" type="text" id="usernameOrEmail" class="input-field" required />
      </div>
      <div class="form-group">
        <label for="password">Password</label>
        <input v-model="password" type="password" id="password" class="input-field" required />
      </div>
      <button type="submit">Login</button>
    </form>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '~/stores/useAuthStore';

const usernameOrEmail = ref('');
const password = ref('');
const router = useRouter();
const authStore = useAuthStore();

const loginUser = async () => {
  try {
    await authStore.login(usernameOrEmail.value, password.value);
    router.push('/');
  } catch (error) {
    console.error('Login failed');
  }
};
</script>

<style scoped>
.login-container {
  max-width: 400px;
  margin: 20px auto;
  padding: 20px;
  border: 1px solid #ccc;
  border-radius: 8px;
}

.form-group {
  margin-bottom: 15px;
  display: flex;
  flex-direction: column;
}

.label {
  margin-bottom: 5px;
  font-weight: bold;
}

.input-field {
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 4px;
  width: 100%;
  margin-top: 5px;
  margin-bottom: 5px;
}

button {
  background-color: #28a745;
  color: white;
  padding: 10px;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  width: 100%;
}

button:hover {
  background-color: #218838;
}

.error {
  color: red;
  margin-top: 10px;
}
</style>