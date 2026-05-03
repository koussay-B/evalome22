<script setup lang="ts">
import { ref, computed } from 'vue'
import { Building2, Loader2 } from 'lucide-vue-next'
import { useLocale } from '@/composables/useLocale'
import { UserRole, getRoleAvatarStyles, getRoleChipStyles } from '@/utils/roles'

export interface EmployeeCardData {
  id: number
  name: string
  email: string
  initials: string
  role: string | UserRole
  active: boolean
  company?: string
  imageUrl?: string | null
}

const props = defineProps<{
  employee: EmployeeCardData
  showCompany?: boolean
}>()

defineEmits<{ click: [] }>()

const { t } = useLocale()

// Image loading state
const imgLoading = ref(true)
const imgError   = ref(false)
const hasImage   = computed(() => !!props.employee.imageUrl && !imgError.value)
</script>

<template>
  <div
    @click="$emit('click')"
    class="p-4 rounded-xl border border-slate-100 bg-[#F5F4F2] hover:border-orange-300 hover:shadow-sm transition-all flex flex-col group cursor-pointer relative overflow-hidden w-full"
  >
    <div class="flex justify-between items-start mb-4 gap-3">
      <div class="flex items-center gap-3 min-w-0">

        <div
          class="w-18 h-18 rounded-full flex items-center justify-center shrink-0 overflow-hidden relative border border-slate-50 shadow-sm"
          :class="!hasImage ? getRoleAvatarStyles(employee.role) : 'bg-muted border border-border'"
        >
          <template v-if="hasImage">
            <span v-if="imgLoading" class="absolute inset-0 flex items-center justify-center">
              <Loader2 class="w-4 h-4 text-orange-400 animate-spin" />
            </span>
            <div class="w-full h-full rounded-full overflow-hidden border border-slate-300/40 shadow-sm">
              <img
                :src="employee.imageUrl!"
                :alt="employee.name"
                class="w-full h-full object-cover transition-all duration-200 group-hover:scale-105"
                :class="imgLoading ? 'opacity-0' : 'opacity-100'"
                @load="imgLoading = false"
                @error="imgError = true"
              />
            </div>
          </template>
          <span v-else class="text-xl text-slate-800">{{ employee.initials }}</span>
        </div>

       <div class="min-w-0">
        <h3 class="text-[15px]  text-slate-800 tracking-tight truncate max-w-[250px]">
          {{ employee.name }}
        </h3>
        
        <p class="text-[11px] text-slate-700 truncate mt-0.5 tracking-tight font-semibold">
          {{ employee.email }}
        </p>
      </div>
      </div>

      <slot name="actions" />
    </div>

    <div class="mt-auto pt-3 border-t border-slate-50">
      <div class="flex items-center justify-between gap-2 flex-wrap">

        <span :class="getRoleChipStyles(employee.role)" class="text-[11px] text-slate-800 uppercase tracking-widest px-2 py-0.5 rounded-md shrink-0">
          {{ t(`users.roles.${employee.role.charAt(0).toLowerCase() + employee.role.slice(1)}`) }}
        </span>

        <div class="flex items-center gap-1.5 shrink-0 ms-auto">
          <span
            class="w-1.5 h-1.5 rounded-full"
            :class="employee.active ? 'bg-orange-500 shadow-[0_0_8px_rgba(249,115,22,0.3)]' : 'bg-slate-300'"
          />
          <span
            class="text-[10px] font-bold uppercase tracking-tighter"
            :class="employee.active ? 'text-orange-600' : 'text-slate-400'"
          >
            {{ employee.active ? t('common.active') : t('common.inactive') }}
          </span>
        </div>
      </div>
    </div>
  </div>
</template>