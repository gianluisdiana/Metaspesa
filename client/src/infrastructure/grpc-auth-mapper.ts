import { LoginResultMessage } from '@/lib/auth-contracts';

import { LoginResponse__Output } from '@/protos/auth/LoginResponse';

import { requireGrpcResponse } from './grpc-response-guards';

export class GrpcAuthMapper {
  public mapLoginResult(response?: LoginResponse__Output): LoginResultMessage {
    const loginResponse = requireGrpcResponse(response, 'LoginResponse');
    return {
      expirationInUtc: loginResponse.expirationInUtc,
      token: loginResponse.token,
    };
  }
}
