import 'server-only';

import { SeverityNumber, logs } from '@opentelemetry/api-logs';

import { CredentialsMessage, LoginResultMessage } from '@/lib/auth-contracts';
import AuthService from '@/lib/auth-service';

import { AuthServiceClient } from '@/protos/auth/AuthService';

import { GrpcAuthMapper } from './grpc-auth-mapper';
import { GrpcClientFactory } from './grpc-client-factory';

const logger = logs.getLogger('grpc-auth-service');

export default class GrpcAuthService implements AuthService {
  private readonly client: AuthServiceClient;
  private readonly factory: GrpcClientFactory;
  private readonly mapper: GrpcAuthMapper;

  constructor(
    factory = new GrpcClientFactory(),
    mapper = new GrpcAuthMapper(),
  ) {
    this.factory = factory;
    this.client = factory.createAuthServiceClient();
    this.mapper = mapper;
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
            resolve(this.mapper.mapLoginResult(response));
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
