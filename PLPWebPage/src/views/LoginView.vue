<template>
  <div class="login-container">
    <form @submit.prevent="handleSubmit" class="login-form">
      <h2>Prijava</h2>

      <div class="form-group">
        <label for="email">Email</label>
        <input
          type="email"
          id="email"
          v-model="email"
          required
          pattern="[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}"
          title="Please enter a valid email address"
          placeholder="Vnesite email"
        />
      </div>

      <div class="form-group">
        <label for="password">Geslo</label>
        <input
          type="password"
          id="password"
          v-model="password"
          required
          placeholder="Vnesite geslo"
        />
      </div>

      <div class="form-group remember-me">
        <input type="checkbox" id="remember" v-model="rememberSession" />
        <label for="remember">Zapomni si me</label>
      </div>

      <div v-if="error" class="error-message">{{ error }}</div>
      <button type="submit">Prijava</button>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useApi } from "@/composables/useApi";
import router from "@/router";
import { throwError, timeout } from "rxjs";

const { auth } = useApi();
const email = ref("");
const password = ref("");
const error = ref("");
const rememberSession = ref(false);

const handleSubmit = () =>
  auth
    .authLoginPost({
      loginRequest: {
        email: email.value,
        password: password.value,
      },
      useCookies: true,
      useSessionCookies: rememberSession.value
    })
    .pipe(
      timeout({
        first: 5000,
        with: () => throwError(() => new Error("login request taking too long!")),
      })
    )
    .subscribe({
      complete() {
        // Store user info if needed
        console.log("Login successful");
        // Redirect to home or dashboard
        router.push("/");
      },
      error(err) {
        error.value = err.response;
        console.error("Login error:", err);
      },
    });
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  overflow: hidden;
}

.login-form {
  width: 100%;
  max-width: 400px;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  background-color: white;
}

.form-group {
  margin-bottom: 20px;
}

.remember-me {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-left: 4px;
}

.remember-me input[type="checkbox"] {
  width: auto;
  margin: 0;
}

.remember-me label {
  display: inline;
  margin: 0;
  cursor: pointer;
}

label {
  display: block;
  margin-bottom: 5px;
  font-weight: 500;
}

input {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 16px;
  box-sizing: border-box;
}

button {
  width: 100%;
  padding: 10px;
  background-color: #4caf50;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 16px;
  cursor: pointer;
}

button:hover {
  background-color: #45a049;
}

.error-message {
  color: #dc3545;
  margin-bottom: 15px;
  font-size: 14px;
  text-align: center;
}
</style>
