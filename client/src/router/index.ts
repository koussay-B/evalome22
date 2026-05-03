import { createRouter, createWebHistory } from 'vue-router';
import { authGuard } from './guards';

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    scrollBehavior(_to, _from, savedPosition) {
        return savedPosition || { top: 0 };
    },
    routes: [
        // ── Public (marketing) ────────────────────────────────────────────
        {
            path: '/',
            component: () => import('@/layouts/PublicLayout.vue'),
            children: [
                { path: '', name: 'Home', component: () => import('@/views/public/Home.vue') },
                { path: 'features', name: 'Features', component: () => import('@/views/public/Features.vue') },
                { path: 'pricing', name: 'Pricing', component: () => import('@/views/public/Pricing.vue') },
                { path: 'about', name: 'About', component: () => import('@/views/public/About.vue') },
                { path: 'company-request', name: 'CompanyRequest', component: () => import('@/views/public/CompanyRequest.vue') },
            ],
        },

        // ── Auth ──────────────────────────────────────────────────────────
        { path: '/login', name: 'Login', component: () => import('@/views/auth/Login.vue') },
        { path: '/register', redirect: '/company-request' },

        // ── Public certificate (shareable, no auth required) ──────────────
        { path: '/certificate/:code', name: 'PublicCertificate', component: () => import('@/views/candidat/CertificatePage.vue') },

        // ── Candidat ────────────────────────────────────────────────────────
        {
            path: '/candidat',
            component: () => import('@/layouts/CandidatLayout.vue'),
            // meta: { requiresAuth: true },
            children: [
                { path: '', name: 'CandidatSettings', component: () => import('@/views/user/Settings.vue') },
                { path: 'dashboard', name: 'CandidatDashboard', component: () => import('@/views/user/Home.vue') },
                { path: 'profile', name: 'CandidatProfile', component: () => import('@/views/user/Profile.vue') },
                { path: 'tests',              name: 'CandidatTests',          component: () => import('@/views/candidat/CandidatTests.vue') },
                { path: 'campaign/:id',       name: 'CandidatCampaignDetail', component: () => import('@/views/candidat/CandidatCampaignDetail.vue') },
                { path: 'test/:id',           name: 'CandidatTestTaking',     component: () => import('@/views/candidat/TestTaking.vue') },
                { path: 'result/:id',         name: 'CandidatTestResult',     component: () => import('@/views/candidat/TestResult.vue') },
                { path: 'certificates',       name: 'CandidatCertificates',   component: () => import('@/views/candidat/CandidatCertificates.vue') },
                { path: 'certificates/:code', name: 'CertificatePage',        component: () => import('@/views/candidat/CertificatePage.vue') },
            ],
        },

        // ── Formateur ───────────────────────────────────────────────────────
        {
            path: '/formateur',
            component: () => import('@/layouts/FormateurLayout.vue'),
            // meta: { requiresAuth: true },
            children: [
                { path: '', name: 'FormateurDashboard', component: () => import('@/views/formateur/Dashboard.vue') },
                { path: 'reports', name: 'FormateurReports', component: () => import('@/views/shared/Reports.vue') },
                { path: 'themes', name: 'FormateurThemes', component: () => import('@/views/formateur/Themes.vue') },
                { path: 'tests', name: 'FormateurTests', component: () => import('@/views/formateur/Tests.vue') },
                { path: 'questionnaires', name: 'FormateurQuestionnaires', component: () => import('@/views/shared/QuestionnaireLibrary.vue') },
                { path: 'tests/new', name: 'FormateurCampaignNew', component: () => import('@/views/shared/CampaignBuilder.vue') },
                { path: 'tests/edit/:id', name: 'FormateurCampaignEdit', component: () => import('@/views/shared/CampaignBuilder.vue'), props: { isEdit: true } },
                { path: 'questionnaire/:id', name: 'FormateurQuestionnaire', component: () => import('@/views/shared/QuestionnaireBuilder.vue') },
                { path: 'employees', name: 'FormateurEmployees', component: () => import('@/views/formateur/Employees.vue') },
                { path: 'settings', name: 'FormateurSettings', component: () => import('@/views/formateur/Settings.vue') },
            ],
        },

        // ── Admin ──────────────────────────────────────────────────────────
        {
            path: '/admin',
            component: () => import('@/layouts/AdminLayout.vue'),
            // meta: { requiresAuth: true, requiresAdmin: true },
            children: [
                { path: '', name: 'AdminDashboard', component: () => import('@/views/admin/Dashboard.vue') },
                { path: 'companies', name: 'AdminCompanies', component: () => import('@/views/admin/company/Companies.vue') },
                { path: 'company-verification', name: 'AdminCompanyVerification', component: () => import('@/views/admin/company/CompanyVerification.vue') },
                { path: 'users', name: 'AdminUsers', component: () => import('@/views/admin/users/Users.vue') },
                { path: 'analytics', name: 'AdminAnalytics', component: () => import('@/views/admin/Analytics.vue') },
                { path: 'settings', name: 'AdminSettings', component: () => import('@/views/admin/Settings.vue') },
            ],
        },

        // ── AdminCompany ───────────────────────────────────────────────────
        {
            path: '/company',
            component: () => import('@/layouts/AdminCompanyLayout.vue'),
            // meta: { requiresAuth: true, requiresAdminCompany: true },
            children: [
                { path: '', name: 'CompanyDashboard', component: () => import('@/views/admin-company/Dashboard.vue') },
                { path: 'employees', name: 'CompanyEmployees', component: () => import('@/views/admin-company/Employees.vue') },
                { path: 'analytics', name: 'CompanyAnalytics', component: () => import('@/views/admin-company/Analytics.vue') },
                { path: 'reports', name: 'CompanyReports', component: () => import('@/views/shared/Reports.vue') },
                { path: 'themes', name: 'CompanyThemes', component: () => import('@/views/admin-company/Themes.vue') },
                { path: 'tests', name: 'CompanyTests', component: () => import('@/views/admin-company/Tests.vue') },
                { path: 'questionnaires', name: 'CompanyQuestionnaires', component: () => import('@/views/shared/QuestionnaireLibrary.vue') },
                { path: 'tests/new', name: 'CompanyCampaignNew', component: () => import('@/views/shared/CampaignBuilder.vue') },
                { path: 'tests/edit/:id', name: 'CompanyCampaignEdit', component: () => import('@/views/shared/CampaignBuilder.vue'), props: { isEdit: true } },
                { path: 'questionnaire/:id', name: 'CompanyQuestionnaire', component: () => import('@/views/shared/QuestionnaireBuilder.vue') },
                { path: 'settings', name: 'CompanySettings', component: () => import('@/views/admin-company/Settings.vue') },
            ],
        },

        // ── Errors ─────────────────────────────────────────────────────────
        { path: '/403', name: 'Forbidden', component: () => import('@/views/error/Forbidden.vue') },
        { path: '/:pathMatch(.*)*', name: 'NotFound', component: () => import('@/views/error/NotFound.vue') },
    ],
});

router.beforeEach(authGuard);

export default router;
