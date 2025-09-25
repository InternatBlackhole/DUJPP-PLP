import { useApi } from "@/composables/useApi";
import type { UserInfo } from "@/api_wrapper/models";
import { firstValueFrom } from "rxjs";
import { useSessionStorage } from "@vueuse/core";

const userInfoStorage = useSessionStorage<UserInfo | null>("user-info", null);

export function useUserInfo() {
  return userInfoStorage;
}

export async function checkAuthentication(): Promise<boolean> {
  const api = useApi();
  try {
    const userInfo = await firstValueFrom(api.auth.authWhoamiGet());
    userInfoStorage.value = userInfo;
    return !!userInfo;
  } catch (error) {
    console.error("Authentication check failed:", error);
    userInfoStorage.value = null;
    return false;
  }
}
