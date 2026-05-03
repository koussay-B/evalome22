<template>

  <!-- ── Create / Edit Sheet ────────────────────────────────────── -->
  <Sheet v-model:open="isFormOpen">
    <SheetContent :side="sheetSide" class="sm:max-w-lg overflow-y-auto">
      <SheetHeader class="px-6 pt-6 pb-5 border-b bg-white border-border">
        <SheetTitle class="text-xl text-slate-800">
          {{ isEditing ? t('companies.editCompany') : t('companies.addCompany') }}
        </SheetTitle>
        <SheetDescription class="mt-1 ">
          {{ isEditing ? t('companies.editDesc') : t('companies.addDesc') }}
        </SheetDescription>
      </SheetHeader>

      <form @submit.prevent="handleSubmit" class="flex flex-col gap-5 p-6 bg-white border-b border-border">

        <!-- Logo upload / preview -->
        <div class="space-y-2">
          <label class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">
            {{ t('companies.logo') }}
            <span class="font-normal normal-case bg-white tracking-normal text-muted-foreground ms-1">({{ t('companies.optional') }})</span>
          </label>

          <!-- Show preview when a logo exists -->
          <div v-if="logoPreview || form.logo" class="flex items-center gap-4">
            <div class="w-16 h-16 rounded-xl border border-border bg-white flex items-center justify-center overflow-hidden shrink-0">
              <img :src="logoPreview || form.logo" class="w-full h-full object-cover" alt="logo preview" />
            </div>
            <div class="flex flex-col gap-1.5">
              <button type="button" @click="triggerFileInput"
                class="text-xs font-semibold text-foreground border border-border rounded-lg px-3 py-1.5 hover:bg-muted/50 transition-colors">
                {{ t('companies.clickToChange') }}
              </button>
              <button type="button" @click="clearLogo"
                class="text-xs font-semibold text-red-600 dark:text-red-400 hover:underline">
                {{ t('companies.removeLogo') }}
              </button>
            </div>
          </div>

          <!-- No logo: show dropzone -->
          <FileDropzone
            v-else
            v-model="logoFiles"
            accept="image/png,image/jpeg,image/svg+xml,image/webp"
            :max-size-mb="5"
            height="sm"
            :drop-label="t('companies.clickToUpload')"
          />

          <!-- Hidden input for "change logo" button -->
          <input ref="fileInputRef" type="file" accept="image/*" class="hidden" @change="onFileChange" />
        </div>

        <!-- Company Name -->
        <div class="space-y-2">
          <label class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">{{ t('companies.companyName') }} *</label>
          <div class="relative">
            <Building2 class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <Input v-model="form.name" :placeholder="t('companies.companyNamePlaceholder')" class="ps-9 h-10" required />
          </div>
        </div>

        <!-- Description -->
        <div class="space-y-2">
          <label class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">{{ t('companies.description') }}</label>
          <textarea
            v-model="form.description"
            :placeholder="t('companies.descriptionPlaceholder')"
            rows="2"
            class="w-full rounded-md border border-input bg-white px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 resize-none"
          />
        </div>

        <!-- Email + Phone row -->
        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-2">
            <label class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">{{ t('companies.adminEmail') }}</label>
            <div class="relative">
              <Mail class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <Input v-model="form.email" type="email" :placeholder="t('companies.adminEmailPlaceholder')" class="ps-9 h-10" />
            </div>
          </div>
          <div class="space-y-2">
            <label class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">{{ t('companies.phone') }}</label>
            <div class="relative">
              <Phone class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <Input v-model="form.phone" type="tel" :placeholder="t('companies.phonePlaceholder')" class="ps-9 h-10" />
            </div>
          </div>
        </div>

        <!-- Website -->
        <div class="space-y-2">
          <label class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">{{ t('companies.website') }}</label>
          <div class="relative">
            <Globe class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <Input v-model="form.website" :placeholder="t('companies.websitePlaceholder')" class="ps-9 h-10" />
          </div>
        </div>

        <!-- Address -->
        <div class="space-y-2">
          <label class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground">{{ t('companies.address') }}</label>
          <div class="relative">
            <MapPin class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <Input v-model="form.address" :placeholder="t('companies.addressPlaceholder')" class="ps-9 h-10" />
          </div>
        </div>

        <!-- Error -->
        <p v-if="formError" class="text-sm text-red-600 dark:text-red-400 -mt-1">{{ formError }}</p>

        <!-- Actions -->
        <div class="flex gap-3 pt-4 border-t border-border mt-auto">
          <button
            type="submit"
            :disabled="isSubmitting"
            class="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg bg-primary text-primary-foreground text-sm font-semibold hover:bg-primary/90 transition-colors disabled:opacity-60"
          >
            <Loader2 v-if="isSubmitting" class="w-4 h-4 animate-spin" />
            <Building2 v-else class="w-4 h-4" />
            {{ isSubmitting ? t('common.saving') : isEditing ? t('common.save') : t('companies.addCompany') }}
          </button>
          <button type="button" @click="isFormOpen = false"
            class="px-4 py-2.5 rounded-lg border border-border text-sm font-semibold text-foreground hover:bg-muted/50 transition-colors">
            {{ t('common.cancel') }}
          </button>
        </div>
      </form>
    </SheetContent>
  </Sheet>

  <!-- ── Affect Theme Dialog ───────────────────────────────────── -->
  <AlertDialog v-model:open="isAddThemeOpen">
    <AlertDialogContent class="sm:max-w-md">
      <AlertDialogHeader>
        <AlertDialogTitle>Affecter un Thème</AlertDialogTitle>
        <AlertDialogDescription>
          Saisissez le nom du thème à affecter à <strong>{{ selectedCompany?.name }}</strong>.
        </AlertDialogDescription>
      </AlertDialogHeader>
      <div class="py-4">
        <label class="text-xs text-slate-800 uppercase tracking-wider text-muted-foreground block mb-2">Nom du thème</label>
        <Input v-model="newThemeName" placeholder="ex: Développement Web, Soft Skills..." @keyup.enter="handleAffectTheme" />
      </div>
      <AlertDialogFooter>
        <AlertDialogCancel @click="isAddThemeOpen = false; newThemeName = ''">Annuler</AlertDialogCancel>
        <AlertDialogAction :disabled="isAffectingTheme || !newThemeName.trim()" @click.prevent="handleAffectTheme">
          <Loader2 v-if="isAffectingTheme" class="w-4 h-4 animate-spin me-1.5" />
          <Plus v-else class="w-4 h-4 me-1.5" />
          Affecter
        </AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>

  <!-- ── Delete Alert Dialog ───────────────────────────────────── -->
  <AlertDialog v-model:open="isDeleteOpen">
    <AlertDialogContent>
      <AlertDialogHeader>
        <AlertDialogTitle>{{ t('companies.deleteConfirmTitle') }}</AlertDialogTitle>
        <AlertDialogDescription>{{ t('companies.deleteConfirmDesc') }}</AlertDialogDescription>
      </AlertDialogHeader>
      <p v-if="deleteError" class="text-xs font-semibold text-red-600 dark:text-red-400 px-1">{{ deleteError }}</p>
      <AlertDialogFooter>
        <AlertDialogCancel :disabled="isDeleting" @click="deleteError = null">
          {{ t('common.cancel') }}
        </AlertDialogCancel>
        <AlertDialogAction
          :disabled="isDeleting"
          class="bg-red-600 hover:bg-red-700 focus:ring-red-600 text-white"
          @click.prevent="handleDelete"
        >
          <Loader2 v-if="isDeleting" class="w-4 h-4 animate-spin me-1.5" />
          <Trash2 v-else class="w-4 h-4 me-1.5" />
          {{ isDeleting ? t('common.saving') : t('common.delete') }}
        </AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>

  <!-- ── Company Detail Sheet ───────────────────────────────────── -->
  <Sheet v-model:open="isDetailOpen">
    <SheetContent :side="sheetSide" class="sm:max-w-sm flex flex-col p-0 bg-white">
      <template v-if="selectedCompany">

        <!-- Hero -->
        <SheetHeader class="px-5 pt-6 pb-5 border-b border-border shrink-0">
          <div class="flex items-center gap-4">
            <!-- Logo -->
            <div class="w-14 h-14 rounded-xl border-2 border-border bg-muted flex items-center justify-center overflow-hidden shadow-sm relative shrink-0">
              <template v-if="selectedCompany.logo && !detailImgError">
                <span v-if="detailImgLoading" class="absolute inset-0 flex items-center justify-center">
                  <Loader2 class="w-4 h-4 text-muted-foreground animate-spin" />
                </span>
                <img
                  :src="selectedCompany.logo"
                  :alt="selectedCompany.name"
                  class="w-full h-full object-cover transition-opacity duration-200"
                  :class="detailImgLoading ? 'opacity-0' : 'opacity-100'"
                  @load="detailImgLoading = false"
                  @error="detailImgError = true"
                />
              </template>
              <span v-else class="text-2xl text-slate-800 text-foreground select-none">
                {{ selectedCompany.name.charAt(0) }}
              </span>
            </div>
            <!-- Name + status -->
            <div class="flex-1 min-w-0">
              <SheetTitle class="text-lg text-slate-800 text-foreground leading-tight truncate">
                {{ selectedCompany.name }}
              </SheetTitle>
              <SheetDescription class="sr-only">{{ selectedCompany.name }}</SheetDescription>
              <div class="flex items-center gap-1.5 mt-1.5">
                <span class="w-1.5 h-1.5 rounded-full shrink-0" :class="selectedCompany.isActive ? 'bg-emerald-500' : 'bg-amber-400'" />
                <span class="text-xs font-semibold" :class="selectedCompany.isActive ? 'text-emerald-600 dark:text-emerald-400' : 'text-amber-600 dark:text-amber-400'">
                  {{ selectedCompany.isActive ? t('common.active') : t('common.inactive') }}
                </span>
              </div>
            </div>
          </div>
        </SheetHeader>

        <!-- Body -->
        <div class="flex-1 overflow-y-auto divide-y divide-border">

          <!-- Description -->
          <div v-if="selectedCompany.description" class="px-5 py-4">
            <p class="text-[10px] text-slate-800 uppercase tracking-widest text-muted-foreground mb-2">{{ t('companies.description') }}</p>
            <p class="text-sm text-foreground text-slate-800 leading-relaxed">{{ selectedCompany.description }}</p>
          </div>

          <!-- Contact -->
          <div class="px-5 py-4 space-y-1">
            <p class="text-[10px] text-slate-800 uppercase tracking-widest text-muted-foreground mb-3">{{ t('companies.information') }}</p>

            <!-- Email -->
            <div class="flex items-start gap-3 py-2 rounded-lg">
              <div class="w-7 h-7 rounded-md bg-muted flex items-center justify-center shrink-0 mt-0.5">
                <Mail class="w-3.5 h-3.5 text-muted-foreground" />
              </div>
              <div class="min-w-0 flex-1">
                <p class="text-[11px] text-muted-foreground font-medium mb-0.5">{{ t('companies.adminEmail') }}</p>
                <a v-if="selectedCompany.email" :href="`mailto:${selectedCompany.email}`"
                  class="text-sm font-semibold text-foreground hover:underline underline-offset-2 truncate block">
                  {{ selectedCompany.email }}
                </a>
                <span v-else class="text-sm text-muted-foreground">—</span>
              </div>
            </div>

            <!-- Phone -->
            <div class="flex items-start gap-3 py-2 rounded-lg">
              <div class="w-7 h-7 rounded-md bg-muted flex items-center justify-center shrink-0 mt-0.5">
                <Phone class="w-3.5 h-3.5 text-muted-foreground" />
              </div>
              <div class="min-w-0 flex-1">
                <p class="text-[11px] text-muted-foreground font-medium mb-0.5">{{ t('companies.phone') }}</p>
                <a v-if="selectedCompany.phone" :href="`tel:${selectedCompany.phone}`"
                  class="text-sm font-semibold text-foreground hover:underline underline-offset-2 block">
                  {{ selectedCompany.phone }}
                </a>
                <span v-else class="text-sm text-muted-foreground">—</span>
              </div>
            </div>

            <!-- Website -->
            <div class="flex items-start gap-3 py-2 rounded-lg">
              <div class="w-7 h-7 rounded-md bg-muted flex items-center justify-center shrink-0 mt-0.5">
                <Globe class="w-3.5 h-3.5 text-muted-foreground" />
              </div>
              <div class="min-w-0 flex-1">
                <p class="text-[11px] text-muted-foreground font-medium mb-0.5">{{ t('companies.website') }}</p>
                <a v-if="selectedCompany.website"
                  :href="selectedCompany.website.startsWith('http') ? selectedCompany.website : `https://${selectedCompany.website}`"
                  target="_blank" rel="noopener"
                  class="text-sm font-semibold text-foreground hover:underline underline-offset-2 truncate flex items-center gap-1">
                  {{ selectedCompany.website }}
                  <ExternalLink class="w-3 h-3 shrink-0 opacity-50" />
                </a>
                <span v-else class="text-sm text-muted-foreground">—</span>
              </div>
            </div>

            <!-- Address -->
            <div class="flex items-start gap-3 py-2 rounded-lg">
              <div class="w-7 h-7 rounded-md bg-muted flex items-center justify-center shrink-0 mt-0.5">
                <MapPin class="w-3.5 h-3.5 text-muted-foreground" />
              </div>
              <div class="min-w-0 flex-1">
                <p class="text-[11px] text-muted-foreground font-medium mb-0.5">{{ t('companies.address') }}</p>
                <span v-if="selectedCompany.address" class="text-sm font-semibold text-foreground">{{ selectedCompany.address }}</span>
                <span v-else class="text-sm text-muted-foreground">—</span>
              </div>
            </div>
          </div>

          <!-- Themes -->
          <div class="px-5 py-4 space-y-3">
  <div class="flex items-center justify-between">
    <p class="text-[10px] text-slate-800 uppercase tracking-[0.2em] font-bold">Thèmes Affectés</p>
    </div>

  <div v-if="isThemesLoading" class="flex justify-center py-2">
    <Loader2 class="w-4 h-4 animate-spin text-muted-foreground" />
  </div>

  <div v-else-if="companyThemes.length > 0" class="flex flex-wrap gap-1.5">
    <div v-for="theme in companyThemes" :key="theme.id" 
      class="flex items-center gap-2 px-2.5 py-1 rounded-lg bg-stone-50 border border-stone-200/60 text-[11px] font-semibold text-slate-600">
      <Layers class="w-3 h-3 opacity-60" />
      <span>{{ theme.name }}</span>
      </div>
  </div>

  <p v-else class="text-xs text-muted-foreground italic text-center py-2">
    Aucun thème affecté.
  </p>
