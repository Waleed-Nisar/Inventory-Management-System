# 📦 Inventory Management System (IMS)

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

> A production-ready inventory management web application built with ASP.NET Core MVC, demonstrating Clean Architecture, role-based access control, and professional development practices.


---

## 🌟 Features

### Core Functionality
- ✅ **Product Management** - Complete CRUD operations with SKU tracking
- ✅ **Stock Tracking** - Real-time inventory updates with audit trails
- ✅ **Category Management** - Organize products hierarchically
- ✅ **Transaction History** - Full audit trail of all stock movements
- ✅ **Low Stock Alerts** - Automated warnings for inventory below minimum levels
- ✅ **Dashboard Analytics** - Real-time statistics and recent activity

### Security & Access Control
- 🔐 **Role-Based Access Control (RBAC)** - 4 user roles with granular permissions
- 🔐 **ASP.NET Core Identity** - Built-in authentication and authorization
- 🔐 **Environment-Aware Seeding** - Secure data initialization strategy

### Technical Highlights
- 🏗️ **Clean Architecture** - Separation of concerns across 4 layers
- 🗄️ **Repository Pattern** - Generic and specialized data access
- ⚡ **Async/Await** - Non-blocking operations throughout
- 📱 **Responsive UI** - Bootstrap 5 with mobile-first design
- ✅ **Validation** - Client-side and server-side data validation

---

## 🎭 User Roles & Permissions

| Role | Permissions |
|------|-------------|
| **Admin** | Full system access, user management, delete operations |
| **Manager** | Create/edit products & categories, view reports |
| **Staff** | Update stock, create transactions, view inventory |
| **Viewer** | Read-only access to all data |

---


**Technologies:**
- **Backend**: ASP.NET Core MVC 8.0, Entity Framework Core
- **Database**: SQL Server LocalDB (Development) / SQL Server (Production)
- **Frontend**: Razor Views, Bootstrap 5, jQuery
- **Authentication**: ASP.NET Core Identity
- **Patterns**: Repository, Dependency Injection, CQRS concepts

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (installed with Visual Studio) or SQL Server
- Visual Studio 2022 or VS Code

## 📊 Database Schema

### Core Entities
- **Product** - Inventory items with SKU, pricing, and stock levels
- **Category** - Product classification
- **Supplier** - Vendor information
- **StockTransaction** - Audit trail of all stock movements
- **ApplicationUser** - Extended Identity user with custom properties



---

## 🎯 Business Rules Implemented

1. **Stock Validation**
   - Stock quantity cannot go negative
   - Validation enforced at service layer

2. **Audit Trail**
   - Every stock change creates a transaction record
   - Captures stock before/after values and user attribution

3. **Referential Integrity**
   - Cannot delete categories with associated products
   - Cannot delete products with existing stock (Admin override available)

4. **Low Stock Alerts**
   - Automatic detection when stock < minimum threshold
   - Visual indicators on dashboard and product lists

5. **Role-Based Actions**
   - Admin-only: Delete operations, user management
   - Manager: Create/edit products and categories
   - Staff: Stock transactions only
   - Viewer: Read-only access

## Screenshots

### Login Page
<img width="1902" height="681" alt="Screenshot 2026-01-14 081005" src="https://github.com/user-attachments/assets/b66079c4-62e4-47f1-9fb3-0510ea8ba5b5" />

### Register Page Page
<img width="1885" height="798" alt="Screenshot 2026-01-14 081023" src="https://github.com/user-attachments/assets/68b86436-3a63-4274-8a95-e7843466eaf2" />

### Dashboard
<img width="1878" height="793" alt="Screenshot 2026-01-14 081210" src="https://github.com/user-attachments/assets/d4f781d9-5ccc-4598-acf0-70e2af12cfde" />
<img width="1889" height="921" alt="Screenshot 2026-01-14 081103" src="https://github.com/user-attachments/assets/d1a5be39-5ae7-402e-886a-6299f349b0ca" />

### Products Section
<img width="1879" height="852" alt="Screenshot 2026-01-14 081118" src="https://github.com/user-attachments/assets/83ef2577-c745-4340-a1c5-180f87bee5a7" />

### Categories Section
<img width="1915" height="687" alt="Screenshot 2026-01-14 081134" src="https://github.com/user-attachments/assets/8e837b6b-1e8d-4e43-9304-40598bb1410f" />

### Stock Transaction
<img width="1918" height="872" alt="Screenshot 2026-01-14 081148" src="https://github.com/user-attachments/assets/b4bae500-c504-4885-b9ad-9b2a1664a564" />

---




