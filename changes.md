Backend

UsersController.cs (line 245): lines 245-364 add admin/company-admin/formateur user management endpoints for edit, reset password, and delete. Lines 412-579 expand /users/{id}/stats and add shared permission/helper methods.
AdminUpdateUserDto.cs (line 1): lines 1-20 add the DTO for managed-user updates.
ResetPasswordEmail.html (line 1): lines 1-184 add the HTML email template used when an admin resets a password.
IdentityServiceExtensions.cs (line 17): lines 17-32 add .AddDefaultTokenProviders() so password reset tokens work.
Frontend shared user-management module

UserDetailsSheet.vue (line 1): lines 1-309 create the reusable user details sheet with:
nested edit sheet
reset/delete confirmations
role badge + active state
user image support in the hero avatar at lines 61-71
admin activity hidden at lines 124-129 and 234-241
delete emit flow at lines 290-308
EditUserSheet.vue (line 1): lines 1-195 create the reusable edit sheet for username, email, role, and active status.
UserStatsGrid.vue (line 1): lines 1-89 create the reusable role-based stats renderer for Condidat, Formateur, CompanyAdmin, and Admin.
SheetContent.vue (line 15): lines 15-62 add overlayClass support so nested sheets can open above the details sheet.
AlertDialogContent.vue (line 17): lines 17-50 add overlayClass support so reset/delete confirmations can also open above the details sheet.
Client API, store, and models

user.api.ts (line 94): lines 94-128 add frontend API helpers for user stats, update, reset password, and delete.
user.model.ts (line 24): lines 24-46 add UpdateManagedUserPayload and ManagedUserStats.
userStore.ts (line 33): lines 33-56 add removeFromCurrentPage() for optimistic delete updates; lines 65-80 expose it under list.
Views updated to use the reusable sheet

Users.vue (line 9): lines 9-16 mount the reusable details sheet. Lines 72-145 wire card/context actions to view/edit/reset/delete. Lines 240-257 keep the original card mapping but attach the full source user. Lines 317-330 open the sheet with action state and refresh the list after update/delete/reset, including optimistic delete removal.
Employees.vue (line 10): lines 10-18 mount the same reusable details sheet for company-admin. Lines 73-145 wire employee actions. Lines 247-262 attach the full source user. Lines 311-324 refresh the list after actions and remove deleted users immediately.
Localization

en.ts (line 68): around lines 68-93 add user-management copy for edit description, reset/delete confirmations, success states, status help, and stats labels.
fr.ts (line 68): around lines 68-93 add the same strings in French.
ar.ts (line 68): around lines 68-93 add the same strings in Arabic.




        modified:   api/Controllers/UsersController.cs
        modified:   api/Extensions/IdentityServiceExtensions.cs
        modified:   client/src/components/auth/DevUserPanel.vue
        modified:   client/src/components/ui/alert-dialog/AlertDialogContent.vue
        modified:   client/src/components/ui/sheet/SheetContent.vue
        modified:   client/src/i18n/locales/ar.ts
        modified:   client/src/i18n/locales/en.ts
        modified:   client/src/i18n/locales/fr.ts
        modified:   client/src/store/userStore.ts
        modified:   client/src/utils/api/user.api.ts
        modified:   client/src/utils/models/user.model.ts
        modified:   client/src/views/admin-company/Employees.vue
        modified:   client/src/views/admin/users/Users.vue