</div>

          <!-- Activity -->
          <div class="px-5 py-4 space-y-1">
            <p class="text-[10px] text-slate-800 uppercase tracking-widest text-muted-foreground mb-3">{{ t('companies.activity') }}</p>

            <div class="flex items-start gap-3 py-2">
              <div class="w-7 h-7 rounded-md bg-muted flex items-center justify-center shrink-0 mt-0.5">
                <CalendarDays class="w-3.5 h-3.5 text-muted-foreground" />
              </div>
              <div>
                <p class="text-[11px] text-muted-foreground font-medium mb-0.5">{{ t('companies.createdOn') }}</p>
                <span class="text-sm font-semibold text-foreground">{{ formatDate(selectedCompany.createdAt) }}</span>
              </div>
            </div>

            <div v-if="selectedCompany.updatedAt" class="flex items-start gap-3 py-2">
              <div class="w-7 h-7 rounded-md bg-muted flex items-center justify-center shrink-0 mt-0.5">
                <RefreshCw class="w-3.5 h-3.5 text-muted-foreground" />
              </div>
              <div>
                <p class="text-[11px] text-muted-foreground font-medium mb-0.5">{{ t('companies.updatedOn') }}</p>
                <span class="text-sm font-semibold text-foreground">{{ formatDate(selectedCompany.updatedAt) }}</span>
              </div>
            </div>
          </div>

        </div>

        <!-- Footer Actions — icon-only with tooltips -->
        <div class="flex items-center justify-between px-6 py-4 border-t border-border shrink-0 gap-2">
          <TooltipProvider :delay-duration="200">

            <!-- Edit -->
            <Tooltip>
              <TooltipTrigger as-child>
                <button
                  @click="openEdit(selectedCompany)"
                  class="inline-flex items-center justify-center w-8 h-8 rounded-lg border border-[oklch(65%_0.18_51.057_/_0.2)] text-[oklch(65%_0.18_51.057)] bg-[oklch(97.5%_0.02_51.057_/_0.5)] hover:bg-[oklch(65%_0.18_51.057)] hover:text-white transition-all duration-200"
                >
                  <Pencil class="w-3.5 h-3.5" />
                </button>
              </TooltipTrigger>
              <TooltipContent side="top">{{ t('companies.editCompany') }}</TooltipContent>
            </Tooltip>
            <!-- add employee-->
            <Tooltip>
              <TooltipTrigger as-child>
                <button
                  @click="isAddEmployeeOpen = true"
                  class="inline-flex items-center justify-center w-8 h-8 rounded-lg border border-[oklch(65%_0.18_51.057_/_0.2)] text-[oklch(65%_0.18_51.057)] bg-[oklch(97.5%_0.02_51.057_/_0.5)] hover:bg-[oklch(65%_0.18_51.057)] hover:text-white transition-all duration-200"
                >
                  <UserPlus class="w-3.5 h-3.5" />
                </button>
              </TooltipTrigger>
              <TooltipContent side="top">{{ t('employees.addEmployee') }}</TooltipContent>
            </Tooltip>
            <!-- Spacer -->
            <div class="flex-1" />

            <!-- Delete -->
            <Tooltip>
              <TooltipTrigger as-child>
                <button
                  @click="isDeleteOpen = true"
                  class="inline-flex items-center justify-center w-8 h-8 rounded-lg border border-red-200 dark:border-red-900 text-red-500 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-950/30 transition-colors"
                >
                  <Trash2 class="w-4 h-4" />
                </button>
              </TooltipTrigger>
              <TooltipContent side="top" class="text-red-600">{{ t('companies.deleteCompany') }}</TooltipContent>
            </Tooltip>

          </TooltipProvider>
        </div>
      </template>
    </SheetContent>
  </Sheet>

  <!-- ── Add Employee Sheet ────────────────────────────────────── -->
  <AddEmployeeSheet
    v-model:open="isAddEmployeeOpen"
    :company-id="selectedCompany?.id"
    :company-name="selectedCompany?.name"
    @invited="fetchCurrentPage"
  />

  <!-- ── Page ───────────────────────────────────────────────────── -->
  <div class="space-y-6">

    <!-- Header -->
    <div class="flex items-end justify-between gap-4">
      <div>
        <p class="text-[11px] text-slate-800 uppercase tracking-widest text-muted-foreground mb-1">{{ t('companies.subtitle') }}</p>
        <h1 class="text-3xl text-slate-800 tracking-tight text-foreground">{{ t('companies.title') }}</h1>
      </div>
      <button
        @click="openCreate"
        class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-primary text-primary-foreground text-sm font-semibold hover:bg-primary/90 transition-colors shrink-0"
      >
        <Building2 class="w-4 h-4" /> {{ t('companies.addCompany') }}
      </button>
    </div>

    <!-- Search -->
    <div class="relative max-w-sm bg-slate-50">
      <Search class="absolute inset-s-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
      <Input v-model="searchQuery" :placeholder="t('companies.searchPlaceholder')" class="ps-9 h-9" />
    </div>

    <!-- ── Loading shimmer ── -->
    <div v-if="companyStore.list.isLoading" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <CompanyCardShimmer v-for="i in shimmerCount" :key="i" />
    </div>

    <!-- ── Error ── -->
    <div v-else-if="companyStore.list.isError" class="flex flex-col items-center gap-3 py-16 text-center">
      <div class="w-14 h-14 rounded-2xl bg-red-50 dark:bg-red-950/20 flex items-center justify-center">
        <AlertCircle class="w-7 h-7 text-red-500" />
      </div>
      <p class="text-sm font-semibold text-foreground">{{ t('common.errorOccurred') }}</p>
      <p class="text-xs text-muted-foreground">
        {{ companyStore.list.state.status === 'error' ? companyStore.list.state.message : '' }}
      </p>
      <button @click="fetchCurrentPage"
        class="flex items-center gap-2 px-4 py-2 rounded-lg border border-border text-sm font-semibold text-foreground hover:bg-muted/50 transition-colors mt-1">
        <RotateCcw class="w-3.5 h-3.5" /> {{ t('common.retry') }}
      </button>
    </div>

    <!-- ── Data ── -->
    <template v-else-if="companyStore.list.hasData">
      <div v-if="companyStore.list.currentItems.length > 0" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
        <CompanyCard
          v-for="company in companyStore.list.currentItems"
          :key="company.id"
          :company="company"
          @click="openDetail(company)"
        >
          <template #actions>
            <DropdownMenu>
              <DropdownMenuTrigger as-child>
                <button @click.stop
                  class="p-1.5 rounded-lg text-muted-foreground hover:bg-muted transition-colors opacity-0 group-hover:opacity-100 focus:opacity-100 shrink-0">
                  <MoreHorizontal class="w-4 h-4" />
                </button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" class="w-48">
                <DropdownMenuLabel class="text-xs text-muted-foreground font-semibold">{{ t('common.actions') }}</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuItem class="gap-2 cursor-pointer" @click.stop="openDetail(company)">
                  <Eye class="w-3.5 h-3.5" /> {{ t('companies.viewDetails') }}
                </DropdownMenuItem>
                <DropdownMenuItem class="gap-2 cursor-pointer" @click.stop="openEdit(company)">
                  <Pencil class="w-3.5 h-3.5" /> {{ t('common.edit') }}
                </DropdownMenuItem>
                <DropdownMenuItem class="gap-2 cursor-pointer" @click.stop="openAddEmployee(company)">
                  <UserPlus class="w-3.5 h-3.5" /> {{ t('employees.addEmployee') }}
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem
                  class="gap-2 cursor-pointer text-red-600 focus:text-red-600 focus:bg-red-50 dark:focus:bg-red-950/30"
                  @click.stop="openDeleteFromDropdown(company)"
                >
                  <Trash2 class="w-3.5 h-3.5" /> {{ t('common.delete') }}
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </template>
        </CompanyCard>
      </div>

      <EmptyData
        v-else
        :icon="Building2"
        :title="t('companies.emptyTitle')"
        :description="t('companies.emptyDesc')"
      >
        <button v-if="searchQuery" @click="clearSearch"
          class="flex items-center gap-2 px-4 py-2 rounded-lg border border-border text-sm font-semibold text-foreground hover:bg-muted/50 transition-colors mt-2">
          <X class="w-3.5 h-3.5" /> {{ t('common.clearFilters') }}
        </button>
      </EmptyData>
    </template>

    <!-- ── Footer: count | paginator | page size ── -->
    <div v-if="companyStore.list.hasData && companyStore.list.meta" class="grid grid-cols-3 items-center gap-2">
      <p class="text-xs text-muted-foreground">
        {{ t('common.showing') }} {{ companyStore.list.currentItems.length }}
        {{ t('common.of') }} {{ companyStore.list.meta.totalCount }}
        {{ t('companies.title').toLowerCase() }}
      </p>

      <div class="flex justify-center ">
        <AppPaginator
          :page="companyStore.list.meta.currentPage"
          :pages="companyStore.list.meta.totalPages"
          @page-change="onPageChange"
        />
      </div>

      <div class="flex justify-end">
        <Select
          :model-value="String(pageSize)"
          @update:model-value="(v) => { if (v) { pageSize = parseInt(v as string); resetPage() } }"
        >
          <SelectTrigger class="h-8 w-[110px] text-xs text-slate-800 bg-slate-50 border-border hover:border-primary/30 transition-colors">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem v-for="s in PAGE_SIZES" :key="s" :value="String(s)">
              {{ s }} {{ t('pagination.perPage') }}
            </SelectItem>
          </SelectContent>
        </Select>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from 'vue'
