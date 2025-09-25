import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import { provideApi } from './composables/useApi'

const app = createApp(App)
provideApi(app)
app.use(router)
app.mount('#app')
