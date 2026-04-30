import ProductCard, { type Product } from './product-card';

const PRODUCTS: Product[] = [
  {
    badge: { colorClass: 'text-primary', label: 'Organic' },
    category: 'Produce',
    hasQuantityControl: true,
    id: '1',
    imageAlt: 'Fresh organic navel oranges',
    imageUrl:
      'https://lh3.googleusercontent.com/aida-public/AB6AXuB2NIB0XuXVOAvXrCJkO2XGkqHJzKQVUjcHYRIqml6L9kS4x1444v5iwqCggXz-QL45ol9kvkOTC3NNcwLWodNeDwVNEyXibpMPIO2wYgb5w6mg-K-lD6btGgUGKIm0vMrd-nQDul_lMGmrMdK6PbROcshGkr2VbBW-nPVzbnYGDnWEbX3SeAtCUd3lBG09DxfYlqoAgz92TMoefdOLQOALA6ZDwSxfw1KU-Q5PCp6TDvqaKXBpLDCVoJGpp2QGb4v1UKtvKzffOTju',
    name: 'Navel Oranges',
    price: '$4.99',
    unit: 'per lb',
  },
  {
    category: 'Bakery',
    id: '2',
    imageAlt: 'Artisan sourdough bread loaf',
    imageUrl:
      'https://lh3.googleusercontent.com/aida-public/AB6AXuCen_CDkeGnN1SSHfYDBMxrJb-hqnwRIZgSL1fsw4eiBSSbNSsJ1ko6i_bzH9eyEWGwLcXUYMG_aLkOw93eFZaiG7KfwOcapSncP8OqexW1QbfgSxqSzevT6S8ytiHy3DT9dkZEMRZzYHvUNHnPsSQSKIbQhd2-I2520CZ5Viv1vdxTztrZSFIQ5D2G3T55ApbDe2v9JSG9XjVf7eykB-W-CTMzyekM-_AlSV63KvQhTN1WUSAsGMuVIOMQPxZE251WweY7Gv7oQWNX',
    name: 'Artisan Sourdough Loaf',
    price: '$6.49',
    unit: 'each',
  },
  {
    badge: { colorClass: 'text-primary', label: 'Sale' },
    category: 'Produce',
    id: '3',
    imageAlt: 'Hass Avocados',
    imageUrl:
      'https://lh3.googleusercontent.com/aida-public/AB6AXuCpEkOzYkz6A4HsfyXTZuPgCeq8uqRym8z1-OkRbxF7iktMouW-viwbib7nrwQR4RyMbjvaQdADoGK65xC1IVFZsGZZ206n4en4EXS-E4c9OCRqLeqkO1321QkRkC7yfzWxkZtnTGyvcA4E5JguHKflPeKERC1yuNYsZqpBaaRMYiOWBnagtGOcKc_wgfwY6NVd1WiSQIJudbxD691FwPa_b1k350xwhBNuSOfLV_O9uWeqDTQFDpot4TTz2nNG_0hdc1G8fcp-d0Km',
    name: 'Hass Avocados',
    originalPrice: '$2.49',
    price: '$1.99',
    unit: 'each',
  },
  {
    badge: { colorClass: 'text-tertiary', label: 'Local' },
    category: 'Dairy',
    id: '4',
    imageAlt: 'Whole Milk Glass Bottle',
    imageUrl:
      'https://lh3.googleusercontent.com/aida-public/AB6AXuAPUAQILdPnwMUcluVqI9tyTtXC0yI3Wz9pQnsbdpeIIvthdcaH6TVUIINNFD9ZVm3cs9nIz8Nb9Aoduz9BVfnj2D5_M2v4DTpIEcqnZt-oEUqaYtG2r03-_hZtXwks3ESKqHz3NuQSKK_zk6c-2UUqUBBH6votwMZevmiI37Imhh1nhuFYnGZkmbjAI5Q4Ici4jorRXxs60hbQTee0cSWODxhZWyMtJ3u4Yd8RBSAFliaDv_rOKBQpj0QAuRJo0WoiXVZ-ndWFQVh2',
    name: 'Creamline Whole Milk',
    price: '$5.29',
    unit: 'Half Gallon',
  },
];

export default function ProductGrid() {
  return (
    <div className="p-container-margin">
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-gutter">
        {PRODUCTS.map(product => (
          <ProductCard key={product.id} product={product} />
        ))}
      </div>
    </div>
  );
}
