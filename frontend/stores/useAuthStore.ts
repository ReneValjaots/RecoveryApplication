import { defineStore } from 'pinia';
import { useCookie } from '#app';
import { jwtDecode } from "jwt-decode";

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as { username: string, email: string } | null,
    token: null as string | null,
    isAuthenticated: false,
    isDoctor: false,
  }),
  actions: {
    initialize() {
      if (this.user && this.token) return;

      if (import.meta.client) {
        const tokenCookie = useCookie('token', {
          secure: true,
          sameSite: 'strict',
          path: '/',
        });

        const token = tokenCookie.value || null;
        this.token = token;
        this.isAuthenticated = !!token;

        if (token) {
          try {
            const decodedToken = jwtDecode<{ given_name: string; email: string; role: string }>(token);
            this.user = { username: decodedToken.given_name, email: decodedToken.email };
            this.isDoctor = decodedToken.role === 'Doctor';
          }
          catch {
            console.error('Failed to decode token');
          }
        }
      }
    },
    async register(username: string, email: string, password: string) {
      const { customFetch } = useApi();
      try {
        const response = await customFetch<{ username: string, email: string, token: string}>('account/register', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json-patch+json',
          },
          body: { username, email, password},
        });

        this.setAuthState(response.token, response.username, response.email);
      } catch (error) {
        console.error('Registration failed:', error);
      }
    },
    
    async registerDoctor(username: string, email: string, password: string, secretKey: string) {
      const { customFetch } = useApi();
      try {
        const response = await customFetch<{ username: string; email: string; token: string }>(
          'account/register/doctor', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify({ username, email, password, secretKey }),
          }
        );
    
        if (!response.token) {
          throw new Error('Invalid secret key'); 
        }
    
        this.setAuthState(response.token, response.username, response.email);
      } catch (error) {
        console.error('Doctor registration failed:', error);
    
        throw error;
      }
    },    

    async login(usernameOrEmail: string, password: string) {
      const { customFetch } = useApi();
      try {
        const isEmail = usernameOrEmail.includes('@');
        const body = isEmail 
          ? { email: usernameOrEmail, username: "", password} 
          : { username: usernameOrEmail, email: "", password };
        const response = await customFetch<{ username: string, email: string, token: string}>('account/login', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json-patch+json',
          },
          body,
        });
        this.setAuthState(response.token, response.username, response.email);
      } catch (error) {
        console.error('Login failed:', error);
      }
    },

    logout() {
      this.token = null;
      this.user = null;
      this.isAuthenticated = false;
      this.isDoctor = false;

      useCookie('token').value = null;
    },

    setAuthState(token: string, username: string, email: string) {
      const tokenCookie = useCookie('token', {
        secure: true,
        sameSite: 'strict',
        path: '/',
      });

      this.token = token;
      this.user = { username, email };
      this.isAuthenticated = true;
      tokenCookie.value = token;

      try {
        const decodedToken = jwtDecode<{ role: string }>(token);
        this.isDoctor = decodedToken.role === 'Doctor';
      } catch {
        console.error('Failed to decode token');
      }
    },
  },
});
