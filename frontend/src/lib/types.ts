export type Product = {
  id: string;
  name: string;
  unitPrice: number;
  unit: string;
  stockQuantity: number;
  reservedQuantity: number;
  availableQuantity: number;
};

export type CartItem = {
  productId: string;
  name: string;
  unitPrice: number;
  unit: string;
  quantity: number;
  lineTotal: number;
  availableQuantity: number;
};

export type Cart = {
  items: CartItem[];
  totalAmount: number;
  itemCount: number;
};

export type CheckoutItem = {
  productId: string;
  productName: string;
  unitPrice: number;
  unit: string;
  quantity: number;
  lineTotal: number;
};

export type CheckoutReceipt = {
  orderId: number;
  totalAmount: number;
  checkedOutAt: string;
  items: CheckoutItem[];
};
