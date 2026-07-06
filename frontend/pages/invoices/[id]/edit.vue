<script setup lang="ts">
import type { Customer, Invoice, InvoicePayload } from '~/types/api';

definePageMeta({ middleware: 'auth' });

const route = useRoute();
const api = useApi();
const customers = ref<Customer[]>([]);
const invoice = ref<Invoice | null>(null);
const loading = ref(true);
const saving = ref(false);
const error = ref('');

async function loadData() {
  loading.value = true;
  error.value = '';
  try {
    const [customerResult, invoiceResult] = await Promise.all([
      api.customers(),
      api.invoice(String(route.params.id))
    ]);
    customers.value = customerResult.items;
    invoice.value = invoiceResult;
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Could not load invoice.';
  } finally {
    loading.value = false;
  }
}

async function saveInvoice(payload: InvoicePayload) {
  saving.value = true;
  error.value = '';
  try {
    await api.updateInvoice(String(route.params.id), payload);
    await navigateTo('/invoices');
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Could not save invoice.';
  } finally {
    saving.value = false;
  }
}

onMounted(loadData);
</script>

<template>
  <div v-if="error" class="alert alert-error mb-4 rounded-lg text-sm whitespace-pre-line">{{ error }}</div>
  <div v-if="loading" class="rounded-lg border border-base-300 bg-base-100 p-4">
    <span class="loading loading-spinner loading-sm"></span> Loading invoice
  </div>
  <InvoiceEditor v-else-if="invoice" :invoice="invoice" :customers="customers" :busy="saving" @save="saveInvoice" />
</template>
