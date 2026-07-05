# نظام إدارة السوبر ماركت المتكامل (Supermarket ERP System)

## خطوات تشغيل البرنامج وبنائه باستخدام Visual Studio

لضمان تشغيل البرنامج بنجاح، يرجى اتباع الخطوات التالية بالترتيب:

### 1. تجهيز قاعدة البيانات (SQL Server)
1. افتح برنامج **SQL Server Management Studio (SSMS)**.
2. قم بفتح ملف السكربت المرفق `database_schema.sql`.
3. قم بتنفيذ السكربت (Execute) لإنشاء قاعدة البيانات `SupermarketDB` والجداول والبيانات الأساسية ودليل الحسابات.

### 2. فتح المشروع في Visual Studio
1. تأكد من تثبيت **Visual Studio 2022** أو إصدار أحدث، مع تثبيت حزمة تطوير **.NET desktop development**.
2. افتح المجلد `src` وقم بالنقر المزدوج على ملف الحل `SupermarketSystem.slnx` (أو قم بفتحه من داخل الفيجوال ستوديو).

### 3. تعديل نص الاتصال بقاعدة البيانات
1. داخل مشروع `Supermarket.UI` في متصفح الحل (Solution Explorer)، افتح ملف `AppSettings.cs`.
2. قم بتعديل قيمة `ConnectionString` لتناسب إعدادات جهازك (اسم السيرفر):
   ```csharp
   public static string ConnectionString { get; set; } = "Server=YOUR_SERVER_NAME;Database=SupermarketDB;Trusted_Connection=True;TrustServerCertificate=True;";
   ```

### 4. بناء وتشغيل المشروع (Build & Run)
1. من القائمة العلوية، اختر **Build** ثم **Build Solution** (أو اضغط `Ctrl+Shift+B`).
2. تأكد من عدم وجود أخطاء في نافذة المخرجات (Output).
3. اضغط على زر **Start** (أو مفتاح `F5`) لتشغيل البرنامج.

### 5. بيانات الدخول الافتراضية
- **اسم المستخدم:** `admin`
- **كلمة المرور:** `admin123`

---
**ملاحظة تقنية:** النظام يعتمد على معمارية الطبقات الأربع لضمان الأداء العالي وسهولة الصيانة، ويستخدم Dapper للتعامل السريع مع قاعدة البيانات.
