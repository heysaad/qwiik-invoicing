<script setup lang="ts">
import type { Customer, InvoicePayload } from '~/types/api';

definePageMeta({ middleware: 'auth' });

const api = useApi();
const customers = ref<Customer[]>([]);
const loading = ref(true);
const saving = ref(false);
const error = ref('');

async function loadCustomers() {
  loading.value = true;
  error.value = '';
  try {
    const result = await api.customers();
    customers.value = result.items;
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Could not load customers.';
  } finally {
    loading.value = false;
  }
}

async function saveInvoice(payload: InvoicePayload) {
  saving.value = true;
  error.value = '';
  try {
    await api.createInvoice(payload);
    await navigateTo('/invoices');
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Could not save invoice.';
  } finally {
    saving.value = false;
  }
}

onMounted(loadCustomers);
</script>

<template>
  <div v-if="error" class="alert alert-error mb-4 rounded-lg text-sm whitespace-pre-line">{{ error }}</div>
  <div v-if="loading" class="rounded-lg border border-base-300 bg-base-100 p-4">
    <span class="loading loading-spinner loading-sm"></span> Loading customer options
  </div>
  <div v-else-if="customers.length === 0" class="rounded-lg border border-base-300 bg-base-100 p-6 text-center">
    <p class="font-medium">Create a customer before creating invoices.</p>
    <NuxtLink to="/customers" class="btn btn-primary btn-sm mt-4 min-h-10 cursor-pointer">Go to customers</NuxtLink>
  </div>
  <InvoiceEditor v-else :customers="customers" :busy="saving" @save="saveInvoice" />
</template>
