# ร้านบ้านสวน — สต็อกสินค้าและตะกร้า

ระบบแสดงสินค้า มีสต็อก และตะกร้าสินค้า

- Frontend: Next.js
- Backend: ASP.NET Core (.NET) REST API
- ฐานข้อมูล: SQLite ผ่าน Entity Framework Core พร้อมข้อมูลสินค้าและสต็อกตั้งต้น

## กติกาสต็อก

- เพิ่มสินค้าลงตะกร้าได้เฉพาะเมื่อจำนวนพร้อมขายพอ ระบบจะจองสต็อกทันที
- เพิ่ม ลด ลบ หรือล้างรายการในตะกร้า จะปรับจำนวนที่จองในสต็อกให้ตรงกับตะกร้า
- กดชำระเงินแล้ว ระบบแสดงยอดที่ต้องชำระ ตัดจำนวนในคลังตามรายการในตะกร้า และล้างตะกร้า

## รันระบบ

เปิดสองเทอร์มินัล

```powershell
cd backend
dotnet run --launch-profile http
```

```powershell
cd frontend
npm install
npm run dev
```

เปิด http://localhost:3000

API อยู่ที่ http://localhost:5120

## API

| Method | Path | ความหมาย |
| --- | --- | --- |
| GET | /api/products | รายการสินค้าและสต็อก |
| GET | /api/cart | ตะกร้า |
| POST | /api/cart/items | เพิ่มสินค้า `{ productId, quantity }` |
| PUT | /api/cart/items/{productId} | กำหนดจำนวน `{ quantity }` |
| DELETE | /api/cart/items/{productId} | ลบรายการ |
| DELETE | /api/cart | ล้างตะกร้า |
| POST | /api/cart/checkout | ชำระเงินและตัดสต็อก |

ถ้าต้องการข้อมูลตั้งต้นใหม่ ให้หยุด API แล้วลบไฟล์ `backend/shopping.db`
