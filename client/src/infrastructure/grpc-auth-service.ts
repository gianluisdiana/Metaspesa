import 'server-only';

import path from 'node:path';

import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';
import { SeverityNumber, logs } from '@opentelemetry/api-logs';

import { CredentialsMessage, LoginResultMessage } from '@/lib/auth-messages';
import AuthService from '@/lib/auth-service';

import { AuthServiceClient } from '@/protos/auth/AuthService';
import { ProtoGrpcType } from '@/protos/auth_service';

import { createTracingMetadata } from './grpc-metadata';

const logger = logs.getLogger('grpc-auth-service');

export default class GrpcAuthService implements AuthService {
  private readonly client: AuthServiceClient;

  constructor() {
    const protoPath = path.resolve(
      process.cwd(),
      'src/infrastructure/protos/Auth/auth_service.proto',
    );
    const packageDefinition = protoLoader.loadSync(protoPath);
    const { AuthService: AuthServiceConstructor } = (
      grpc.loadPackageDefinition(packageDefinition) as unknown as ProtoGrpcType
    ).Metaspesa.Protos.Auth;

    const credentials =
      process.env.BACKEND_SECURE === 'true'
        ? grpc.credentials.createSsl()
        : grpc.credentials.createInsecure();
    this.client = new AuthServiceConstructor(
      process.env.GRPC_SERVER_URL as string,
      credentials,
    );
  }

  async login(credentials: CredentialsMessage): Promise<LoginResultMessage> {
    try {
      return await new Promise<LoginResultMessage>((resolve, reject) => {
        this.client.Login(
          credentials,
          createTracingMetadata(),
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
        this.client.Register(credentials, createTracingMetadata(), err => {
          if (err) {
            reject(err);
            return;
          }
          resolve();
        });
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
