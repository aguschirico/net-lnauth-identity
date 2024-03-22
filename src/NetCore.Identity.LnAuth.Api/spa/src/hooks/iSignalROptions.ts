import {
  HubConnection,
  LogLevel,
  ILogger,
  HttpTransportType,
  IHubProtocol,
} from "@microsoft/signalr";

export interface iSignalROptions {
  onConnected?: (hub: HubConnection) => void;
  onDisconnected?: (error?: Error) => void;
  onReconnecting?: (error?: Error) => void;
  onReconnected?: (connectionId?: string) => void;
  onError?: (error?: Error) => void;
  enabled?: boolean;
  automaticReconnect?: number[] | boolean;
  httpTransportTypeOrOptions?: HttpTransportType;
  hubProtocol?: IHubProtocol;
  logging?: LogLevel | string | ILogger;
}

const DEFAULTS: iSignalROptions = {
  enabled: true,
};

export let defaultOptions: iSignalROptions = DEFAULTS;

export const setDefaults = (options: iSignalROptions) => {
  defaultOptions = {
    ...DEFAULTS,
    ...options,
  };
};