import { watchDebounced } from '@vueuse/core'
import { Input } from '@/components/ui/input'
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription } from '@/components/ui/sheet'
import { AlertDialog, AlertDialogContent, AlertDialogHeader, AlertDialogTitle, AlertDialogDescription, AlertDialogFooter, AlertDialogCancel, AlertDialogAction } from '@/components/ui/alert-dialog'
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip'
import { DropdownMenu, DropdownMenuTrigger, DropdownMenuContent, DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator } from '@/components/ui/dropdown-menu'
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from '@/components/ui/select'
import AppPaginator from '@/components/common/AppPaginator.vue'
import EmptyData from '@/components/common/EmptyData.vue'
import FileDropzone from '@/components/common/FileDropzone.vue'
import CompanyCard from '@/components/company/CompanyCard.vue'
import CompanyCardShimmer from '@/components/company/CompanyCardShimmer.vue'
import AddEmployeeSheet from '@/components/company/AddEmployeeSheet.vue'
import { useLocale } from '@/composables/useLocale'
import { useTableQuery, VALID_SIZES as PAGE_SIZES } from '@/composables/useTableQuery'
import { useCompanyStore } from '@/store/companyStore'
import { uploadImageApi } from '@utils/api'
import type { Company } from '@utils/models'
import {
  Building2, Mail, Phone, Globe, Pencil, UserPlus, Eye, Search,
  MoreHorizontal, X, MapPin, CalendarDays, AlertCircle, RotateCcw,
  Trash2, Loader2, RefreshCw, ExternalLink, Layers, Plus,
} from 'lucide-vue-next'
import { getAdminThemesApi, adminCreateThemeApi, adminDeleteThemeApi } from '@/utils/api/theme.api'

