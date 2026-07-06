<script setup lang="ts">
import { X } from '@lucide/vue';
import type { Customer, CustomerPayload } from '~/types/api';

const props = defineProps<{
  open: boolean;
  customer?: Customer | null;
  busy?: boolean;
}>();

const emit = defineEmits<{
  close: [];
  save: [payload: CustomerPayload];
}>();

const form = reactive<CustomerPayload>({
  name: '',
  email: null,
  phone: null,
  billingAddress: null
});

const title = computed(() => (props.customer ? 'Edit customer' : 'Create customer'));

watch(
  () => [props.open, props.customer] as const,
  () => {
    form.name = props.customer?.name || '';
    form.email = props.customer?.email || '';
    form.phone = props.customer?.phone || '';
    form.billingAddress = props.customer?.billingAddress || '';
  },
  { immediate: true }
);

function submit() {
  emit('save', {
    name: form.name.trim(),
    email: form.email?.trim() || null,
    phone: form.phone?.trim() || null,
    billingAddress: form.billingAddress?.trim() || null
  });
}
</script>

<template>
  <dialog class="modal modal-bottom sm:modal-middle" :class="{ 'modal-open': open }" @cancel.prevent="emit('close')">
    <div class="modal-box w-[calc(100%-1rem)] max-w-2xl overflow-hidden rounded-lg border border-slate-200 bg-white p-0 text-slate-950 shadow-xl">
      <header class="flex items-start justify-between gap-4 border-b border-slate-200 px-5 py-4">
        <div>
          <h3 class="text-lg font-semibold leading-6">{{ title }}</h3>
          <p class="mt-1 text-sm text-slate-500">Keep customer details ready for new invoices.</p>
        </div>
        <button class="btn btn-ghost btn-square min-h-11 shrink-0 cursor-pointer rounded-md" type="button" :disabled="busy" aria-label="Close customer modal" @click="emit('close')">
          <X :size="19" aria-hidden="true" />
        </button>
      </header>

      <form class="grid gap-5 px-5 py-5" @submit.prevent="submit">
        <label class="grid gap-1.5">
          <span class="text-sm font-medium text-slate-700">Name</span>
          <input
            v-model="form.name"
            class="input h-11 w-full rounded-md border-slate-300 bg-white text-slate-950 placeholder:text-slate-400 focus:border-slate-950 focus:outline-slate-950"
            required
            maxlength="256"
            placeholder="Customer name"
          />
        </label>

        <div class="grid gap-4 sm:grid-cols-2">
          <label class="grid gap-1.5">
            <span class="text-sm font-medium text-slate-700">Email</span>
            <input
              v-model="form.email"
              class="input h-11 w-full rounded-md border-slate-300 bg-white text-slate-950 placeholder:text-slate-400 focus:border-slate-950 focus:outline-slate-950"
              type="email"
              maxlength="256"
              placeholder="customer@example.com"
            />
          </label>
          <label class="grid gap-1.5">
            <span class="text-sm font-medium text-slate-700">Phone</span>
            <input
              v-model="form.phone"
              class="input h-11 w-full rounded-md border-slate-300 bg-white text-slate-950 placeholder:text-slate-400 focus:border-slate-950 focus:outline-slate-950"
              maxlength="32"
              placeholder="+1 555 0100"
            />
          </label>
        </div>

        <label class="grid gap-1.5">
          <span class="text-sm font-medium text-slate-700">Billing address</span>
          <textarea
            v-model="form.billingAddress"
            class="textarea min-h-28 w-full resize-y rounded-md border-slate-300 bg-white text-slate-950 placeholder:text-slate-400 focus:border-slate-950 focus:outline-slate-950"
            maxlength="512"
            placeholder="Street, city, state, ZIP"
          ></textarea>
        </label>

        <footer class="flex flex-col-reverse gap-2 border-t border-slate-200 pt-4 sm:flex-row sm:justify-end">
          <button class="btn min-h-11 cursor-pointer rounded-md border-slate-200 bg-white text-slate-700 hover:border-slate-300 hover:bg-slate-50" type="button" :disabled="busy" @click="emit('close')">
            Cancel
          </button>
          <button class="btn min-h-11 cursor-pointer rounded-md border-0 bg-slate-950 text-white hover:bg-slate-800" type="submit" :disabled="busy || !form.name.trim()">
            <span v-if="busy" class="loading loading-spinner loading-sm"></span>
            Save customer
          </button>
        </footer>
      </form>
    </div>
    <form method="dialog" class="modal-backdrop">
      <button type="button" aria-label="Close customer modal" @click="emit('close')">close</button>
    </form>
  </dialog>
</template>
