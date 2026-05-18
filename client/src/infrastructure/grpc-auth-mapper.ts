import { LoginResultMessage } from '@/lib/auth-contracts';

import { LoginResponse__Output } from '@/protos/auth/LoginResponse';

export class GrpcAuthMapper {
  public mapLoginResult(response?: LoginResponse__Output): LoginResultMessage {
    return {
      expirationInUtc: response?.expirationInUtc ?? '',
      token: response?.token ?? '',
    };
  }
}
