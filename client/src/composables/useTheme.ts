import { computed } from 'vue';
import { useThemeStore, type Theme } from '@/store/themeStore';

export function useTheme() {
    const themeStore = useThemeStore();

    // Create a computed property to maintain reactivity
    const theme = computed(() => themeStore.state.theme as Theme);

    const toggleTheme = () => {
        themeStore.dispatch({ type: 'CYCLE_THEME' });
    };

    const setTheme = (newTheme: Theme) => {
        themeStore.dispatch({ type: 'SET_THEME', payload: newTheme });
    };

    return {
        theme,
        toggleTheme,
        setTheme,
    };
}
