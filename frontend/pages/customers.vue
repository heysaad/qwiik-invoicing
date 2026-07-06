<script setup lang="ts">
import { Edit, Plus, RefreshCw, Trash2 } from '@lucide/vue';
import type { Customer, CustomerPayload } from '~/types/api';

definePageMeta({ middleware: 'auth' });

const api = useApi();
const customers = ref<Customer[]>([]);
const loading = ref(true);
const saving = ref(false);
const deleting = ref(false);
const error = ref('');
const modalOpen = ref(false);
const editingCustomer = ref<Customer | null>(null);
const confirmCustomer = ref<Customer | null>(null);

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

function openCreate() {
  editingCustomer.value = null;
  modalOpen.value = true;
}

function openEdit(customer: Customer) {
  editingCustomer.value = customer;
  modalOpen.value = true;
}

async function saveCustomer(payload: CustomerPayload) {
  saving.value = true;
  error.value = '';
  try {
    if (editingCustomer.value) {
      await api.updateCustomer(editingCustomer.value.id, payload);
    } else {
      await api.createCustomer(payload);
    }
    modalOpen.value = false;
    await loadCustomers();
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Could not save customer.';
  } finally {
    saving.value = false;
  }
}

async function deleteCustomer() {
  if (!confirmCustomer.value) return;
  deleting.value = true;
  error.value = '';
  try {
    await api.deleteCustomer(confirmCustomer.value.id);
    confirmCustomer.value = null;
    await loadCustomers();
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Could not delete customer.';
  } finally {
    deleting.value = false;
  }
}

onMounted(loadCustomers);
</script>

<template>
  <section class="rounded-lg border border-base-300 bg-base-100">
    <div class="flex flex-col gap-3 border-b border-base-300 p-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h2 class="font-semibold">Customer records</h2>
        <p class="text-sm text-base-content/60">{{ customers.length }} customer{{ customers.length === 1 ? '' : 's' }}</p>
      </div>
      <div class="flex gap-2">
        <button class="btn btn-ghost btn-sm min-h-10 cursor-pointer" :disabled="loading" @click="loadCustomers">
          <RefreshCw :size="16" aria-hidden="true" />
          Refresh
        </button>
        <button class="btn btn-primary btn-sm min-h-10 cursor-pointer" @click="openCreate">
          <Plus :size="16" aria-hidden="true" />
          Customer
        </button>
      </div>
    </div>

    <div v-if="error" class="alert alert-error m-4 rounded-lg text-sm whitespace-pre-line">{{ error }}</div>

    <div class="overflow-x-auto">
      <table class="table table-sm">
        <thead>
          <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Phone</th>
            <th>Billing address</th>
            <th class="text-right">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td colspan="5"><span class="loading loading-spinner loading-sm"></span> Loading customers</td>
          </tr>
          <tr v-else-if="customers.length === 0">
            <td colspan="5" class="py-8 text-center text-base-content/60">Create your first customer to start invoicing.</td>
          </tr>
          <tr v-for="customer in customers" v-else :key="customer.id">
            <td class="font-medium">{{ customer.name }}</td>
            <td>{{ customer.email || '-' }}</td>
            <td>{{ customer.phone || '-' }}</td>
            <td class="max-w-xs truncate">{{ customer.billingAddress || '-' }}</td>
            <td>
              <div class="flex justify-end gap-1">
                <button class="btn btn-ghost btn-square btn-sm min-h-10 cursor-pointer" aria-label="Edit customer" @click="openEdit(customer)">
                  <Edit :size="16" aria-hidden="true" />
                </button>
                <button class="btn btn-ghost btn-square btn-sm min-h-10 cursor-pointer text-error" aria-label="Delete customer" @click="confirmCustomer = customer">
                  <Trash2 :size="16" aria-hidden="true" />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>

  <CustomerModal :open="modalOpen" :customer="editingCustomer" :busy="saving" @close="modalOpen = false" @save="saveCustomer" />
  <ConfirmDialog
    :open="!!confirmCustomer"
    title="Delete customer?"
    :message="`This will permanently delete ${confirmCustomer?.name || 'this customer'}. Customers with invoices cannot be deleted by the API.`"
    confirm-text="Delete customer"
    :busy="deleting"
    @cancel="confirmCustomer = null"
    @confirm="deleteCustomer"
  />
</template>
