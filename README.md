# 📦 Inventory Management System (IMS)

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

> A production-ready inventory management web application built with ASP.NET Core MVC, demonstrating Clean Architecture, role-based access control, and professional development practices.

![Dashboard Preview](<img width="1895" height="930" alt="Screenshot 2026-01-07 144904" src="https://github.com/user-attachments/assets/089f3378-ce03-4d50-b6c6-fc3cddcb6354" />
)

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



## 📸 Screenshots

### Dashboard
![Dashboard](<img width="1895" height="930" alt="Screenshot 2026-01-07 144904" src="https://github.com/user-attachments/assets/a66397ad-ea49-42c8-a620-da3a4642c462" />
)

### Product Management
![Products](<img width="1891" height="815" alt="Screenshot 2026-01-07 145024" src="https://github.com/user-attachments/assets/f1ca52ac-313b-4fd3-8d2f-9e5df301cb6d" />
)

### Stock Transaction
![Stock](<img width="1918" height="855" alt="Screenshot 2026-01-07 145109" src="https://github.com/user-attachments/assets/2a7d859c-d087-46fe-a412-708629704f94" />
)

### User Management (Admin)
![Users](<img width="1908" height="853" alt="Screenshot 2026-01-07 145131" src="https://github.com/user-attachments/assets/2b5df1e1-15ed-44a3-8547-dc4a18ac5ddb" />
)

---



## 📊 Database Schema

### Core Entities
- **Product** - Inventory items with SKU, pricing, and stock levels
- **Category** - Product classification
- **Supplier** - Vendor information
- **StockTransaction** - Audit trail of all stock movements
- **ApplicationUser** - Extended Identity user with custom properties

### Key Relationships
Category 1──────N Product
Supplier 1──────N Product
Product 1───────N StockTransaction
ApplicationUser 1───N StockTransaction (CreatedBy)

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

---




