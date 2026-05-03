<template>
  <div class="rounded-2xl border border-border bg-card p-5">
    <div class="flex items-center justify-between mb-5">
      <div>
        <p v-if="title" class="text-[10px] font-bold uppercase tracking-[0.2em] text-primary/60 leading-none">{{ title }}</p>
        <p v-if="subtitle" class="text-sm font-semibold text-slate-800 mt-0.5">{{ subtitle }}</p>
      </div>
    </div>

    <div :style="{ height: height + 'px' }">
      <Bar v-if="hasData" :data="chartData" :options="opts" />
      <div v-else class="h-full flex items-center justify-center text-xs text-muted-foreground italic">
        No data available
      </div>
    </div>

    <div v-if="hasData" class="flex items-center justify-between mt-4 pt-3 border-t border-border">
      <div class="flex items-center gap-1.5 text-xs text-muted-foreground">
        <span class="w-2.5 h-2.5 rounded-sm shrink-0" :style="{ backgroundColor: color }" />
        <span>{{ subtitle ?? title ?? 'Values' }}</span>
      </div>
    
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Bar } from 'vue-chartjs'
import {
  Chart as ChartJS, CategoryScale, LinearScale,
  BarElement, BarController, Tooltip,
} from 'chart.js'

ChartJS.register(CategoryScale, LinearScale, BarElement, BarController, Tooltip)

interface BarItem { label: string; value: number; color?: string }

const props = withDefaults(defineProps<{
  data:         BarItem[]
  title?:       string
  subtitle?:    string
  color?:       string
  height?:      number
  horizontal?:  boolean
  max?:         number // <-- Prop jdida bch t-fixi el axe (ex: 100)
}>(), {
  color:      '#6366f1',
  height:     220,
  horizontal: true,
})

const hasData  = computed(() => props.data.some(d => d.value > 0))
const maxValue = computed(() => Math.max(...props.data.map(d => d.value), 1))

const resolvedColors = computed(() =>
  props.data.map(d => d.color ?? props.color)
)

const chartData = computed(() => ({
  labels: props.data.map(d => d.label),
  datasets: [{
    data:             props.data.map(d => d.value),
    backgroundColor: resolvedColors.value.map(c => c + 'cc'),
    hoverBackgroundColor: resolvedColors.value,
    borderRadius:     6,
    borderSkipped:    false,
    borderWidth:      0,
    barThickness:     props.horizontal ? 18 : undefined,
  }],
}))

const opts = computed(() => ({
  indexAxis:           (props.horizontal ? 'y' : 'x') as 'y' | 'x',
  responsive:          true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: '#1a1a1a',
      titleColor:      '#ffffff',
      bodyColor:       '#a1a1aa',
      borderColor:     '#27272a',
      borderWidth:     1,
      padding:         10,
      displayColors:   false,
      callbacks: {
        label: (ctx: any) => {
          // Itha fama props.max nesta3mlouh fil calcul mta' el pourcentage
          const reference = props.max ?? maxValue.value
          const pct = Math.round(ctx.raw / reference * 100)
          return ` ${ctx.raw.toLocaleString()} (${pct}%)`
        },
      },
    },
  },
  scales: {
    x: {
      grid:   { display: !props.horizontal, color: 'rgba(128,128,128,0.07)' },
      border: { display: false },
      ticks:  { color: '#71717a', font: { size: 11 } },
      max:    props.horizontal ? props.max : undefined, // Fixé kenou horizontal
    },
    y: {
      grid:         { display: props.horizontal, color: 'rgba(128,128,128,0.07)' },
      border:       { display: false },
      beginAtZero:  true,
      ticks:        { color: '#71717a', font: { size: 11 } },
      max:          !props.horizontal ? props.max : undefined, // Fixé kenou vertical
    },
  },
}))
</script>