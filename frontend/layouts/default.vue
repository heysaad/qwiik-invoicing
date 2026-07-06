<script setup lang="ts">
import { ChevronDown, FileText, LogOut, Menu, PlusCircle, Users, X } from '@lucide/vue';

const route = useRoute();
const { session, loadSession, clearSession } = useSession();
const sidebarOpen = ref(false);

const pageTitle = computed(() => {
  if (route.path.startsWith('/customers')) return 'Customers';
  if (route.path.includes('/new')) return 'Create invoice';
  if (route.path.includes('/edit')) return 'Edit invoice';
  if (route.path === '/auth') return 'Account access';
  return 'Invoices';
});

const pageDescription = computed(() => {
  if (route.path.startsWith('/customers')) return 'Manage billing profiles and contact details.';
  if (route.path.includes('/new')) return 'Prepare a new invoice for a customer.';
  if (route.path.includes('/edit')) return 'Update invoice details, items, and status.';
  return 'Track invoice totals, due dates, and payment status.';
});

const navItems = [
  { label: 'Invoices', to: '/invoices', icon: FileText, match: '/invoices' },
  { label: 'Customers', to: '/customers', icon: Users, match: '/customers' }
];

const isAuthPage = computed(() => route.path === '/auth');
const userLabel = computed(() => session.value.email || 'Account');
const userInitials = computed(() => {
  const email = session.value.email?.trim();
  if (!email) return 'QA';
  const name = email.split('@')[0] || email;
  const parts = name.split(/[._-]+/).filter(Boolean);
  if (parts.length > 1) return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
  return name.slice(0, 2).toUpperCase();
});

onMounted(loadSession);

watch(
  () => route.path,
  () => {
    sidebarOpen.value = false;
  }
);

async function logout() {
  clearSession();
  await navigateTo('/auth');
}
</script>

