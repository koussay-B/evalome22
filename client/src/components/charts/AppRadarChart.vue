<template>
  <div class="rounded-2xl border border-border bg-card p-5">
    <div class="mb-4">
      <p v-if="title" class="text-[11px] font-bold uppercase tracking-widest text-muted-foreground">{{ title }}</p>
      <p v-if="subtitle" class="text-sm font-semibold text-slate-800 mt-0.5">{{ subtitle }}</p>
    </div>

    <div :style="{ height: height + 'px' }" class="flex items-center justify-center">
      <Radar v-if="data.length" :data="chartData" :options ="opts" />
      <p v-else class="text-xs text-muted-foreground italic">No data available</p>
    </div>

    <!-- Footer: avg + max -->
    <div v-if="data.length" class="flex items-center justify-between mt-3 pt-3 border-t border-border">
      <div class="flex items-center gap-4">
        <div class="text-center">
          <p class="text-xs text-muted-foreground font-medium">Avg</p>
          <p class="text-sm text-slate-800 text-slate-800">{{ avg }}{{ suffix }}</p>
        </div>
        <div class="text-center">
          <p class="text-xs text-muted-foreground font-medium">Max</p>
          <p class="text-sm text-slate-800 text-emerald-600">{{ max_ }}{{ suffix }}</p>
        </div>
        <div class="text-center">
          <p class="text-xs text-muted-foreground font-medium">Min</p>
          <p class="text-sm text-slate-800 text-amber-600">{{ min_ }}{{ suffix }}</p>
        </div>
      </div>
      <div class="flex items-center gap-1.5">
        <span class="w-2.5 h-2.5 rounded-sm" :style="{ backgroundColor: color }" />
        <span class="text-xs text-muted-foreground font-medium">{{ label }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Radar } from 'vue-chartjs'
import {
  Chart as ChartJS, RadialLinearScale, PointElement,
  LineElement, Filler, Tooltip,
} from 'chart.js'

ChartJS.register(RadialLinearScale, PointElement, LineElement, Filler, Tooltip)

const props = withDefaults(defineProps<{
  data:     { axis: string; value: number }[]
  title?:   string
  subtitle?: string
  label?:   string
  color?:   string
  height?:  number
  maxScale?: number
  suffix?:  string
}>(), {
  color:    '#6366f1',
  height:   260,
  maxScale: 100,
  suffix:   '%',
  label:    'Score',
})

const values = computed(() => props.data.map(d => d.value))
const avg    = computed(() => Math.round(values.value.reduce((s, v) => s + v, 0) / values.value.length))
const max_   = computed(() => Math.max(...values.value))
const min_   = computed(() => Math.min(...values.value))

const chartData = computed(() => ({
  labels: props.data.map(d => d.axis),
  datasets: [{
    label: props.label,
    data: values.value,
    fill: true,
    backgroundColor: props.color + '28',
    borderColor: props.color,
    pointBackgroundColor: props.color,
    pointBorderColor: '#fff',
    pointBorderWidth: 1.5,
    borderWidth: 2,
    pointRadius: 4,
    pointHoverRadius: 6,
  }],
}))

const opts = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
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
      callbacks: {
        label: (ctx: any) => ` ${ctx.raw}${props.suffix}`,
      },
    },
  },
  scales: {
    r: {
      beginAtZero: true,
      max: props.maxScale,
      grid: { color: 'rgba(128,128,128,0.15)' },
      angleLines: { color: 'rgba(128,128,128,0.15)' },
      ticks: {
        display: false,
        stepSize: props.maxScale / 5,
      },
      pointLabels: {
        color: '#a1a1aa',
        font: { size: 11, weight: '600' as const },
      },
    },
  },
}))
</script>
