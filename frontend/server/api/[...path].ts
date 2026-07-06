import { proxyRequest } from 'h3';

export default defineEventHandler((event) => {
  const config = useRuntimeConfig();
  const path = event.context.params?.path || '';
  return proxyRequest(event, `${config.apiOrigin}/${path}`);
});
