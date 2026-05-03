<template>
  <div class="rounded-2xl border border-border bg-card p-5">
    <div class="flex items-center justify-between mb-5">
      <div>
        <p v-if="title" class="text-[10px] font-bold uppercase tracking-[0.2em] text-primary/60 leading-none">{{ title }}</p>
        <p v-if="subtitle" class="text-sm font-semibold text-slate-800 mt-0.5">{{ subtitle }}</p>
      </div>
      <div
        v-if="showRangeSelector"
        class="flex items-center gap-0.5 rounded-lg border border-border p-0.5 bg-background"
      >
        <button
          v-for="r in RANGES"
          :key="r.days"
          @click="activeRange = r.days"
          class="px-2.5 py-1 rounded-md text-xs font-bold transition-all"
          :class="activeRange === r.days
            ? 'bg-foreground text-background shadow-sm'
            : 'text-muted-foreground hover:text-slate-800'"
        >
          {{ r.label }}
        </button>
      </div>
    </div>

    <div :style="{ height: height + 'px' }">
      <Line v-if="filtered.length" :data="chartData" :options="opts" />
      <div v-else class="h-full flex items-center justify-center text-xs text-muted-foreground italic">
        No data available
      </div>
    </div>

    <!-- Footer summary -->
    <div v-if="filtered.length" class="flex items-center justify-between mt-4 pt-3 border-t border-border">
      <div class="flex items-center gap-1.5 text-xs text-muted-foreground">
        <TrendingUp class="w-3.5 h-3.5 text-emerald-500" />
        <span>Total: <strong class="text-slate-800">{{ total.toLocaleString() }}</strong></span>
      </div>
      <span class="text-xs text-muted-foreground">
        {{ rangeLabel }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { Line } from 'vue-chartjs'
import {
  Chart as ChartJS, CategoryScale, LinearScale,
  PointElement, LineElement, Filler, Tooltip,
} from 'chart.js'
import { TrendingUp } from 'lucide-vue-next'

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Filler, Tooltip)

interface DataPoint { date: string; count: number }

const props = withDefaults(defineProps<{
  data:               DataPoint[]
  title?:             string
  subtitle?:          string
  color?:             string
  height?:            number
  showRangeSelector?: boolean
}>(), {
  color:              '#6366f1',
  height:             220,
  showRangeSelector:  false,
})

const RANGES = [
  { label: '7D',  days: 7  },
  { label: '30D', days: 30 },
  { label: '90D', days: 90 },
]
const activeRange = ref(30)

const filtered = computed(() => {
  if (!props.showRangeSelector || !props.data.length) return props.data
  const cutoff = new Date()
  cutoff.setDate(cutoff.getDate() - activeRange.value)
  return props.data.filter(d => new Date(d.date) >= cutoff)
})

const total = computed(() => filtered.value.reduce((s, d) => s + d.count, 0))

const rangeLabel = computed(() => {
  if (!props.showRangeSelector) return ''
  const r = RANGES.find(r => r.days === activeRange.value)
  return `Last ${r?.label ?? ''}`
})

const chartData = computed(() => ({
  labels: filtered.value.map(d =>
    new Date(d.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
  ),
  datasets: [{
    label: props.title ?? 'Count',
    fill: true,
    tension: 0.4,
    data: filtered.value.map(d => d.count),
    borderColor: props.color,
    backgroundColor: (ctx: any) => {
      const chart = ctx.chart
      const { ctx: c, chartArea } = chart
      if (!chartArea) return 'transparent'
      const g = c.createLinearGradient(0, chartArea.top, 0, chartArea.bottom)
      const hex = props.color.replace('#', '')
      const r = parseInt(hex.slice(0, 2), 16)
      const gr = parseInt(hex.slice(2, 4), 16)
      const b = parseInt(hex.slice(4, 6), 16)
      g.addColorStop(0, `rgba(${r},${gr},${b},0.32)`)
      g.addColorStop(1, `rgba(${r},${gr},${b},0.00)`)
      return g
    },
    borderWidth: 2,
    pointRadius: filtered.value.length < 15 ? 3 : 1,
    pointHoverRadius: 5,
  }],
}))

const opts = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  interaction: { mode: 'index' as const, intersect: false },
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: '#1a1a1a',
      titleColor: '#ffffff',
      bodyColor: '#a1a1aa',
      borderColor: '#27272a',
      borderWidth: 1,
      padding: 10,
      displayColors: false,
    },
  },
  scales: {
    x: {
      grid: { display: false },
      border: { display: false },
      ticks: {
        maxTicksLimit: activeRange.value <= 7 ? 7 : 6,
        color: '#71717a',
        font: { size: 11 },
      },
    },
    y: {
      beginAtZero: true,
      grid: { color: 'rgba(128,128,128,0.07)' },
      border: { display: false },
      ticks: { color: '#71717a', font: { size: 11 } },
    },
  },
}))
</script>
