import type { Metadata } from "next";
import { Sarabun } from "next/font/google";
import "./globals.css";

const sarabun = Sarabun({
  subsets: ["thai", "latin"],
  weight: ["400", "500", "600", "700"],
  variable: "--font-sarabun",
});

export const metadata: Metadata = {
  title: "ร้านบ้านสวน | สต็อกและตะกร้าสินค้า",
  description: "แสดงสินค้า จองสต็อกผ่านตะกร้า และชำระเงินเพื่อตัดสต็อก",
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="th">
      <body className={`${sarabun.variable} font-sans antialiased`}>{children}</body>
    </html>
  );
}
