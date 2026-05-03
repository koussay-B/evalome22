<template>
  <Card class="bg-card shadow-sm border-border overflow-hidden">
    <CardHeader class="flex flex-row items-center justify-between border-b bg-muted/20 pb-4">
      <div class="space-y-1">
        <CardTitle class="text-2xl font-black tracking-tight">Job Offers</CardTitle>
        <CardDescription>All job offers posted by companies on the platform.</CardDescription>
      </div>
      <Button size="lg" class="font-bold shadow-lg shadow-primary/20">
        <BriefcaseBusiness class="w-4 h-4 mr-2" />
        Add Offer
      </Button>
    </CardHeader>
    <CardContent class="p-0">
      <div class="overflow-x-auto">
        <table class="w-full text-left">
          <thead class="bg-muted/50 text-muted-foreground text-xs uppercase tracking-widest font-bold">
            <tr>
              <th class="px-8 py-5">Job Title</th>
              <th class="px-8 py-5">Company</th>
              <th class="px-8 py-5">Type</th>
              <th class="px-8 py-5">Status</th>
              <th class="px-8 py-5 text-right">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-border text-sm">
            <tr v-for="i in 5" :key="i" class="hover:bg-muted/30 transition-colors group">
              <td class="px-8 py-5 font-semibold text-foreground">
                {{ ['Frontend Developer', 'Backend Engineer', 'UX Designer', 'Data Analyst', 'DevOps Engineer'][i - 1] }}
              </td>
              <td class="px-8 py-5 text-muted-foreground">Company {{ i }}</td>
              <td class="px-8 py-5">
                <span class="bg-accent text-accent-foreground px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest border border-border">
                  {{ ['Full-time', 'Remote', 'Part-time', 'Contract', 'Full-time'][i - 1] }}
                </span>
              </td>
              <td class="px-8 py-5 font-bold">
                <div class="flex items-center gap-2">
                  <div :class="i === 3 ? 'bg-red-500' : 'bg-green-500'" class="w-2 h-2 rounded-full"></div>
                  <span :class="i === 3 ? 'text-red-600' : 'text-green-600'">
                    {{ i === 3 ? 'Closed' : 'Open' }}
                  </span>
                </div>
              </td>
              <td class="px-8 py-5 text-right">
                <Button variant="ghost" size="sm" class="font-bold hover:text-primary">
                  Manage
                </Button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </CardContent>
  </Card>
</template>

<script setup lang="ts">
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { BriefcaseBusiness } from 'lucide-vue-next';
</script>
