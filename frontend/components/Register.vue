<template>
  <div class="register-container">
    <form @submit.prevent="registerUser">
      <div class="form-group">
        <label for="username">Username</label>
        <input v-model="username" type="text" class="input-field" required />
      </div>
      <div class="form-group">
        <label for="email">Email</label>
        <input v-model="email" type="email" class="input-field" required />
      </div>
      <div class="form-group">
        <label for="password">Password</label>
        <input
          v-model="password"
          type="password"
          class="input-field"
          @input="validatePassword"
          required
        />
        <p v-if="passwordError" class="error">{{ passwordError }}</p>
      </div>
      <div class="mb-4">
        <p>
          Already have an account? 
          <a @click.prevent="navigateTo('/login')" class="text-button">Login</a>
        </p>
        <p>
          Are you a doctor? 
          <a @click.prevent="toggleDoctorMode" class="text-button">Enter key</a>
        </p>
      </div>
      <div v-if="isDoctorMode" class="form-group">
        <label for="secret-key">Secret Key</label>
        <input
          v-model="secretKey"
          type="password"
          class="input-field"
          placeholder="Enter doctor secret key"
          required
        />
        <p v-if="secretKeyError" class="error">{{ secretKeyError }}</p>
      </div>
      <button type="submit" :disabled="isButtonDisabled">
        {{ isLoading ? 'Registering...' : 'Register' }}
      </button>
    </form>
    <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '~/stores/useAuthStore';

const username = ref('');
const email = ref('');
const password = ref('');
const secretKey = ref('');
const passwordError = ref('');
const secretKeyError = ref('');
const errorMessage = ref('');
const isDoctorMode = ref(false);
const isLoading = ref(false);
const authStore = useAuthStore();
const router = useRouter();

const isButtonDisabled = computed(() =>
  isLoading.value || !!passwordError.value || (isDoctorMode.value && !secretKey.value.trim())
);

const validatePassword = () => {
  const passwordRegex = /^(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{6,}$/;
  passwordError.value = passwordRegex.test(password.value)
    ? ''
    : 'Password must contain: a capital letter, a number, and at least 6 characters.';
};

const toggleDoctorMode = () => {
  isDoctorMode.value = !isDoctorMode.value;
};

const registerUser = async () => {
  if (passwordError.value || (isDoctorMode.value && !secretKey.value.trim())) return;

  try {
    isLoading.value = true;

    if (isDoctorMode.value) {
      await authStore.registerDoctor(username.value, email.value, password.value, secretKey.value);
    } else {
      await authStore.register(username.value, email.value, password.value);
    }

    router.push('/');
  } catch (error) {
    isLoading.value = false;

    if (isDoctorMode.value && error.message === 'Invalid secret key') {
      secretKeyError.value = 'Invalid secret key. Please try again.';
    } else {
      errorMessage.value = 'Registration failed. Please try again.';
    }
  } finally {
    isLoading.value = false;
  }
};
</script>

<style scoped>
.register-container {
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

button:disabled {
  background-color: #6c757d;
  cursor: not-allowed;
}

.error {
  color: red;
  margin-top: 10px;
}
.text-button {
  background: none;
  border: none;
  color: #007bff;
  cursor: pointer;
  text-decoration: underline;
  font: inherit; 
}

.text-button:hover {
  color: #0056b3;
  background: none;
}
</style>