const { t, locale } = useLocale()
const { page, size: pageSize, search: searchQuery, resetPage } = useTableQuery()
const companyStore = useCompanyStore()

const isRtl      = computed(() => locale.value === 'ar')
const sheetSide  = computed<'right' | 'left'>(() => (isRtl.value ? 'left' : 'right'))
const shimmerCount = computed(() => Math.min(pageSize.value, 9))

// ── Form state (shared create / edit) ──────────────────────────────────────────

const isFormOpen   = ref(false)
const isEditing    = ref(false)
const editingId    = ref<number | null>(null)
const isSubmitting = ref(false)
const formError    = ref<string | null>(null)

// The actual File picked in the create/edit form (for Cloudinary upload)
const logoFile    = ref<File | null>(null)
const logoFiles   = ref<File[]>([])
const logoPreview = ref<string>('')    // data URL for preview only
const fileInputRef = ref<HTMLInputElement | null>(null)

const defaultForm = () => ({
  name: '', description: '', email: '', phone: '', website: '', address: '', logo: '',
})
const form = reactive(defaultForm())

// FileDropzone → track file + generate preview
watch(logoFiles, (files) => {
  const file = files[0] ?? null
  logoFile.value = file
  if (file) {
    const reader = new FileReader()
    reader.onload = () => { logoPreview.value = reader.result as string }
    reader.readAsDataURL(file)
  } else {
    logoPreview.value = ''
  }
})

