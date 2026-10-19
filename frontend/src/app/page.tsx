"use client";

import { useCallback, useEffect, useState } from "react";
import {
  addToCart,
  checkout,
  clearCart,
  formatMoney,
  getCart,
  getProducts,
  removeCartItem,
  setCartQuantity,
} from "@/lib/api";
import type { Cart, CheckoutReceipt, Product } from "@/lib/types";

const emptyCart: Cart = { items: [], totalAmount: 0, itemCount: 0 };

export default function HomePage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [cart, setCart] = useState<Cart>(emptyCart);
  const [quantities, setQuantities] = useState<Record<string, number>>({});
  const [loading, setLoading] = useState(true);
  const [busyKey, setBusyKey] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [receipt, setReceipt] = useState<CheckoutReceipt | null>(null);

  const load = useCallback(async () => {
    const [productList, cartData] = await Promise.all([getProducts(), getCart()]);
    setProducts(productList);
    setCart(cartData);
    setQuantities((current) => {
      const next = { ...current };
      for (const product of productList) {
        if (!next[product.id]) {
          next[product.id] = 1;
        }
      }
      return next;
    });
  }, []);

  useEffect(() => {
    load()
      .catch((reason: unknown) => {
        setError(reason instanceof Error ? reason.message : "โหลดข้อมูลไม่สำเร็จ");
      })
      .finally(() => setLoading(false));
  }, [load]);

  async function run(key: string, action: () => Promise<Cart | void>) {
    setBusyKey(key);
    setError(null);
    try {
      const nextCart = await action();
      if (nextCart) {
        setCart(nextCart);
      }
      const productList = await getProducts();
      setProducts(productList);
    } catch (reason: unknown) {
      setError(reason instanceof Error ? reason.message : "ทำรายการไม่สำเร็จ");
    } finally {
      setBusyKey(null);
    }
  }

  return (
    <main className="mx-auto min-h-screen max-w-6xl px-4 py-8 md:px-6">
      <header className="mb-6 flex flex-col gap-4 border-b border-line pb-6 md:flex-row md:items-end md:justify-between">
        <div>
          <p className="text-sm font-semibold tracking-wide text-leaf">ร้านบ้านสวน</p>
          <h1 className="mt-1 text-3xl font-bold text-ink">สินค้า สต็อก และตะกร้า</h1>
          <p className="mt-2 max-w-2xl text-sm leading-6 text-muted">
            เพิ่มสินค้าลงตะกร้าเพื่อจองสต็อก ลด ลบ หรือล้างตะกร้าจะคืนสต็อก
            และเมื่อกดชำระเงินระบบจะแสดงยอดที่ต้องชำระพร้อมตัดสต็อกจริง
          </p>
        </div>
        <div className="rounded-2xl border border-line bg-card px-4 py-3 text-sm shadow-card">
          <p className="text-muted">สินค้าในตะกร้า</p>
          <p className="text-2xl font-bold">{cart.itemCount}</p>
        </div>
      </header>

      {error && (
        <div className="mb-4 rounded-xl border border-clay/30 bg-red-50 px-4 py-3 text-sm text-clay" role="alert">
          {error}
        </div>
      )}

      {loading ? (
        <p className="text-muted">กำลังโหลดสินค้า...</p>
      ) : (
        <div className="grid items-start gap-6 lg:grid-cols-[minmax(0,1fr)_360px]">
          <section>
            <h2 className="mb-3 text-lg font-semibold">รายการสินค้า</h2>
            <div className="grid gap-3 sm:grid-cols-2">
              {products.map((product) => {
                const quantity = quantities[product.id] ?? 1;
                const soldOut = product.availableQuantity <= 0;
                return (
                  <article key={product.id} className="rounded-2xl border border-line bg-card p-4 shadow-card">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <p className="text-xs font-semibold tracking-wide text-muted">{product.id}</p>
                        <h3 className="text-lg font-semibold">{product.name}</h3>
                      </div>
                      <p className="text-right font-bold text-clay">{formatMoney(product.unitPrice)}</p>
                    </div>
                    <p className="mt-1 text-sm text-muted">ราคาต่อ{product.unit}</p>
                    <dl className="mt-4 grid grid-cols-3 gap-2 text-center text-xs">
                      <div className="rounded-xl bg-paper px-2 py-2">
                        <dt className="text-muted">ในคลัง</dt>
                        <dd className="mt-1 text-base font-semibold">{product.stockQuantity}</dd>
                      </div>
                      <div className="rounded-xl bg-paper px-2 py-2">
                        <dt className="text-muted">จองแล้ว</dt>
                        <dd className="mt-1 text-base font-semibold">{product.reservedQuantity}</dd>
                      </div>
                      <div className="rounded-xl bg-leaf/10 px-2 py-2">
                        <dt className="text-leaf">พร้อมขาย</dt>
                        <dd className="mt-1 text-base font-semibold text-leaf">{product.availableQuantity}</dd>
                      </div>
                    </dl>
                    <div className="mt-4 flex items-center gap-2">
                      <label className="sr-only" htmlFor={`qty-${product.id}`}>
                        จำนวน {product.name}
                      </label>
                      <input
                        id={`qty-${product.id}`}
                        type="number"
                        min={1}
                        max={Math.max(product.availableQuantity, 1)}
                        value={quantity}
                        disabled={soldOut}
                        onChange={(event) => {
                          const next = Number(event.target.value);
                          setQuantities((current) => ({
                            ...current,
                            [product.id]: Number.isFinite(next) && next > 0 ? Math.floor(next) : 1,
                          }));
                        }}
                        className="w-20 rounded-xl border border-line bg-white px-3 py-2 text-center outline-none focus:border-leaf"
                      />
                      <button
                        type="button"
                        disabled={soldOut || busyKey !== null || quantity > product.availableQuantity}
                        onClick={() => run(`add-${product.id}`, () => addToCart(product.id, quantity))}
                        className="flex-1 rounded-xl bg-leaf px-3 py-2 text-sm font-semibold text-white hover:bg-[#184f34] disabled:bg-stone-300"
                      >
                        {soldOut ? "สินค้าหมด" : "เพิ่มลงตะกร้า"}
                      </button>
                    </div>
                  </article>
                );
              })}
            </div>
          </section>

          <aside className="rounded-2xl border border-line bg-card p-4 shadow-card lg:sticky lg:top-6">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold">ตะกร้าสินค้า</h2>
              <button
                type="button"
                disabled={cart.items.length === 0 || busyKey !== null}
                onClick={() => run("clear", () => clearCart())}
                className="text-sm font-medium text-clay disabled:text-stone-300"
              >
                ล้างทั้งหมด
              </button>
            </div>

            {cart.items.length === 0 ? (
              <p className="mt-6 rounded-xl bg-paper px-3 py-6 text-center text-sm text-muted">
                ยังไม่มีสินค้าในตะกร้า
              </p>
            ) : (
              <ul className="mt-4 space-y-3">
                {cart.items.map((item) => (
                  <li key={item.productId} className="rounded-xl border border-line p-3">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <p className="text-xs text-muted">{item.productId}</p>
                        <p className="font-semibold">{item.name}</p>
                        <p className="text-sm text-muted">
                          {formatMoney(item.unitPrice)} / {item.unit}
                        </p>
                      </div>
                      <p className="font-semibold">{formatMoney(item.lineTotal)}</p>
                    </div>
                    <div className="mt-3 flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <button
                          type="button"
                          aria-label={`ลด ${item.name}`}
                          disabled={busyKey !== null}
                          onClick={() =>
                            run(`dec-${item.productId}`, () =>
                              item.quantity === 1
                                ? removeCartItem(item.productId)
                                : setCartQuantity(item.productId, item.quantity - 1),
                            )
                          }
                          className="h-8 w-8 rounded-lg border border-line text-lg leading-none"
                        >
                          −
                        </button>
                        <span className="w-8 text-center font-semibold">{item.quantity}</span>
                        <button
                          type="button"
                          aria-label={`เพิ่ม ${item.name}`}
                          disabled={busyKey !== null || item.availableQuantity <= 0}
                          onClick={() =>
                            run(`inc-${item.productId}`, () => setCartQuantity(item.productId, item.quantity + 1))
                          }
                          className="h-8 w-8 rounded-lg border border-line text-lg leading-none disabled:text-stone-300"
                        >
                          +
                        </button>
                      </div>
                      <button
                        type="button"
                        disabled={busyKey !== null}
                        onClick={() => run(`del-${item.productId}`, () => removeCartItem(item.productId))}
                        className="text-sm text-muted underline-offset-2 hover:underline"
                      >
                        ลบ
                      </button>
                    </div>
                  </li>
                ))}
              </ul>
            )}

            <div className="mt-4 border-t border-line pt-4">
              <div className="flex items-center justify-between text-sm text-muted">
                <span>ยอดที่ต้องชำระ</span>
                <span className="text-xl font-bold text-ink">{formatMoney(cart.totalAmount)}</span>
              </div>
              <button
                type="button"
                disabled={cart.items.length === 0 || busyKey !== null}
                onClick={() =>
                  run("checkout", async () => {
                    const nextReceipt = await checkout();
                    setReceipt(nextReceipt);
                    return emptyCart;
                  })
                }
                className="mt-4 w-full rounded-xl bg-ink px-4 py-3 font-semibold text-white disabled:bg-stone-300"
              >
                ชำระเงิน
              </button>
            </div>
          </aside>
        </div>
      )}

      {receipt && (
        <div className="fixed inset-0 z-20 flex items-end justify-center bg-ink/40 p-4 sm:items-center">
          <section className="w-full max-w-md rounded-2xl bg-card p-5 shadow-card">
            <p className="text-sm font-semibold text-leaf">ชำระเงินสำเร็จ</p>
            <h2 className="mt-1 text-2xl font-bold">ใบเสร็จ #{receipt.orderId}</h2>
            <p className="mt-1 text-sm text-muted">
              {new Intl.DateTimeFormat("th-TH", {
                dateStyle: "medium",
                timeStyle: "short",
                timeZone: "Asia/Bangkok",
              }).format(new Date(receipt.checkedOutAt))}
            </p>
            <ul className="mt-4 space-y-2 text-sm">
              {receipt.items.map((item) => (
                <li key={item.productId} className="flex justify-between gap-3">
                  <span>
                    {item.productName} × {item.quantity} {item.unit}
                  </span>
                  <span>{formatMoney(item.lineTotal)}</span>
                </li>
              ))}
            </ul>
            <div className="mt-4 flex items-center justify-between border-t border-line pt-4">
              <span className="font-semibold">ยอดที่ต้องชำระ</span>
              <span className="text-2xl font-bold text-clay">{formatMoney(receipt.totalAmount)}</span>
            </div>
            <p className="mt-3 text-sm text-muted">สต็อกถูกตัดตามจำนวนในตะกร้าแล้ว</p>
            <button
              type="button"
              onClick={() => setReceipt(null)}
              className="mt-4 w-full rounded-xl bg-leaf px-4 py-3 font-semibold text-white"
            >
              ปิด
            </button>
          </section>
        </div>
      )}
    </main>
  );
}
