import 'server-only';

import { SeverityNumber, logs } from '@opentelemetry/api-logs';

import { CredentialsMessage, LoginResultMessage } from '@/lib/auth-messages';
import AuthService from '@/lib/auth-service';

import { AuthServiceClient } from '@/protos/auth/AuthService';

import { GrpcClientFactory } from './grpc-client-factory';

const logger = logs.getLogger('grpc-auth-service');

export default class GrpcAuthService implements AuthService {
  private readonly client: AuthServiceClient;
  private readonly factory: GrpcClientFactory;

  constructor(factory = new GrpcClientFactory()) {
    this.factory = factory;
    this.client = factory.createAuthServiceClient();
  }

  async login(credentials: CredentialsMessage): Promise<LoginResultMessage> {
    try {
      return await new Promise<LoginResultMessage>((resolve, reject) => {
        this.client.Login(
          credentials,
          this.factory.createMetadata(),
          (err, response) => {
            if (err) {
              reject(err);
              return;
            }
            resolve({
              expirationInUtc: response!.expirationInUtc,
              token: response!.token,
            });
          },
        );
      });
    } catch (err) {
      logger.emit({
        body: `Login failed: ${(err as Error).message}`,
        severityNumber: SeverityNumber.ERROR,
        severityText: 'ERROR',
      });
      throw err;
    }
  }

  async register(credentials: CredentialsMessage): Promise<void> {
    try {
      await new Promise<void>((resolve, reject) => {
        this.client.Register(
          credentials,
          this.factory.createMetadata(),
          err => {
            if (err) {
              reject(err);
              return;
            }
            resolve();
          },
        );
      });
    } catch (err) {
      logger.emit({
        body: `Register failed: ${(err as Error).message}`,
        severityNumber: SeverityNumber.ERROR,
        severityText: 'ERROR',
      });
      throw err;
    }
  }
}
