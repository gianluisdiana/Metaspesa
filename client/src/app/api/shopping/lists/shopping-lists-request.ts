import { NextRequest } from 'next/server';

import { hasParam, stringParam } from '@/lib/search-params';

export const ALREADY_EXISTS = 6;

export class ShoppingListsRequest {
  public constructor(private readonly request: NextRequest) {}

  public get hasListName(): boolean {
    return hasParam(this.request.nextUrl.searchParams, 'name');
  }

  public get listName(): string | undefined {
    return stringParam(this.request.nextUrl.searchParams, 'name');
  }
}

export class GrpcStatusError {
  public constructor(private readonly error: unknown) {}

  public get code(): number | undefined {
    if (
      typeof this.error !== 'object' ||
      this.error === null ||
      !('code' in this.error)
    ) {
      return undefined;
    }

    return Number(this.error.code);
  }

  public get alreadyExists(): boolean {
    return this.code === ALREADY_EXISTS;
  }
}
