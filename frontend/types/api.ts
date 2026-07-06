export interface SessionState {
  tenantId: string;
  email: string;
  accessToken: string;
}

export interface Tenant {
  id: string;
  name: string;
  slug: string;
}

export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface Customer {
  id: string;
  tenantId: string;
  name: string;
  email?: string | null;
  phone?: string | null;
  billingAddress?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CustomerPayload {
  name: string;
  email: string | null;
  phone: string | null;
  billingAddress: string | null;
}

export interface InvoiceItemPayload {
  description: string;
  quantity: number;
  unitPrice: number;
}

export interface InvoiceItem extends InvoiceItemPayload {
  id: string;
  position: number;
  lineTotal: number;
}

export interface Invoice {
  id: string;
  tenantId: string;
  invoiceNumber: string;
  customer?: Customer | null;
  issueDate: string;
  dueDate?: string | null;
  subtotal: number;
  tax: number;
  total: number;
  status: number;
  notes?: string | null;
  items: InvoiceItem[];
  createdAt: string;
  updatedAt?: string | null;
}

export interface InvoicePayload {
  invoiceNumber: string;
  customerId: string;
  issueDate: string;
  dueDate: string | null;
  tax: number;
  status: number;
  notes: string | null;
  items: InvoiceItemPayload[];
}
