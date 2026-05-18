/* eslint-disable @typescript-eslint/no-magic-numbers */
import { describe, expect, it } from 'vitest';

import { dollars, euros } from '@/lib/formatters/money-formatter';

describe('MoneyFormatter', () => {
  it('formats missing prices with a placeholder', () => {
    expect(dollars.format()).toBe('-');
  });

  it('formats dollar prices with two decimals', () => {
    expect(dollars.format(1.2)).toBe('$1.20');
  });

  it('formats euro prices with two decimals', () => {
    expect(euros.format(1.2)).toBe('€1.20');
  });

  it('rounds prices to two decimals', () => {
    expect(dollars.round(1.235)).toBe(1.24);
  });

  it('sums optional prices and rounds the result', () => {
    expect(dollars.sum([1.234, undefined, 2.345])).toBe(3.58);
  });
});