function triggerFileInput() { fileInputRef.value?.click() }

function onFileChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0] ?? null
  logoFile.value = file
  if (!file) return
  const reader = new FileReader()
  reader.onload = () => { logoPreview.value = reader.result as string }
  reader.readAsDataURL(file)
}

function clearLogo() {
  logoFile.value    = null
  logoPreview.value = ''
  form.logo         = ''
  logoFiles.value   = []
  if (fileInputRef.value) fileInputRef.value.value = ''
}

function openCreate() {
  isEditing.value = false
  editingId.value = null
  Object.assign(form, defaultForm())
  clearLogo()
  formError.value = null
  isFormOpen.value = true
}

function openEdit(company: Company) {
  isEditing.value = true
  editingId.value = company.id
  Object.assign(form, {
    name:        company.name,
    description: company.description ?? '',
    email:       company.email       ?? '',
    phone:       company.phone       ?? '',
    website:     company.website     ?? '',
    address:     company.address     ?? '',
    logo:        company.logo        ?? '',
  })
  logoFile.value    = null
  logoPreview.value = ''
  logoFiles.value   = []
  formError.value   = null
  isDetailOpen.value = false   // close detail if open
  isFormOpen.value   = true
}

async function handleSubmit() {
  isSubmitting.value = true
  formError.value    = null

  // ── Logo: upload to Cloudinary if a new file was picked ──────────────
  let logoUrl = form.logo || undefined

  if (logoFile.value) {
    const { url, error } = await uploadImageApi(logoFile.value)
    if (error || !url) {
      formError.value    = error ?? t('common.unknownError')
      isSubmitting.value = false
      return
    }
    logoUrl = url
  }

  const payload = {
    name:        form.name,
    description: form.description || undefined,
    email:       form.email       || undefined,
    phone:       form.phone       || undefined,
    website:     form.website     || undefined,
    address:     form.address     || undefined,
    logo:        logoUrl,
  }

  const res = isEditing.value && editingId.value !== null
    ? await companyStore.updateCompany(editingId.value, payload)
    : await companyStore.createCompany(payload)

  isSubmitting.value = false

  if (res.error || !res.data) {
    formError.value = res.error ?? t('common.unknownError')
    return
  }

  isFormOpen.value = false
  fetchCurrentPage()
}

