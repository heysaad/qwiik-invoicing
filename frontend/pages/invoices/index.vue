<script setup lang="ts">
import { Edit, Plus, RefreshCw, Trash2 } from '@lucide/vue';
import type { Invoice } from '~/types/api';

definePageMeta({ middleware: 'auth' });

const api = useApi();
const invoices = ref<Invoice[]>([]);
const loading = ref(true);
const deleting = ref(false);
const error = ref('');
const confirmInvoice = ref<Invoice | null>(null);

async function loadInvoices() {
  loading.value = true;
  error.value = '';
  try {
    const result = await api.invoices();
    invoices.value = result.items;
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Could not load invoices.';
  } finally {
    loading.value = false;
  }
}

async function deleteInvoice() {
  if (!confirmInvoice.value) return;
  deleting.value = true;
  error.value = '';
  try {
    await api.deleteInvoice(confirmInvoice.value.id);
    confirmInvoice.value = null;
    await loadInvoices();
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Could not delete invoice.';
  } finally {
    deleting.value = false;
  }
}

onMounted(loadInvoices);
</script>

<template>
  <section class="rounded-lg border border-base-300 bg-base-100">
    <div class="flex flex-col gap-3 border-b border-base-300 p-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h2 class="font-semibold">Invoice register</h2>
        <p class="text-sm text-base-content/60">{{ invoices.length }} invoice{{ invoices.length === 1 ? '' : 's' }}</p>
      </div>
      <div class="flex gap-2">
        <button class="btn btn-ghost btn-sm min-h-10 cursor-pointer" :disabled="loading" @click="loadInvoices">
          <RefreshCw :size="16" aria-hidden="true" />
          Refresh
        </button>
        <NuxtLink to="/invoices/new" class="btn btn-primary btn-sm min-h-10 cursor-pointer">
          <Plus :size="16" aria-hidden="true" />
          Invoice
        </NuxtLink>
      </div>
    </div>

    <div v-if="error" class="alert alert-error m-4 rounded-lg text-sm whitespace-pre-line">{{ error }}</div>

    <div class="overflow-x-auto">
      <table class="table table-sm">
        <thead>
          <tr>
            <th>Number</th>
            <th>Customer</th>
            <th>Issue</th>
            <th>Due</th>
            <th>Status</th>
            <th class="text-right">Total</th>
            <th class="text-right">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td colspan="7"><span class="loading loading-spinner loading-sm"></span> Loading invoices</td>
          </tr>
          <tr v-else-if="invoices.length === 0">
            <td colspan="7" class="py-8 text-center text-base-content/60">No invoices yet. Create one when a customer is ready.</td>
          </tr>
          <tr v-for="invoice in invoices" v-else :key="invoice.id">
            <td class="font-medium">{{ invoice.invoiceNumber }}</td>
            <td>{{ invoice.customer?.name || '-' }}</td>
            <td>{{ invoice.issueDate }}</td>
            <td>{{ invoice.dueDate || '-' }}</td>
            <td><span class="badge badge-outline">{{ statusLabels[invoice.status] || invoice.status }}</span></td>
            <td class="text-right font-medium">{{ currency(invoice.total) }}</td>
            <td>
              <div class="flex justify-end gap-1">
                <NuxtLink :to="`/invoices/${invoice.id}/edit`" class="btn btn-ghost btn-square btn-sm min-h-10 cursor-pointer" aria-label="Edit invoice">
                  <Edit :size="16" aria-hidden="true" />
                </NuxtLink>
                <button class="btn btn-ghost btn-square btn-sm min-h-10 cursor-pointer text-error" aria-label="Delete invoice" @click="confirmInvoice = invoice">
                  <Trash2 :size="16" aria-hidden="true" />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>

  <ConfirmDialog
    :open="!!confirmInvoice"
    title="Delete invoice?"
    :message="`This will permanently delete invoice ${confirmInvoice?.invoiceNumber || ''}.`"
    confirm-text="Delete invoice"
    :busy="deleting"
    @cancel="confirmInvoice = null"
    @confirm="deleteInvoice"
  />
</template>
