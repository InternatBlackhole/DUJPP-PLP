<template>
  <div class="home-container">
    <header class="home-header">
      <div class="header-content">
        <div>
          <h1>Welcome to PLP</h1>
          <p v-if="userInfo" class="user-info">Logged in as: {{ userInfo.email }}</p>
        </div>
        <button @click="handleLogout" class="logout-button">Logout</button>
      </div>
      <nav class="main-nav">
        <ul>
          <li><RouterLink to="/zapisi">Records</RouterLink></li>
          <li><RouterLink to="/linije">Lines</RouterLink></li>
          <li><RouterLink to="/pogodbe">Contracts</RouterLink></li>
        </ul>
      </nav>
    </header>

    <main class="home-content">
      <div class="tab-content">
        <component :is="currentView" />
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { RouterLink, useRouter, useRoute } from "vue-router";
import { useApi } from "@/composables/useApi";
import { useUserInfo } from "@/utils/auth";
import { firstValueFrom } from "rxjs";
import { computed } from "vue";
import ZapisiView from "@/components/ZapisiView.vue";
import LinijeView from "@/components/LinijeView.vue";
import PogodbeView from "@/components/PogodbeView.vue";

const router = useRouter();
const route = useRoute();
const { auth } = useApi();
const userInfo = useUserInfo();

// Map routes to components
const viewComponents = {
  zapisi: ZapisiView,
  linije: LinijeView,
  pogodbe: PogodbeView,
} as const;

// Compute the current component based on route
const currentView = computed(() => {
  const path = route.path.split("/").pop() || "zapisi";
  return viewComponents[path as keyof typeof viewComponents] || ZapisiView;
});

const handleLogout = async () => {
  try {
    await firstValueFrom(auth.authLogoutPost());
    userInfo.value = null; // Clear the cached user info
    router.push("/login");
  } catch (error) {
    console.error("Logout failed:", error);
  }
};
</script>

<style scoped>
.home-container {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.home-header {
  margin-bottom: 2rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #eee;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.home-header h1 {
  margin-bottom: 0.5rem;
  color: #333;
}

.user-info {
  color: #666;
  font-size: 0.9rem;
  margin: 0;
}

.logout-button {
  background-color: #dc3545;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
}

.logout-button:hover {
  background-color: #c82333;
}

.main-nav ul {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  gap: 1.5rem;
}

.main-nav a {
  text-decoration: none;
  color: #2c3e50;
  font-weight: 500;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  transition: background-color 0.2s;
}

.main-nav a:hover {
  background-color: #f5f5f5;
}

.main-nav a.router-link-active {
  color: #42b883;
  background-color: #f0f9f4;
}

.home-content {
  margin-top: 2rem;
}

.quick-actions {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.quick-actions h2 {
  margin-top: 0;
  margin-bottom: 1rem;
  color: #2c3e50;
}
</style>
