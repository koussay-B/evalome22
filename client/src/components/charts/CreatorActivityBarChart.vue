<template>
  <div class="rounded-3xl border border-border bg-card p-6 shadow-sm">
    <div class="mb-5 flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
      <div>
        <p class="text-[10px] font-bold uppercase tracking-[0.2em] text-primary/60">
          {{ title }}
        </p>
        <p class="mt-1 text-sm font-bold text-slate-800">
          {{ subtitle }}
        </p>
      </div>
      <div v-if="$slots.actions" class="flex flex-wrap items-center gap-3">
        <slot name="actions" />
      </div>
    </div>

    <div :style="{ height: height + 'px' }">
      <Bar v-if="hasData" :data="chartData" :options="opts" />
      <div
        v-else
        class="flex h-full items-center justify-center rounded-2xl border border-dashed border-border bg-muted/10 text-xs font-medium text-muted-foreground"
      >
        Aucune donnee disponible
      </div>
    </div>

    <div v-if="hasData" class="mt-4 flex flex-wrap items-center justify-between gap-3 border-t border-border pt-3">
      <div class="flex items-center gap-4 text-xs text-muted-foreground">
        <span class="flex items-center gap-1.5">
          <span class="h-2.5 w-2.5 rounded-sm bg-sky-500" />
          Questions
        </span>
        <span class="flex items-center gap-1.5">
          <span class="h-2.5 w-2.5 rounded-sm bg-orange-500" />
          Campagnes
        </span>
      </div>
      <span class="text-xs text-muted-foreground tabular-nums">
        Total:
        <strong class="text-slate-800">{{ totalCreations.toLocaleString() }}</strong>
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Bar } from 'vue-chartjs'
import {
  Chart as ChartJS,
  BarController,
  BarElement,
  CategoryScale,
  Legend,
  LinearScale,
  Tooltip,
} from 'chart.js'

ChartJS.register(BarController, BarElement, CategoryScale, LinearScale, Legend, Tooltip)

interface CreatorActivityBarItem {
  label: string
  role: string
  questionsCount: number
  campaignsCount: number
}

const props = withDefaults(defineProps<{
  data: CreatorActivityBarItem[]
  title?: string
  subtitle?: string
  height?: number
}>(), {
  title: 'CREATIONS PAR UTILISATEUR',
  subtitle: 'Questions et campagnes creees par role',
  height: 320,
})

const hasData = computed(() =>
  props.data.some(item => item.questionsCount > 0 || item.campaignsCount > 0)
)

const totalCreations = computed(() =>
  props.data.reduce((sum, item) => sum + item.questionsCount + item.campaignsCount, 0)
)

const chartData = computed(() => ({
  labels: props.data.map(item => item.label),
  datasets: [
    {
      label: 'Questions',
      data: props.data.map(item => item.questionsCount),
      backgroundColor: '#0ea5e9cc',
      hoverBackgroundColor: '#0ea5e9',
      borderRadius: 6,
      borderSkipped: false,
      categoryPercentage: 0.72,
      barPercentage: 0.86,
    },
    {
      label: 'Campagnes',
      data: props.data.map(item => item.campaignsCount),
      backgroundColor: '#f97316cc',
      hoverBackgroundColor: '#f97316',
      borderRadius: 6,
      borderSkipped: false,
      categoryPercentage: 0.72,
      barPercentage: 0.86,
    },
  ],
}))

const opts = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  interaction: {
    mode: 'index' as const,
    intersect: false,
  },
  plugins: {
    legend: {
      display: false,
    },
    tooltip: {
      backgroundColor: '#1a1a1a',
      titleColor: '#ffffff',
      bodyColor: '#e5e7eb',
      borderColor: '#27272a',
      borderWidth: 1,
      padding: 10,
      displayColors: true,
      callbacks: {
        title: (items: any[]) => {
          const first = items[0]
          const item = props.data[first?.dataIndex ?? 0]
          if (!item) return ''
          return item.label === item.role ? item.label : `${item.label} - ${item.role}`
        },
        label: (ctx: any) => ` ${ctx.dataset.label}: ${ctx.raw.toLocaleString()}`,
      },
    },
  },
  scales: {
    x: {
      grid: { display: false },
      border: { display: false },
      ticks: {
        color: '#71717a',
        font: { size: 11 },
        maxRotation: 0,
        autoSkip: true,
        maxTicksLimit: 8,
      },
    },
    y: {
      beginAtZero: true,
      grid: { color: 'rgba(128,128,128,0.08)' },
      border: { display: false },
      ticks: {
        precision: 0,
        color: '#71717a',
        font: { size: 11 },
      },
    },
  },
}))
</script>
