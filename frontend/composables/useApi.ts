import type { CustomerPayload, InvoicePayload, PagedResponse, Customer, Invoice, Tenant } from '~/types/api';

async function readApiError(error: unknown) {
  const fetchError = error as { data?: { message?: string; title?: string; errors?: Record<string, string[]> }; statusMessage?: string; message?: string };
  const data = fetchError.data;
  if (data?.message) return data.message;
  if (data?.errors) {
    return Object.entries(data.errors)
      .flatMap(([field, messages]) => messages.map((message) => `${field}: ${message}`))
      .join('\n');
  }
  return data?.title || fetchError.statusMessage || fetchError.message || 'Request failed.';
}

export function useApi() {
  const config = useRuntimeConfig();
  const { session } = useSession();

  async function request<T>(path: string, options: Parameters<typeof $fetch<T>>[1] = {}) {
    try {
      return await $fetch<T>(path, {
        baseURL: config.public.apiBase,
        ...options,
        headers: {
          'Content-Type': 'application/json',
          ...(session.value.tenantId ? { 'X-Tenant-Id': session.value.tenantId } : {}),
          ...(session.value.accessToken ? { Authorization: `Bearer ${session.value.accessToken}` } : {}),
          ...options?.headers
        }
      });
    } catch (error) {
      throw new Error(await readApiError(error));
    }
  }

  return {
    register: (payload: { email: string; password: string }) => request<{ tenantId: string; userId: string }>('/auth/register', { method: 'POST', body: payload }),
    login: (payload: { email: string; password: string }) => request<{ tokenType: string; accessToken: string; expiresIn: number; refreshToken: string }>('/login', { method: 'POST', body: payload }),
    myTenant: () => request<Tenant>('/me/tenant'),
    customers: () => request<PagedResponse<Customer>>('/customers?pageNumber=1&pageSize=50'),
    createCustomer: (payload: CustomerPayload) => request<Customer>('/customers', { method: 'POST', body: payload }),
    updateCustomer: (id: string, payload: CustomerPayload) => request<Customer>(`/customers/${id}`, { method: 'PUT', body: payload }),
    deleteCustomer: (id: string) => request<void>(`/customers/${id}`, { method: 'DELETE' }),
    invoices: () => request<PagedResponse<Invoice>>('/invoices?pageNumber=1&pageSize=50'),
    invoice: (id: string) => request<Invoice>(`/invoices/${id}`),
    createInvoice: (payload: InvoicePayload) => request<Invoice>('/invoices', { method: 'POST', body: payload }),
    updateInvoice: (id: string, payload: InvoicePayload) => request<Invoice>(`/invoices/${id}`, { method: 'PUT', body: payload }),
    deleteInvoice: (id: string) => request<void>(`/invoices/${id}`, { method: 'DELETE' })
  };
}
