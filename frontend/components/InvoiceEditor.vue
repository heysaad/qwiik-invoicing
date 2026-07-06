<script setup lang="ts">
import { ArrowLeft, Plus, Save, Trash2 } from '@lucide/vue';
import type { Customer, Invoice, InvoiceItemPayload, InvoicePayload } from '~/types/api';

const props = defineProps<{
  invoice?: Invoice | null;
  customers: Customer[];
  busy?: boolean;
}>();

const emit = defineEmits<{
  save: [payload: InvoicePayload];
}>();

interface InvoiceForm {
  invoiceNumber: string;
  customerId: string;
  issueDate: string;
  dueDate: string;
  tax: number;
  status: number;
  notes: string;
  items: InvoiceItemPayload[];
}

const form = reactive<InvoiceForm>({
  invoiceNumber: '',
  customerId: '',
  issueDate: todayIso(),
  dueDate: addDaysIso(todayIso(), 15),
  tax: 0,
  status: 0,
  notes: '',
  items: [{ description: '', quantity: 1, unitPrice: 0 }]
});

const subtotal = computed(() => form.items.reduce((sum, item) => sum + Number(item.quantity || 0) * Number(item.unitPrice || 0), 0));
const total = computed(() => subtotal.value + Number(form.tax || 0));
const canSave = computed(() => form.invoiceNumber.trim() && form.customerId && form.issueDate && form.items.length > 0 && form.items.every((item) => item.description.trim() && Number(item.quantity) > 0));

function lineTotal(item: InvoiceItemPayload) {
  return Number(item.quantity || 0) * Number(item.unitPrice || 0);
}

watch(
  () => props.invoice,
  (invoice) => {
    form.invoiceNumber = invoice?.invoiceNumber || '';
    form.customerId = invoice?.customer?.id || props.customers[0]?.id || '';
    form.issueDate = invoice?.issueDate || todayIso();
    form.dueDate = invoice?.dueDate || addDaysIso(form.issueDate, 15);
    form.tax = Number(invoice?.tax || 0);
    form.status = Number(invoice?.status || 0);
    form.notes = invoice?.notes || '';
    form.items = invoice?.items?.length
      ? invoice.items.map((item) => ({ description: item.description, quantity: item.quantity, unitPrice: item.unitPrice }))
      : [{ description: '', quantity: 1, unitPrice: 0 }];
  },
  { immediate: true }
);

watch(
  () => form.issueDate,
  (issueDate, previousIssueDate) => {
    if (props.invoice?.dueDate || !issueDate) return;
    const previousDefault = previousIssueDate ? addDaysIso(previousIssueDate, 15) : '';
    if (!form.dueDate || form.dueDate === previousDefault) {
      form.dueDate = addDaysIso(issueDate, 15);
    }
  }
);

watch(
  () => props.customers,
  (customers) => {
    const firstCustomer = customers[0];
    if (!form.customerId && firstCustomer) form.customerId = firstCustomer.id;
  },
  { immediate: true }
);

function addItem() {
  form.items.push({ description: '', quantity: 1, unitPrice: 0 });
}

function removeItem(index: number) {
  if (form.items.length === 1) return;
  form.items.splice(index, 1);
}

function submit() {
  emit('save', {
    invoiceNumber: form.invoiceNumber.trim(),
    customerId: form.customerId,
    issueDate: form.issueDate,
    dueDate: form.dueDate || null,
    tax: Number(form.tax || 0),
    status: Number(form.status),
    notes: form.notes.trim() || null,
    items: form.items.map((item) => ({
      description: item.description.trim(),
      quantity: Number(item.quantity || 0),
      unitPrice: Number(item.unitPrice || 0)
    }))
  });
}
</script>

