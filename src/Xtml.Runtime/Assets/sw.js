/// <reference lib="webworker" />
/** @type {ServiceWorkerGlobalScope} */
const sw = self;
const clientMap = new Map();

sw.addEventListener("message", async (event) => {
  switch (event.data.type) {
    case "register-window":
      clientMap.set(event.source.id, event.data.window);
      break;
    case "unregister-window":
      clientMap.delete(event.source.id);
      break;
  }
});

sw.addEventListener('fetch', (event) => {
  const windowId = clientMap.get(event.clientId);
  if (
    // Ignore if window is not yet registered
    windowId &&
    // Same-origin only
    sw.location.origin === new URL(event.request.url).origin &&
    // HTML only, not scripts, styles, images, etc.
    event.request.destination === 'document'
  ) {
    event.respondWith(fetch(new Request(event.request, { 
      mode: 'same-origin',
      headers: {
        ...Object.fromEntries(event.request.headers),
        'x-window': windowId,
      },
    })));
  }
});
