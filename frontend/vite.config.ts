import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const backendOrigin = env.VITE_BACKEND_ORIGIN || 'http://localhost:5000';

  return {
    plugins: [react()],
    optimizeDeps: {
      include: ['react', 'react-dom', 'react-router-dom', 'antd', '@tanstack/react-query', 'axios'],
    },
    build: {
      sourcemap: false,
      chunkSizeWarningLimit: 1200,
      rollupOptions: {
        output: {
          manualChunks(id) {
            const normalizedId = id.replace(/\\/g, '/');

            if (!normalizedId.includes('node_modules')) {
              return undefined;
            }

            if (
              normalizedId.includes('/react/') ||
              normalizedId.includes('/react-dom/') ||
              normalizedId.includes('/react-router-dom/')
            ) {
              return 'react';
            }

            if (normalizedId.includes('/@tanstack/react-query/')) {
              return 'query';
            }

            if (normalizedId.includes('/axios/')) {
              return 'axios';
            }

            return undefined;
          },
        },
      },
    },
    server: {
      port: 5173,
      host: true,
      proxy: {
        '/api': {
          target: backendOrigin,
          changeOrigin: true,
          secure: false,
        },
        '/health': {
          target: backendOrigin,
          changeOrigin: true,
          secure: false,
        },
      },
    },
  };
});
