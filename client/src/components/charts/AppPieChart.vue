<template>
  <div class="rounded-2xl border border-border bg-card p-5 flex flex-col">
    <div class="mb-4">
      <p v-if="title" class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ title }}</p>
      <p v-if="subtitle" class="text-sm font-semibold text-slate-800 mt-0.5">{{ subtitle }}</p>
    </div>

    <div class="flex-1 flex flex-col items-center gap-5">
      <!-- Chart -->
      <div :style="{ height: size + 'px', width: size + 'px' }" class="relative">
        <Pie v-if="hasData" :data="chartData" :options="opts" />
        <div v-else class="h-full w-full flex items-center justify-center">
          <div class="w-full h-full rounded-full bg-muted/40 flex items-center justify-center">
            <span class="text-xs text-muted-foreground italic">No data</span>
          </div>
        </div>

        <!-- Center label (total) -->
        <div v-if="hasData && showTotal" class="absolute inset-0 flex flex-col items-center justify-center pointer-events-none">
          <span class="text-2xl text-slate-800 text-slate-800">{{ totalValue }}</span>
          <span class="text-[10px] font-bold uppercase tracking-wide text-muted-foreground">Total</span>
        </div>
      </div>

      <!-- Legend -->
      <div class="w-full space-y-2">
        <div
          v-for="(item, i) in data"
          :key="item.label"
          class="flex items-center justify-between gap-3"
        >
          <div class="flex items-center gap-2 min-w-0">
            <span
              class="w-2.5 h-2.5 rounded-sm shrink-0"
              :style="{ backgroundColor: resolvedColors[i] }"
            />
            <span class="text-xs text-muted-foreground font-medium truncate">{{ item.label }}</span>
          </div>
          <div class="flex items-center gap-2 shrink-0">
            <span class="text-xs text-slate-800 text-slate-800">{{ item.value.toLocaleString() }}</span>
            <span class="text-[10px] text-muted-foreground tabular-nums w-8 text-right">
              {{ totalValue > 0 ? Math.round(item.value / totalValue * 100) : 0 }}%
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Pie } from 'vue-chartjs'
import { Chart as ChartJS, ArcElement, Tooltip } from 'chart.js'

ChartJS.register(ArcElement, Tooltip)

const PALETTE = [
  '#6366f1', '#10b981', '#f59e0b', '#3b82f6',
  '#f43f5e', '#8b5cf6', '#06b6d4', '#84cc16',
]

interface PieItem { label: string; value: number; color?: string }

const props = withDefaults(defineProps<{
  data:       PieItem[]
  title?:     string
  subtitle?:  string
  size?:      number
  showTotal?: boolean
  cutout?:    string
}>(), {
  size:      190,
  showTotal: false,
  cutout:    '0%',
})

const hasData = computed(() => props.data.some(d => d.value > 0))
const totalValue = computed(() => props.data.reduce((s, d) => s + d.value, 0))

const resolvedColors = computed(() =>
  props.data.map((d, i) => d.color ?? PALETTE[i % PALETTE.length])
)

const chartData = computed(() => ({
  labels: props.data.map(d => d.label),
  datasets: [{
    data: props.data.map(d => d.value),
    backgroundColor: resolvedColors.value,
    borderWidth: 0,
    hoverOffset: 6,
  }],
}))

const opts = computed(() => ({
  responsive: true,
  maintainAspectRatio: true,
  cutout: props.cutout,
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: '#1a1a1a',
      titleColor: '#ffffff',
      bodyColor: '#a1a1aa',
      borderColor: '#27272a',
      borderWidth: 1,
      padding: 10,
      displayColors: true,
      callbacks: {
        label: (ctx: any) => ` ${ctx.label}: ${ctx.formattedValue} (${Math.round(ctx.parsed / totalValue.value * 100)}%)`,
      },
    },
  },
}))
</script>
