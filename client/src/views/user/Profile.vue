<template>
  <div class="max-w-4xl mx-auto space-y-10">
    <Card class="bg-card shadow-xl border-border overflow-hidden rounded-[2rem]">
      <div class="h-48 bg-gradient-to-r from-primary/80 to-blue-600/80 backdrop-blur-sm relative">
        <div class="absolute inset-0 bg-grid-white/10 [mask-image:linear-gradient(to_bottom,white,transparent)]"></div>
      </div>
      <CardContent class="px-10 pb-10">
        <div class="relative flex justify-between items-end -translate-y-12">
          <div class="w-32 h-32 rounded-3xl bg-background p-2 shadow-2xl border border-border/50">
             <div class="w-full h-full rounded-2xl bg-secondary flex items-center justify-center text-4xl text-slate-800 text-primary border border-primary/10">
               {{ authStore.state.user?.userName.charAt(0).toUpperCase() }}
             </div>
          </div>
          <div class="flex gap-3">
            <Button variant="outline" class="font-bold rounded-xl px-6">Settings</Button>
            <Button class="font-bold rounded-xl px-8 shadow-lg shadow-primary/30">Edit Profile</Button>
          </div>
        </div>
        
        <div class="space-y-1 mt-2">
          <h2 class="text-4xl text-slate-800 text-slate-800tracking-tighter">{{ authStore.state.user?.userName }}</h2>
          <p class="text-muted-foreground font-medium">{{ authStore.state.user?.email }}</p>
        </div>

        <Separator class="my-10" />

        <div class="grid grid-cols-1 md:grid-cols-2 gap-12">
          <div class="space-y-6">
            <h4 class="text-xs text-slate-800 text-primary uppercase tracking-[0.3em]">Account Information</h4>
            <div class="space-y-3">
              <div class="flex justify-between items-center p-4 rounded-2xl bg-muted/40 border border-border/50">
                <span class="text-sm font-bold text-muted-foreground">Unique ID</span>
                <span class="font-mono text-xs font-bold bg-background px-3 py-1 rounded-full border border-border">#{{ authStore.state.user?.id }}</span>
              </div>
              <div class="flex justify-between items-center p-4 rounded-2xl bg-muted/40 border border-border/50">
                <span class="text-sm font-bold text-muted-foreground">Security Roles</span>
                <div class="flex gap-2">
                  <span v-if="authStore.state.user?.role" class="bg-primary/20 text-primary border border-primary/20 px-3 py-1 rounded-full text-[10px] text-slate-800 uppercase tracking-widest leading-none">
                    {{ authStore.state.user?.role }}
                  </span>
                </div>
              </div>
            </div>
          </div>
          
          <div class="space-y-6">
            <h4 class="text-xs text-slate-800 text-primary uppercase tracking-[0.3em]">System Status</h4>
             <div class="p-6 rounded-3xl bg-secondary/20 border border-dashed border-primary/30 text-center">
               <div class="text-2xl text-slate-800 text-primary italic mb-1 uppercase tracking-tighter">Verified Account</div>
               <p class="text-[10px] text-muted-foreground uppercase font-bold tracking-widest">Global Synchronization Active</p>
             </div>
          </div>
        </div>
      </CardContent>
    </Card>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/store/authStore';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';

const authStore = useAuthStore();
</script>

