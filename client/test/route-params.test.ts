/* eslint-disable @typescript-eslint/no-magic-numbers */
import { MarketProductsRequest } from '@/app/api/markets/products/market-products-request';
import {
  GrpcStatusError,
  ShoppingListsRequest,
} from '@/app/api/shopping/lists/shopping-lists-request';
import { NextRequest } from 'next/server';
import { describe, expect, it } from 'vitest';

describe('MarketProductsRequest', () => {
  it('parses string filters and positive pagination values', () => {
    const request = new MarketProductsRequest(
      new URLSearchParams({
        brand_name: 'brand',
        market_name: 'market',
        name_segment: 'name',
        page: '3',
        page_size: '15',
      }),
    );

    expect(request.toFilter()).toEqual({
      brandNameSegment: 'brand',
      marketName: 'market',
      nameSegment: 'name',
      page: 3,
      pageSize: 15,
    });
  });

  it('normalizes empty strings and invalid pagination values', () => {
    const request = new MarketProductsRequest(
      new URLSearchParams({
        brand_name: '',
        page: '-1',
        page_size: 'abc',
      }),
    );

    expect(request.toFilter()).toEqual({
      brandNameSegment: undefined,
      marketName: undefined,
      nameSegment: undefined,
      page: 1,
      pageSize: 20,
    });
  });
});

describe('ShoppingListsRequest', () => {
  it('detects when list name is not requested', () => {
    const request = new ShoppingListsRequest(
      new NextRequest('http://localhost/api/shopping/lists'),
    );

    expect(request.hasListName).toBe(false);
    expect(request.listName).toBeUndefined();
  });

  it('normalizes empty and named list names', () => {
    const emptyNameRequest = new ShoppingListsRequest(
      new NextRequest('http://localhost/api/shopping/lists?name='),
    );
    const namedRequest = new ShoppingListsRequest(
      new NextRequest('http://localhost/api/shopping/lists?name=Groceries'),
    );

    expect(emptyNameRequest.hasListName).toBe(true);
    expect(emptyNameRequest.listName).toBeUndefined();
    expect(namedRequest.hasListName).toBe(true);
    expect(namedRequest.listName).toBe('Groceries');
  });
});

describe('GrpcStatusError', () => {
  it('extracts numeric error codes', () => {
    expect(new GrpcStatusError({ code: '6' }).code).toBe(6);
  });

  it('detects already exists errors', () => {
    expect(new GrpcStatusError({ code: 6 }).alreadyExists).toBe(true);
    expect(new GrpcStatusError({ code: 13 }).alreadyExists).toBe(false);
    expect(new GrpcStatusError(new Error('failed')).alreadyExists).toBe(false);
  });
});
