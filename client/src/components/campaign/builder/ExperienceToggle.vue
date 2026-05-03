<template>
  <div class="flex items-center justify-between px-4 py-3.5 bg-muted/20 hover:bg-muted/40 transition-colors">
    <div class="flex items-start gap-3 min-w-0">
      <component
        :is="iconComponent"
        class="w-4 h-4 shrink-0 mt-0.5"
        :class="modelValue ? 'text-primary' : 'text-muted-foreground'"
      />
      <div class="min-w-0">
        <p class="text-sm font-semibold text-foreground">{{ label }}</p>
        <p class="text-xs text-muted-foreground mt-0.5">{{ desc }}</p>
      </div>
    </div>
    <ToggleSwitch :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)" class="ms-4 shrink-0" />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import ToggleSwitch from './ToggleSwitch.vue'
import {
  Clock, ArrowLeftRight, Shuffle, ListOrdered,
  CheckCircle, Star, CircleCheckBig,
} from 'lucide-vue-next'

const props = defineProps<{
  modelValue: boolean
  icon: string
  label: string
  desc: string
}>()
defineEmits<{ (e: 'update:modelValue', v: boolean): void }>()

const iconMap: Record<string, unknown> = {
  'clock':             Clock,
  'arrow-left-right':  ArrowLeftRight,
  'shuffle':           Shuffle,
  'list-ordered':      ListOrdered,
  'check-circle':      CheckCircle,
  'star':              Star,
  'circle-check-big':  CircleCheckBig,
}
const iconComponent = computed(() => iconMap[props.icon] ?? Clock)
</script>
