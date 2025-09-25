import { createRouter, createWebHistory } from "vue-router";
import type { NavigationGuardNext, RouteLocationNormalized } from "vue-router";
import LoginView from "../views/LoginView.vue";
import HomeView from "../views/HomeView.vue";
import { checkAuthentication } from "@/utils/auth";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "home",
      component: HomeView,
      meta: { requiresAuth: true },
      redirect: "/zapisi",
      children: [
        {
          path: "zapisi",
          name: "zapisi",
          component: HomeView,
          meta: { requiresAuth: true },
        },
        {
          path: "linije",
          name: "linije",
          component: HomeView,
          meta: { requiresAuth: true },
        },
        {
          path: "pogodbe",
          name: "pogodbe",
          component: HomeView,
          meta: { requiresAuth: true },
        },
      ],
    },
    {
      path: "/login",
      name: "login",
      component: LoginView,
      beforeEnter: async (
        to: RouteLocationNormalized,
        from: RouteLocationNormalized,
        next: NavigationGuardNext
      ) => {
        try {
          const isAuthenticated = await checkAuthentication();
          if (isAuthenticated) {
            next(from.name ? { name: from.name } : "/");
          } else {
            next();
          }
        } catch (error) {
          console.error(error);
          next();
        }
      },
    },
    // Redirect to login if not found
    {
      path: "/:pathMatch(.*)*",
      redirect: "/login",
    },
  ],
});

// Global navigation guard for protected routes
router.beforeEach(
  async (to: RouteLocationNormalized, from: RouteLocationNormalized, next: NavigationGuardNext) => {
    if (to.meta.requiresAuth) {
      try {
        const isAuthenticated = await checkAuthentication();
        if (!isAuthenticated) {
          next("/login");
          return;
        }
      } catch (error) {
        console.error("Authentication check failed:", error);
        next("/login");
        return;
      }
    }
    next();
  }
);

export default router;
