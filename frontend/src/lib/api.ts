import type { Cart, CheckoutReceipt, Product } from "./types";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5120";

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_URL}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(init?.headers ?? {}),
    },
    cache: "no-store",
  });

  if (!response.ok) {
    const body = (await response.json().catch(() => null)) as { message?: string } | null;
    throw new Error(body?.message ?? "ไม่สามารถติดต่อเซิร์ฟเวอร์ได้");
  }

  return response.json() as Promise<T>;
}

export function getProducts() {
  return request<Product[]>("/api/products");
}

export function getCart() {
  return request<Cart>("/api/cart");
}

export function addToCart(productId: string, quantity: number) {
  return request<Cart>("/api/cart/items", {
    method: "POST",
    body: JSON.stringify({ productId, quantity }),
  });
}

export function setCartQuantity(productId: string, quantity: number) {
  return request<Cart>(`/api/cart/items/${productId}`, {
    method: "PUT",
    body: JSON.stringify({ quantity }),
  });
}

export function removeCartItem(productId: string) {
  return request<Cart>(`/api/cart/items/${productId}`, { method: "DELETE" });
}

export function clearCart() {
  return request<Cart>("/api/cart", { method: "DELETE" });
}

export function checkout() {
  return request<CheckoutReceipt>("/api/cart/checkout", { method: "POST" });
}

export function formatMoney(amount: number) {
  return new Intl.NumberFormat("th-TH", {
    style: "currency",
    currency: "THB",
    minimumFractionDigits: 2,
  }).format(amount);
}