// ── Detail sheet ───────────────────────────────────────────────────────────────

const isDetailOpen     = ref(false)
const selectedCompany  = ref<Company | null>(null)
const detailImgLoading = ref(true)
const detailImgError   = ref(false)

const isAddEmployeeOpen = ref(false)

// ── Themes Affectation ────────────────────────────────────────────────────────
const companyThemes = ref<any[]>([])
const isThemesLoading = ref(false)
const isAffectingTheme = ref(false)
const newThemeName = ref('')
const isAddThemeOpen = ref(false)

async function fetchCompanyThemes(companyId: number) {
  isThemesLoading.value = true
  const { data } = await getAdminThemesApi()
  companyThemes.value = (data || []).filter((t: any) => t.companyId === companyId)
  isThemesLoading.value = false
}

async function handleAffectTheme() {
  if (!selectedCompany.value || !newThemeName.value.trim()) return
  isAffectingTheme.value = true
  const { error } = await adminCreateThemeApi({
    name: newThemeName.value,
    companyId: selectedCompany.value.id
  })
  isAffectingTheme.value = false
  if (!error) {
    newThemeName.value = ''
    isAddThemeOpen.value = false
    fetchCompanyThemes(selectedCompany.value.id)
  }
}

async function handleDeleteTheme(themeId: number) {
  if (!confirm('Supprimer ce thème ?')) return
  await adminDeleteThemeApi(themeId)
  if (selectedCompany.value) fetchCompanyThemes(selectedCompany.value.id)
}