<template>
  <form class="grid gap-5 xl:grid-cols-[minmax(0,1fr)_22rem]" @submit.prevent="submit">
    <div class="grid min-w-0 gap-5">
      <section class="overflow-hidden rounded-lg border border-slate-200 bg-white">
        <header class="border-b border-slate-200 px-5 py-4">
          <h2 class="text-base font-semibold text-slate-950">Invoice details</h2>
          <p class="mt-1 text-sm text-slate-500">Choose the customer, invoice date, and billing status.</p>
        </header>

        <div class="grid gap-4 p-5 md:grid-cols-4">
          <label class="grid gap-1.5 md:col-span-2">
          <span class="text-sm font-medium text-slate-700">Invoice number</span>
          <input
            v-model="form.invoiceNumber"
            class="input h-11 w-full rounded-md border-slate-300 bg-white text-slate-950 placeholder:text-slate-400 focus:border-slate-950 focus:outline-slate-950"
            required
            maxlength="64"
            placeholder="INV-001"
          />
        </label>
          <label class="grid gap-1.5 md:col-span-2">
          <span class="text-sm font-medium text-slate-700">Customer</span>
          <select v-model="form.customerId" class="select h-11 w-full rounded-md border-slate-300 bg-white text-slate-950 focus:border-slate-950 focus:outline-slate-950" required>
            <option disabled value="">Select customer</option>
            <option v-for="customer in customers" :key="customer.id" :value="customer.id">{{ customer.name }}</option>
          </select>
        </label>
          <label class="grid gap-1.5">
          <span class="text-sm font-medium text-slate-700">Issue date</span>
          <input v-model="form.issueDate" class="input h-11 w-full rounded-md border-slate-300 bg-white text-slate-950 focus:border-slate-950 focus:outline-slate-950" type="date" required />
        </label>
          <label class="grid gap-1.5">
          <span class="text-sm font-medium text-slate-700">Due date</span>
          <input v-model="form.dueDate" class="input h-11 w-full rounded-md border-slate-300 bg-white text-slate-950 focus:border-slate-950 focus:outline-slate-950" type="date" />
        </label>
          <label class="grid gap-1.5">
          <span class="text-sm font-medium text-slate-700">Status</span>
          <select v-model.number="form.status" class="select h-11 w-full rounded-md border-slate-300 bg-white text-slate-950 focus:border-slate-950 focus:outline-slate-950">
            <option v-for="(label, index) in statusLabels" :key="label" :value="index">{{ label }}</option>
          </select>
        </label>
          <label class="grid gap-1.5">
          <span class="text-sm font-medium text-slate-700">Tax</span>
          <input v-model.number="form.tax" class="input h-11 w-full rounded-md border-slate-300 bg-white text-slate-950 focus:border-slate-950 focus:outline-slate-950" type="number" min="0" step="0.01" />
        </label>
      </div>
      </section>

      <section class="overflow-hidden rounded-lg border border-slate-200 bg-white">
        <header class="flex flex-col gap-3 border-b border-slate-200 px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 class="text-base font-semibold text-slate-950">Line items</h2>
            <p class="mt-1 text-sm text-slate-500">Add billable work, quantity, price, and review each line total.</p>
          </div>
          <button class="btn min-h-10 cursor-pointer rounded-md border-slate-200 bg-white text-slate-700 hover:border-slate-300 hover:bg-slate-50" type="button" @click="addItem">
            <Plus :size="16" aria-hidden="true" />
            Item
          </button>
        </header>

        <div class="hidden border-b border-slate-200 bg-slate-50 px-5 py-2 text-xs font-semibold uppercase text-slate-500 md:grid md:grid-cols-[minmax(0,1fr)_7rem_9rem_9rem_3rem] md:gap-3">
          <div>Description</div>
          <div>Qty</div>
          <div>Unit price</div>
          <div class="text-right">Amount</div>
          <div></div>
        </div>

        <div class="divide-y divide-slate-200">
          <div v-for="(item, index) in form.items" :key="index" class="grid gap-3 px-5 py-4 md:grid-cols-[minmax(0,1fr)_7rem_9rem_9rem_3rem] md:items-end">
            <label class="grid gap-1.5">
              <span class="text-sm font-medium text-slate-700 md:hidden">Description</span>
              <input
                v-model="item.description"
                class="input h-11 w-full rounded-md border-slate-300 bg-white text-slate-950 placeholder:text-slate-400 focus:border-slate-950 focus:outline-slate-950"
                required
                maxlength="256"
                placeholder="Item or service"
              />
            </label>

            <label class="grid gap-1.5">
              <span class="text-sm font-medium text-slate-700 md:hidden">Qty</span>
              <input
                v-model.number="item.quantity"
                class="input h-11 w-full rounded-md border-slate-300 bg-white text-slate-950 focus:border-slate-950 focus:outline-slate-950"
                type="number"
                min="0.01"
                step="0.01"
                required
              />
            </label>

            <label class="grid gap-1.5">
              <span class="text-sm font-medium text-slate-700 md:hidden">Unit price</span>
              <input
                v-model.number="item.unitPrice"
                class="input h-11 w-full rounded-md border-slate-300 bg-white text-slate-950 focus:border-slate-950 focus:outline-slate-950"
                type="number"
                min="0"
                step="0.01"
                required
              />
            </label>

            <div class="grid min-h-11 content-center rounded-md bg-slate-50 px-3 text-right">
              <span class="text-xs font-medium text-slate-500 md:hidden">Amount</span>
              <span class="font-semibold text-slate-950">{{ currency(lineTotal(item)) }}</span>
            </div>

            <div class="flex justify-end md:justify-center">
              <button class="btn btn-ghost btn-square min-h-11 cursor-pointer rounded-md text-error" type="button" aria-label="Remove item" :disabled="form.items.length === 1" @click="removeItem(index)">
                <Trash2 :size="17" aria-hidden="true" />
              </button>
            </div>
          </div>
        </div>
      </section>
    </div>

    <aside class="h-fit rounded-lg border border-slate-200 bg-white p-5 xl:sticky xl:top-20">
      <h2 class="text-base font-semibold text-slate-950">Summary</h2>
      <dl class="mt-4 grid gap-3 text-sm">
        <div class="flex justify-between gap-4"><dt class="text-slate-500">Subtotal</dt><dd class="font-medium text-slate-950">{{ currency(subtotal) }}</dd></div>
        <div class="flex justify-between gap-4"><dt class="text-slate-500">Tax</dt><dd class="font-medium text-slate-950">{{ currency(form.tax) }}</dd></div>
        <div class="h-px bg-slate-200"></div>
        <div class="flex justify-between gap-4 text-base"><dt class="font-semibold text-slate-950">Total</dt><dd class="font-semibold text-slate-950">{{ currency(total) }}</dd></div>
      </dl>

      <label class="mt-5 grid gap-1.5">
        <span class="text-sm font-medium text-slate-700">Notes</span>
        <textarea
          v-model="form.notes"
          class="textarea min-h-32 w-full resize-y rounded-md border-slate-300 bg-white text-slate-950 placeholder:text-slate-400 focus:border-slate-950 focus:outline-slate-950"
          maxlength="1024"
          placeholder="Optional payment terms or customer notes"
        ></textarea>
      </label>

      <div class="mt-5 grid gap-2">
        <button class="btn min-h-11 cursor-pointer rounded-md border-0 bg-slate-950 text-white hover:bg-slate-800" type="submit" :disabled="busy || !canSave">
          <span v-if="busy" class="loading loading-spinner loading-sm"></span>
          <Save :size="16" aria-hidden="true" />
          Save invoice
        </button>
        <NuxtLink to="/invoices" class="btn min-h-11 cursor-pointer rounded-md border-slate-200 bg-white text-slate-700 hover:border-slate-300 hover:bg-slate-50">
          <ArrowLeft :size="16" aria-hidden="true" />
          Back to invoices
        </NuxtLink>
      </div>
    </aside>
  </form>
</template>
