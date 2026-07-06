export default defineNuxtRouteMiddleware((to) => {
  const { session, loadSession } = useSession();

  if (!import.meta.client) return;

  loadSession();

  if (to.path !== '/auth' && !session.value.tenantId) {
    return navigateTo('/auth');
  }
});
