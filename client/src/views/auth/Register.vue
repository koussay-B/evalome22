<template>
  <Card class="w-full max-w-md border-none shadow-none bg-transparent">
    <CardHeader class="space-y-1 text-center pb-8">
      <CardTitle class="text-3xl font-bold tracking-tight">Create Account</CardTitle>
      <CardDescription class="text-muted-foreground">
        Join our platform today
      </CardDescription>
    </CardHeader>
    <CardContent class="grid gap-4">
      <form @submit.prevent="handleRegister" class="grid gap-4">
        <div class="grid gap-2">
          <label for="username" class="text-sm font-medium leading-none">Username</label>
          <Input id="username" v-model="form.username" type="text" placeholder="johndoe" required />
        </div>
        <div class="grid gap-2">
          <label for="email" class="text-sm font-medium leading-none">Email</label>
          <Input id="email" v-model="form.email" type="email" placeholder="name@example.com" required />
        </div>
        <div class="grid gap-2">
          <label for="password" class="text-sm font-medium leading-none">Password</label>
          <Input id="password" v-model="form.password" type="password" required />
        </div>
        <Button type="submit" class="w-full py-6 mt-4 font-bold text-lg">
          Create Account
        </Button>
      </form>
    </CardContent>
    <CardFooter class="flex flex-col gap-4 text-center mt-2">
      <div class="text-sm text-muted-foreground">
        Already have an account? 
        <router-link to="/auth/login" class="text-primary font-bold hover:underline underline-offset-4">
          Sign In
        </router-link>
      </div>
    </CardFooter>
  </Card>
</template>

<script setup lang="ts">
import { reactive } from 'vue';
import { useRouter } from 'vue-router';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter } from '@/components/ui/card';

const router = useRouter();
const form = reactive({
  username: '',
  email: '',
  password: '',
});

const handleRegister = () => {
  console.log('Registering...', form);
  router.push({ name: 'Login' });
};
</script>
