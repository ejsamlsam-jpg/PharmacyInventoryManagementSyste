PHARMACARE - PHARMACY INVENTORY MANAGEMENT SYSTEM
C# Windows Forms + SQLite

REQUIREMENTS
- Windows 10/11
- Visual Studio 2022
- .NET 8 SDK (Windows Desktop development workload)

HOW TO RUN
1. Extract the ZIP file.
2. Open PharmacyInventoryManagementSystem.csproj in Visual Studio 2022.
3. Allow NuGet package restore to complete.
4. Press F5 or click Start.
5. The app automatically creates pharmacy.db beside the application executable.

FEATURES
- Create: Add a medicine record.
- Read: View and search medicine records.
- Update: Select a row, edit its fields, then click UPDATE.
- Delete: Select a row, click DELETE, and confirm.
- Dashboard counts: total medicines, low stock (10 or fewer), expired medicines.
- Search by medicine name, category, brand, or supplier.

DATABASE
SQLite database file: pharmacy.db
Table: Medicines
Fields: Id, MedicineName, Category, Brand, Quantity, UnitPrice, ExpiryDate, Supplier, DateAdded

NOTES
- This is a starter school-project inventory system.
- It does not include authentication, prescription validation, sales/POS, or multi-user access yet.
- Use sample/non-sensitive data while testing.
