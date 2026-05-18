import { NextRequest } from 'next/server';

export const ALREADY_EXISTS = 6;

export class ShoppingListsRequest {
  public constructor(private readonly request: NextRequest) {}

  public get hasListName(): boolean {
    return this.request.nextUrl.searchParams.has('name');
  }

  public get listName(): string | undefined {
    const name = this.request.nextUrl.searchParams.get('name');
    return name && name.length > 0 ? name : undefined;
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
