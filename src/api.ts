import { invoke } from '@tauri-apps/api/core'

export type PluginRequest = {
  controller: string
  action: string
  data?: object
}

export type RouteResponse<T> = {
  errorMessage?: string
  data?: T
}

export class Api {
  public static async invokePlugin<T>(request: PluginRequest): Promise<T | null> {
    let response = (await invoke('dotnet_request', { request: JSON.stringify(request) })) as string
    let jsonResponse = JSON.parse(response) as RouteResponse<T>

    if (jsonResponse.errorMessage) throw new Error(jsonResponse.errorMessage)

    return jsonResponse.data ?? (null as T | null)
  }
}