<template>
  <div class="min-h-screen bg-slate-100 text-slate-950">
    <main v-if="isAuthPage" class="min-h-screen">
      <slot />
    </main>

    <div v-else class="min-h-screen lg:grid lg:grid-cols-[17rem_1fr]">
      <aside class="fixed inset-y-0 left-0 z-40 hidden w-[17rem] border-r border-slate-200 bg-white lg:flex lg:flex-col">
        <div class="flex h-16 items-center gap-3 border-b border-slate-200 px-5">
          <div class="flex h-10 w-10 items-center justify-center rounded-lg bg-slate-950 text-white">
            <FileText :size="20" aria-hidden="true" />
          </div>
          <div class="min-w-0">
            <p class="text-sm font-semibold leading-5">Qwiik Invoicing</p>
            <p class="truncate text-xs text-slate-500">Tenant {{ shortId(session.tenantId) }}</p>
          </div>
        </div>

        <nav class="flex-1 space-y-1 px-3 py-4" aria-label="Main navigation">
          <NuxtLink
            v-for="item in navItems"
            :key="item.to"
            :to="item.to"
            class="flex min-h-11 items-center gap-3 rounded-md px-3 text-sm font-medium text-slate-600 transition-colors hover:bg-slate-100 hover:text-slate-950 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-slate-950"
            :class="{ 'bg-slate-950 text-white hover:bg-slate-950 hover:text-white': route.path.startsWith(item.match) }"
          >
            <component :is="item.icon" :size="18" aria-hidden="true" />
            {{ item.label }}
          </NuxtLink>
        </nav>

        <div class="border-t border-slate-200 p-3">
          <NuxtLink to="/invoices/new" class="btn min-h-11 w-full rounded-md border-0 bg-slate-950 text-white hover:bg-slate-800">
            <PlusCircle :size="17" aria-hidden="true" />
            New invoice
          </NuxtLink>
        </div>
      </aside>

      <div v-if="sidebarOpen" class="fixed inset-0 z-40 bg-slate-950/35 lg:hidden" @click="sidebarOpen = false"></div>
      <aside
        class="fixed inset-y-0 left-0 z-50 flex w-72 -translate-x-full flex-col border-r border-slate-200 bg-white transition-transform duration-200 lg:hidden"
        :class="{ 'translate-x-0': sidebarOpen }"
      >
        <div class="flex h-16 items-center justify-between border-b border-slate-200 px-4">
          <div class="flex items-center gap-3">
            <div class="flex h-10 w-10 items-center justify-center rounded-lg bg-slate-950 text-white">
              <FileText :size="20" aria-hidden="true" />
            </div>
            <div>
              <p class="text-sm font-semibold">Qwiik Invoicing</p>
              <p class="text-xs text-slate-500">Tenant {{ shortId(session.tenantId) }}</p>
            </div>
          </div>
          <button class="btn btn-ghost btn-square min-h-11 cursor-pointer rounded-md" aria-label="Close navigation" @click="sidebarOpen = false">
            <X :size="20" aria-hidden="true" />
          </button>
        </div>
        <nav class="flex-1 space-y-1 px-3 py-4" aria-label="Mobile navigation">
          <NuxtLink
            v-for="item in navItems"
            :key="item.to"
            :to="item.to"
            class="flex min-h-11 items-center gap-3 rounded-md px-3 text-sm font-medium text-slate-600 transition-colors hover:bg-slate-100 hover:text-slate-950"
            :class="{ 'bg-slate-950 text-white hover:bg-slate-950 hover:text-white': route.path.startsWith(item.match) }"
          >
            <component :is="item.icon" :size="18" aria-hidden="true" />
            {{ item.label }}
          </NuxtLink>
        </nav>
      </aside>

      <div class="min-w-0 lg:col-start-2">
        <header class="sticky top-0 z-30 border-b border-slate-200 bg-white/95 backdrop-blur">
          <div class="flex h-16 items-center justify-between gap-3 px-4 sm:px-6 lg:px-8">
            <div class="flex min-w-0 items-center gap-3">
              <button class="btn btn-ghost btn-square min-h-11 cursor-pointer rounded-md lg:hidden" aria-label="Open navigation" @click="sidebarOpen = true">
                <Menu :size="20" aria-hidden="true" />
              </button>
              <div class="min-w-0">
                <h1 class="truncate text-lg font-semibold text-slate-950">{{ pageTitle }}</h1>
                <p class="hidden truncate text-sm text-slate-500 sm:block">{{ pageDescription }}</p>
              </div>
            </div>

            <div class="dropdown dropdown-end">
              <button tabindex="0" class="flex min-h-11 cursor-pointer items-center gap-2 rounded-full border border-slate-200 bg-white py-1.5 pl-1.5 pr-2.5 text-slate-700 transition-colors hover:border-slate-300 hover:bg-slate-50 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-slate-950">
                <span class="flex h-8 w-8 items-center justify-center rounded-full bg-slate-950 text-xs font-semibold text-white">{{ userInitials }}</span>
                <span class="hidden max-w-36 truncate text-sm font-medium sm:inline">{{ userLabel }}</span>
                <ChevronDown :size="16" aria-hidden="true" />
              </button>
              <ul tabindex="0" class="menu dropdown-content z-50 mt-2 w-64 rounded-lg border border-slate-200 bg-white p-2 shadow-lg">
                <li class="mb-1 flex-row items-center gap-3 rounded-md px-3 py-2">
                  <span class="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-slate-950 text-xs font-semibold text-white">{{ userInitials }}</span>
                  <span class="min-w-0">
                    <span class="block truncate text-sm font-medium text-slate-950">Account</span>
                    <span class="block truncate text-xs text-slate-500">{{ session.email || 'Signed in' }}</span>
                  </span>
                </li>
                <li>
                  <button class="min-h-11 cursor-pointer rounded-md text-error" @click="logout">
                    <LogOut :size="17" aria-hidden="true" />
                    Sign out
                  </button>
                </li>
              </ul>
            </div>
          </div>
        </header>

        <main class="mx-auto w-full max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
          <slot />
        </main>
      </div>
    </div>
  </div>
</template>
