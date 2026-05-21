import { describe, expect, it } from 'vitest';

import { GrpcMarketMapper } from '@/infrastructure/grpc-market-mapper';

describe('GrpcMarketMapper', () => {
  const mapper = new GrpcMarketMapper();

  it('maps missing markets to an empty collection', () => {
    expect(mapper.mapMarkets()).toEqual([]);
  });

  it('maps market products and normalizes empty image urls', () => {
    expect(
      mapper.mapMarket({
        name: 'Mercadona',
        products: [
          {
            brandName: 'Brand',
            formats: [
              {
                imageUrl: '',
                price: '1.5',
                quantity: '1 kg',
              },
            ],
            name: 'Apples',
          },
        ],
      }),
    ).toEqual({
      name: 'Mercadona',
      products: [
        {
          brandName: 'Brand',
          formats: [
            {
              imageUrl: null,
              price: 1.5,
              quantity: '1 kg',
            },
          ],
          name: 'Apples',
        },
      ],
    });
  });

  it('maps market summaries and normalizes empty logo urls', () => {
    expect(
      mapper.mapMarketSummaries([{ logoUrl: '', name: 'Mercadona' }]),
    ).toEqual([{ logoUrl: null, name: 'Mercadona' }]);
  });
});
