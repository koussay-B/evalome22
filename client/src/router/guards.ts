import type { NavigationGuardNext, RouteLocationNormalized } from 'vue-router';
import { useAuthStore } from '@/store/authStore';

export const authGuard = async (
    to: RouteLocationNormalized,
    _from: RouteLocationNormalized,
    next: NavigationGuardNext
) => {
    const authStore = useAuthStore();

    // If user is not fetched yet, try to fetch it
    if (authStore.isAuthenticated && !authStore.state.user) {
        await authStore.getUser();
    }

    const requiresAuth = to.matched.some(record => record.meta.requiresAuth);
    const requiresAdmin = to.matched.some(record => record.meta.requiresAdmin);

    if (requiresAuth && !authStore.isAuthenticated) {
        next({ name: 'Login', query: { redirect: to.fullPath } });
    } else if (requiresAdmin && !authStore.isAdmin) {
        next({ name: 'Forbidden' });
    } else if (authStore.isAuthenticated && to.name === 'Login') {
        next({ name: 'Home' });
    } else {
        next();
    }
};
