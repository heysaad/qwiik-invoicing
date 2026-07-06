export const statusLabels = ['Draft', 'Sent', 'Paid', 'Overdue', 'Cancelled'];

export function currency(value: number | string | null | undefined) {
  return new Intl.NumberFormat(undefined, {
    style: 'currency',
    currency: 'USD',
    maximumFractionDigits: 2
  }).format(Number(value || 0));
}

export function shortId(value: string | null | undefined) {
  return value ? value.slice(0, 8) : '';
}

export function todayIso() {
  return new Date().toISOString().slice(0, 10);
}

export function addDaysIso(value: string, days: number) {
  const date = new Date(`${value}T00:00:00`);
  date.setDate(date.getDate() + days);
  return date.toISOString().slice(0, 10);
}
