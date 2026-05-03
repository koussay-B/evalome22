import { defineStore } from 'pinia';
import { ref, watchEffect } from 'vue';
import { usePreferredDark } from '@vueuse/core';

export type Theme = 'light' | 'dark' | 'system';

export const useThemeStore = defineStore('theme', () => {
    const isPreferredDark = usePreferredDark();

    // Safely parse local storage or fallback to system
    const getInitialTheme = (): Theme => {
        const stored = localStorage.getItem('theme');
        if (stored === 'light' || stored === 'dark' || stored === 'system') {
            return stored as Theme;
        }
        return 'system';
    };

    // Initialize theme from localStorage or default to 'system'
    const state = ref({
        theme: getInitialTheme(),
        lastAction: '',
    });

    // Apply the theme to the document when it changes
    watchEffect(() => {
        const root = window.document.documentElement;

        // Save the chosen theme preference
        localStorage.setItem('theme', state.value.theme);

        let effectiveTheme = state.value.theme;
        if (state.value.theme === 'system') {
            // Dynamically follow OS preference changes
            effectiveTheme = isPreferredDark.value ? 'dark' : 'light';
        }

        if (effectiveTheme === 'dark') {
            root.classList.add('dark');
        } else {
            root.classList.remove('dark');
        }
    });

    // "Dispatch" function to handle actions
    function dispatch(action: { type: string; payload?: any }) {
        state.value.lastAction = action.type;

        switch (action.type) {
            case 'SET_THEME':
                if (['light', 'dark', 'system'].includes(action.payload)) {
                    state.value.theme = action.payload;
                } else {
                    console.warn(`Invalid theme payload: ${action.payload}`);
                }
                break;
            case 'CYCLE_THEME':
                if (state.value.theme === 'light') {
                    state.value.theme = 'dark';
                } else if (state.value.theme === 'dark') {
                    state.value.theme = 'system';
                } else {
                    state.value.theme = 'light';
                }
                break;
            default:
                console.warn(`Unknown action type: ${action.type}`);
        }
    }

    return {
        state,
        dispatch,
    };
});
