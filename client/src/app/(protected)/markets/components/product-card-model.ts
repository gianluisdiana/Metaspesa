export type Product = {
  id: string;
  category: string;
  name: string;
  price: string;
  unit: string;
  imageUrl: string;
  imageAlt: string;
  badge?: { label: string; colorClass: string };
  originalPrice?: string;
  hasQuantityControl?: boolean;
};
