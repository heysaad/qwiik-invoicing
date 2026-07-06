<script setup lang="ts">
defineProps<{
  open: boolean;
  title?: string;
  message?: string;
  confirmText?: string;
  busy?: boolean;
}>();

const emit = defineEmits<{
  cancel: [];
  confirm: [];
}>();
</script>

<template>
  <dialog class="modal" :class="{ 'modal-open': open }">
    <div class="modal-box max-w-md rounded-lg">
      <h3 class="text-lg font-semibold">{{ title || 'Confirm action' }}</h3>
      <p class="mt-2 text-sm text-base-content/70">{{ message }}</p>
      <div class="modal-action">
        <button class="btn btn-ghost min-h-11 cursor-pointer" :disabled="busy" @click="emit('cancel')">Cancel</button>
        <button class="btn btn-error min-h-11 cursor-pointer" :disabled="busy" @click="emit('confirm')">
          <span v-if="busy" class="loading loading-spinner loading-sm"></span>
          {{ confirmText || 'Delete' }}
        </button>
      </div>
    </div>
  </dialog>
</template>
