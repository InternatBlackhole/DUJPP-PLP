import { useApi } from '@/composables/useApi'
import type { UserInfo } from '@/api_wrapper/models'
import { firstValueFrom, throwError, timeout } from 'rxjs'
import { useSessionStorage } from '@vueuse/core'

const userInfoStorage = useSessionStorage<UserInfo | null>('user-info', null)
const lastAuthCheckStorage = useSessionStorage<number>('user-info-last-check', 0)
const AUTH_CACHE_MS = 5 * 60 * 1000 // 5 minutes

function getCookie(name: string): string | null {
  const match = document.cookie.match(new RegExp('(?:^|; )' + name.replace(/([.$?*|{}()\[\]\\\/\+^])/g, '\\$1') + '=([^;]*)'))
  return match ? decodeURIComponent(match[1]) : null
}

export function useUserInfo() {
  return userInfoStorage
}

export async function checkAuthentication(): Promise<boolean> {
  const aspIdentity = getCookie('Asp.Identity')
  if (!aspIdentity) {
    userInfoStorage.value = null
    return false
  }
  const now = Date.now()
  if (userInfoStorage.value && lastAuthCheckStorage.value && (now - lastAuthCheckStorage.value < AUTH_CACHE_MS)) {
    return true
  }
  const api = useApi()
  try {
    const userInfo = await firstValueFrom(api.auth.authWhoamiGet())
    userInfoStorage.value = userInfo
    lastAuthCheckStorage.value = now
    return !!userInfo
  } catch (error) {
    userInfoStorage.value = null
    lastAuthCheckStorage.value = 0
    return false
  }
}

export async function login(email: string, password: string): Promise<boolean> {
  const api = useApi()
  try {
    await firstValueFrom(
      api.auth.authLoginPost({
        loginRequest: { email, password },
        useCookies: true
      }).pipe(
        timeout({
          first: 5000,
          with: () => throwError(() => new Error('login request taking too long!')),
        })
      )
    )
    // After login, refresh user info
    await checkAuthentication()
    return true
  } catch (error) {
    return false
  }
}

export async function logout(): Promise<void> {
  const api = useApi()
  try {
    await firstValueFrom(api.auth.authLogoutPost())
  } catch (error) {
    // ignore
  }
  userInfoStorage.value = null
  lastAuthCheckStorage.value = 0
}
