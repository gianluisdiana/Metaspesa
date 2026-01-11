import { Product } from '../Product';

export const ShoppingListTable = ({
  products,
  onToggle,
}: {
  products: Product[];
  onToggle: (name: string) => void;
}) => {
  return (
    <table>
      <thead>
        <ShoppingListHeader />
      </thead>
      <tbody>
        {products.map(product => (
          <ShoppingListProductRow
            key={product.name}
            product={product}
            onToggle={onToggle}
          />
        ))}
      </tbody>
      <tfoot>
        <ShoppingListTotalRow products={products} />
      </tfoot>
    </table>
  );
};

const ShoppingListProductRow = ({
  product,
  onToggle,
}: {
  product: Product;
  onToggle: (name: string) => void;
}) => {
  const price = product.price ? `${product.price} €` : 'N/A';
  return (
    <tr>
      <td>{product.name}</td>
      <td>{product.quantity}</td>
      <td>{price}</td>
      <td>
        <input
          type="checkbox"
          checked={product.checked}
          onChange={() => onToggle(product.name)}
        />
      </td>
    </tr>
  );
};

const ShoppingListTotalRow = ({ products }: { products: Product[] }) => {
  const totalAmount = products
    .filter(product => product.checked && product.price !== undefined)
    .reduce((total, product) => total + (product.price ?? 0), 0);

  return (
    <tr>
      <td>
        <strong>Total</strong>
      </td>
      <td></td>
      <td>{totalAmount} €</td>
    </tr>
  );
};

const ShoppingListHeader = () => {
  return (
    <tr>
      <th>Producto</th>
      <th>Cantidad</th>
      <th>Precio</th>
      <th>Marcado</th>
    </tr>
  );
};
