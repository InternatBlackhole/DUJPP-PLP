import { inject, type App } from 'vue'
import { Configuration } from '@/api_wrapper'
import { AuthenticationApi, LinijeApi, PogodbeApi, ZapisiApi } from '@/api_wrapper/apis'

export interface ApiServices {
  auth: AuthenticationApi
  linije: LinijeApi
  pogodbe: PogodbeApi
  zapisi: ZapisiApi
}

const apiServicesKey = Symbol('api-services')

function createApiServices(): ApiServices {
  const config = new Configuration({
    basePath: import.meta.env.VITE_API_BASE_URL || 'http://127.0.0.1:5000/api'
  })

  return {
    auth: new AuthenticationApi(config),
    linije: new LinijeApi(config),
    pogodbe: new PogodbeApi(config),
    zapisi: new ZapisiApi(config)
  }
}

export function provideApi(app: App<unknown>) {
  const services = createApiServices()
  app.provide(apiServicesKey, services)
}

export function useApi(): ApiServices {
  const services = inject<ApiServices>(apiServicesKey)
  if (!services) {
    throw new Error('API services have not been provided')
  }
  return services
}
