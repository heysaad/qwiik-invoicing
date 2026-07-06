<script setup lang="ts">
import { FileText, KeyRound, LogIn, Mail, UserPlus } from '@lucide/vue';

definePageMeta({ middleware: 'auth' });

const api = useApi();
const { saveSession } = useSession();

const mode = ref<'login' | 'register'>('login');
const email = ref('');
const password = ref('');
const busy = ref(false);
const error = ref('');
const notice = ref('');

async function submit() {
  error.value = '';
  notice.value = '';
  busy.value = true;
  saveSession({ tenantId: '', email: email.value.trim(), accessToken: '' });

  try {
    if (mode.value === 'register') {
      const result = await api.register({ email: email.value.trim(), password: password.value });
      saveSession({ tenantId: result.tenantId, email: email.value.trim() });
      notice.value = 'Account registered. Your tenant is ready.';
      await navigateTo('/customers');
      return;
    }

    const result = await api.login({ email: email.value.trim(), password: password.value });
    saveSession({
      email: email.value.trim(),
      accessToken: result.accessToken
    });

    const tenant = await api.myTenant();
    saveSession({ tenantId: tenant.id });

    await navigateTo('/invoices');
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Request failed.';
  } finally {
    busy.value = false;
  }
}
</script>

<template>
  <div class="grid min-h-screen bg-slate-100 px-4 py-6 text-slate-950 sm:px-6 lg:grid-cols-[1fr_30rem] lg:p-6">
    <section class="relative hidden overflow-hidden rounded-lg bg-slate-950 text-white lg:flex lg:flex-col lg:justify-between">
      <div class="absolute inset-0 opacity-25">
        <div class="h-full w-full bg-[linear-gradient(135deg,#0f172a_0%,#164e63_52%,#f59e0b_100%)]"></div>
      </div>
      <div class="relative z-10 flex items-center gap-3 p-8">
        <div class="flex h-11 w-11 items-center justify-center rounded-lg bg-white text-slate-950">
          <FileText :size="22" aria-hidden="true" />
        </div>
        <div>
          <p class="text-sm font-semibold">Qwiik Invoicing</p>
          <p class="text-xs text-white/60">Admin workspace</p>
        </div>
      </div>

      <div class="relative z-10 max-w-xl p-8">
        <p class="text-4xl font-semibold leading-tight">Clean invoicing, without the back-office noise.</p>
        <p class="mt-4 max-w-md text-sm leading-6 text-white/70">Customers, invoices, and payment status stay close at hand in one focused admin panel.</p>
      </div>
    </section>

    <main class="grid min-h-[calc(100vh-3rem)] place-items-center lg:min-h-0 lg:bg-white">
      <section class="w-full max-w-md">
        <div class="mb-8 flex items-center gap-3 lg:hidden">
          <div class="flex h-11 w-11 items-center justify-center rounded-lg bg-slate-950 text-white">
            <FileText :size="22" aria-hidden="true" />
          </div>
          <div>
            <p class="text-sm font-semibold">Qwiik Invoicing</p>
            <p class="text-xs text-slate-500">Admin workspace</p>
          </div>
        </div>

        <div class="rounded-lg border border-slate-200 bg-white p-5 shadow-sm sm:p-6 lg:border-0 lg:shadow-none">
          <div class="mb-6">
            <div class="mb-4 flex h-12 w-12 items-center justify-center rounded-lg bg-slate-950 text-white">
              <KeyRound :size="21" aria-hidden="true" />
            </div>
            <h1 class="text-2xl font-semibold tracking-tight">{{ mode === 'register' ? 'Create your account' : 'Welcome back' }}</h1>
            <p class="mt-2 text-sm leading-6 text-slate-500">
              {{ mode === 'register' ? 'Start a new workspace for your invoices.' : 'Sign in to continue to your workspace.' }}
            </p>
          </div>

          <div class="mb-5 grid grid-cols-2 rounded-lg bg-slate-100 p-1">
            <button
              type="button"
              class="flex min-h-10 cursor-pointer items-center justify-center gap-2 rounded-md text-sm font-medium text-slate-600 transition-colors"
              :class="mode === 'login' ? 'bg-white text-slate-950 shadow-sm' : 'hover:text-slate-950'"
              @click="mode = 'login'"
            >
              <LogIn :size="15" aria-hidden="true" />
              Login
            </button>
            <button
              type="button"
              class="flex min-h-10 cursor-pointer items-center justify-center gap-2 rounded-md text-sm font-medium text-slate-600 transition-colors"
              :class="mode === 'register' ? 'bg-white text-slate-950 shadow-sm' : 'hover:text-slate-950'"
              @click="mode = 'register'"
            >
              <UserPlus :size="15" aria-hidden="true" />
              Register
            </button>
          </div>

          <form class="grid gap-4" @submit.prevent="submit">
            <label class="grid gap-1.5">
              <span class="text-sm font-medium text-slate-700">Email</span>
              <span class="relative">
                <Mail class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" :size="18" aria-hidden="true" />
                <input
                  v-model="email"
                  class="input h-11 w-full rounded-md border-slate-300 bg-white pl-10 text-slate-950 placeholder:text-slate-400 focus:border-slate-950 focus:outline-slate-950"
                  type="email"
                  required
                  autocomplete="email"
                  placeholder="you@example.com"
                />
              </span>
            </label>
            <label class="grid gap-1.5">
              <span class="text-sm font-medium text-slate-700">Password</span>
              <span class="relative">
                <KeyRound class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" :size="18" aria-hidden="true" />
                <input
                  v-model="password"
                  class="input h-11 w-full rounded-md border-slate-300 bg-white pl-10 text-slate-950 placeholder:text-slate-400 focus:border-slate-950 focus:outline-slate-950"
                  type="password"
                  required
                  :autocomplete="mode === 'register' ? 'new-password' : 'current-password'"
                  placeholder="Enter password"
                />
              </span>
            </label>

            <div v-if="error" class="alert alert-error rounded-md py-3 text-sm whitespace-pre-line">{{ error }}</div>
            <div v-if="notice" class="alert alert-success rounded-md py-3 text-sm">{{ notice }}</div>

            <button class="btn min-h-11 w-full cursor-pointer rounded-md border-0 bg-slate-950 text-white hover:bg-slate-800" :disabled="busy || !email || !password">
              <span v-if="busy" class="loading loading-spinner loading-sm"></span>
              {{ mode === 'register' ? 'Create account' : 'Sign in' }}
            </button>
          </form>
        </div>
      </section>
    </main>
  </div>
</template>