const isDeleteOpen = ref(false)
const isDeleting   = ref(false)
const deleteError  = ref<string | null>(null)

function openDetail(company: Company) {
  selectedCompany.value  = company
  deleteError.value      = null
  detailImgLoading.value = true
  detailImgError.value   = false
  isDetailOpen.value     = true
  fetchCompanyThemes(company.id)
}

/** From dropdown → open detail sheet then immediately open the delete dialog */
function openDeleteFromDropdown(company: Company) {
  openDetail(company)
  isDeleteOpen.value = true
}

/** From dropdown → set context company + open add-employee sheet */
function openAddEmployee(company: Company) {
  selectedCompany.value  = company
  isAddEmployeeOpen.value = true
}

async function handleDelete() {
  if (!selectedCompany.value) return
  isDeleting.value  = true
  deleteError.value = null

  const { error } = await companyStore.deleteCompany(selectedCompany.value.id)
  isDeleting.value = false

  if (error) { deleteError.value = error; return }

  isDeleteOpen.value = false
  isDetailOpen.value = false
  fetchCurrentPage()
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
}

// ── Fetch logic ────────────────────────────────────────────────────────────────

function fetchCurrentPage() {
  companyStore.list.fetchPage({
    pageNumber: page.value,
    pageSize:   pageSize.value,
    search:     searchQuery.value || undefined,
    // no orderBy → backend uses COALESCE(UpdatedAt, CreatedAt) DESC
  })
}

onMounted(fetchCurrentPage)
watch([page, pageSize], fetchCurrentPage)

watchDebounced(
  searchQuery,
  () => { if (page.value !== 1) resetPage(); else fetchCurrentPage() },
  { debounce: 350 },
)

function onPageChange(newPage: number) { page.value = newPage }
function clearSearch() { searchQuery.value = ''; resetPage() }
</script>
