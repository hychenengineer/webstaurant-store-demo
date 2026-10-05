# webstaurant-store-demo: Mini-IDS (Inventory Distribution System)

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![React 19](https://img.shields.io/badge/React-19.0-61DAFB?logo=react&logoColor=black)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![MediatR](https://img.shields.io/badge/MediatR-Event_Bus-purple)](https://github.com/jbogard/MediatR)
[![ANSI X12](https://img.shields.io/badge/EDI-ANSI_X12_(850%2F856)-blue)](https://x12.org/)
[![EF Core](https://img.shields.io/badge/EF_Core-SQLite-green)](https://learn.microsoft.com/en-us/ef/core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

An enterprise-grade, event-driven **Inventory Distribution System (IDS)** reference architecture designed around the architectural, data-contract, and supply-chain patterns used by **WebstaurantStore / Clark Associates**.

---

## 📌 Executive Summary

WebstaurantStore's internal **IDS (Inventory Distribution System)** team builds and maintains the core ERP frontends, EDI transaction engines, and asynchronous C# worker suites that power physical warehouse distribution across the country.

This demo simulates a production-grade slice of that ecosystem:
1. **Multi-Warehouse Routing**: Real-time Available-to-Promise ($\text{ATP} = \text{OnHand} - \text{Reserved}$) tracking across regional distribution centers (`PA-LITITZ`, `NV-DAYTON`, `GA-CUMMING`).
2. **Asynchronous Decoupling**: Uses **MediatR** to implement the in-process Publisher/Consumer pattern, mimicking Webstaurant's RabbitMQ worker architecture.
3. **ANSI X12 B2B Integration**: Generates compliant **EDI 850 (Purchase Orders)** and parses inbound **EDI 856 (Advance Ship Notices)** with carrier tracking extraction.
4. **Physical Warehouse Lifecycle**: Clearly models the transition from *In-Transit* freight to *On-Hand* stock via a dedicated dock receiving portal.
5. **Vendor Drop-Ship Routing**: Automatically intercepts oversized commercial equipment (e.g. Vulcan ranges) and routes the order directly to the manufacturer via custom EDI Ship-To (`N1*ST`) segments.

---

## 🏛 System Architecture

The application is structured as a **Modular Monolith** using the **Controller $\rightarrow$ Service $\rightarrow$ Repository** pattern, keeping domain boundaries strictly isolated and connected exclusively through domain events.

```text
                               ┌─────────────────────────────────┐
                               │   React 19 + TypeScript SPA     │
                               │   Webstaurant IDS Operations    │
                               └────────────────┬────────────────┘
                                                │ REST API (JSON)
┌───────────────────────────────────────────────┴───────────────────────────────────────────────┐
│                                       .NET 8 WEB API                                          │
│                                                                                               │
│   ┌──────────────────────┐        ┌──────────────────────┐        ┌───────────────────────┐   │
│   │ Inventory Controller │        │  Orders Controller   │        │ Purchasing Controller │   │
│   └──────────┬───────────┘        └──────────┬───────────┘        └───────────┬───────────┘   │
│              │                               │                                │               │
│   ┌──────────▼───────────┐        ┌──────────▼───────────┐        ┌───────────▼───────────┐   │
│   │  Inventory Service   │        │    Order Service     │        │  Purchasing Service   │   │
│   └──────────┬───────────┘        └──────────┬───────────┘        └───────────┬───────────┘   │
│              │                               │                                │               │
│   ┌──────────▼───────────┐        ┌──────────▼───────────┐        ┌───────────▼───────────┐   │
│   │ Inventory Repository │        │   Order Repository   │        │ Purchasing Repository │   │
│   └──────────┬───────────┘        └──────────┬───────────┘        └───────────┬───────────┘   │
│              │                               │                                │               │
│              └───────────────────────────────┼────────────────────────────────┘               │
│                                              ▼                                                │
│   ┌───────────────────────────────────────────────────────────────────────────────────────┐   │
│   │                        MediatR Event Bus (Simulating RabbitMQ)                        │   │
│   │                                                                                       │   │
│   │   [LowStockEvent]           ──► Auto-generates Replenishment PO & Outbound EDI 850    │   │
│   │   [DropShipRequestedEvent]  ──► Dispatches Drop-Ship EDI 850 with Customer Address    │   │
│   │   [AsnReceivedEvent]        ──► Routes Freight to In-Transit or Marks Order Shipped   │   │
│   └──────────────────────────────────────────┬────────────────────────────────────────────┘   │
│                                              │                                                │
│                                   ┌──────────▼───────────┐                                    │
│                                   │   EDI X12 Engine     │                                    │
│                                   │  (850 PO & 856 ASN)  │                                    │
│                                   └──────────────────────┘                                    │
└──────────────────────────────────────────────┬────────────────────────────────────────────────┘
                                               ▼
                               ┌────────────────────────────────┐
                               │     Entity Framework Core      │
                               │ Products, Stock, POs, EDI Logs │
                               └────────────────────────────────┘
```

---

## 📂 Project Structure

```text
webstaurant-store-demo/
├── backend/
│   ├── Controllers/               # Web API HTTP endpoints
│   │   ├── InventoryController.cs # Stock queries, depletion, receiving
│   │   ├── OrdersController.cs    # Customer sales order placement
│   │   ├── PurchasingController.cs# PO history, vendor ASN simulation
│   │   └── EdiController.cs       # Raw EDI transaction log stream
│   ├── Domains/
│   │   ├── Inventory/             # Product, Warehouse, StockBalance models & services
│   │   ├── Orders/                # CustomerOrder, OrderItem, order routing logic
│   │   ├── Purchasing/            # VendorPurchaseOrder, replenishment triggers
│   │   └── EDI/                   # ANSI X12 850 generator & 856 parser
│   ├── Infrastructure/
│   │   ├── Data/                  # AppDbContext & DbSeeder (Webstaurant catalog)
│   │   └── Repositories/          # EF Core repository abstractions
│   └── Program.cs                 # DI registration, MediatR setup, CORS
├── frontend/
│   ├── src/
│   │   ├── components/
│   │   │   ├── InventoryView.tsx  # Multi-DC stock matrix & ATP indicators
│   │   │   ├── OrdersView.tsx     # Sales order form (DC stocked vs drop-ship)
│   │   │   ├── EdiConsoleView.tsx # PO table, ASN triggers, raw X12 terminal
│   │   │   └── ReceivingDockView.tsx # Dock operator view (pallet scanning)
│   │   ├── types.ts               # Domain TypeScript contracts
│   │   ├── App.tsx                # Master navigation and live event banners
│   │   └── index.css              # Custom enterprise ERP styling
│   └── vite.config.ts             # Dev server with backend API proxy
├── start-demo.ps1                 # One-click startup script (Backend + Frontend)
└── README.md
```

---

## ⚡ Quick Start

### Option 1: Standalone Single-Click Executable (Zero Prerequisites)
*No .NET SDK, Node.js, or npm required!*

👉 **[Download WebstaurantStore-Mini-IDS-Windows-x64.zip (Direct Download)](https://github.com/hychenengineer/webstaurant-store-demo/releases/download/v1.0.0/WebstaurantStore-Mini-IDS-Windows-x64.zip)** *(or visit the [GitHub Releases](https://github.com/hychenengineer/webstaurant-store-demo/releases) page)*

1. Unzip the downloaded archive.
2. Double-click **`WebstaurantStore-Mini-IDS.exe`**.
3. The embedded server boots, auto-seeds the database, serves the compiled React 19 UI, and immediately launches `http://localhost:5067` in your browser!

---

### Option 2: One-Click Launch (Windows PowerShell)

```powershell
./start-demo.ps1
```
*Automatically starts the .NET 8 backend, boots Vite, and opens `http://localhost:5173` in your default browser.*

---

### Option 3: Manual Developer Startup

**1. Start the Backend API:**
```bash
cd backend
dotnet run
# Listening on http://localhost:5067
```

**2. Start the Frontend Application:**
```bash
cd frontend
npm install
npm run dev
# Accessible at http://localhost:5173
```

---

## 🎬 Interactive Demo Workflows

### 📦 Workflow A: DC Replenishment & Dock Pallet Receiving
*Demonstrates physical inventory replenishment when stock falls below reorder levels.*

1. **Inventory Matrix Tab**: View the stock for `[177FF40N] Avantco 40 lb. Floor Fryer` at `PA-LITITZ` (starting at 12 units).
2. Click **"Deplete (-5)"** until stock drops to 2 units (below the reorder threshold of 10).
3. **The Event Chain**:
   - `InventoryService` detects stock $\le$ reorder point and publishes `LowStockEvent`.
   - `LowStockEventHandler` calls `PurchasingService` to create a replenishment PO for 25 units.
   - `EdiService` generates an **ANSI X12 850 Purchase Order**:
     ```text
     ISA*00*          *00*          *ZZ*WEBSTAURANT   *01*Avantco Equip  *261001*1200*U*00401*000000001*0*P*>~
     ST*850*00001~
     BEG*00*SA*PO-20261001-4821**261001~
     PO1*001*25*EA*949.00*PE*VN*177FF40N*IN*177FF40N~
     CTT*1~
     ```
4. **Purchasing & EDI Tab**: Inspect the generated PO and click **"Simulate Vendor 856 ASN"**.
5. **Supply Chain Reality**: Stock transitions to **"In-Transit"** (on a freight carrier truck). It is **not** sellable yet.
6. **Receiving Dock Tab**: Switch to the dock operator screen. Click **"Receive Pallet & Stock In"** to simulate the barcode scan.
7. Return to the **Inventory Matrix Tab**—`QtyOnHand` increases by 25 units. The cycle is complete!

---

### 🚚 Workflow B: Vendor Drop-Ship Equipment Order
*Demonstrates direct-to-customer vendor fulfillment for heavy commercial equipment.*

1. **Customer Sales Orders Tab**: Select `[922V36G] Vulcan 36" Commercial 6-Burner Gas Range` ($3,850.00). Note the `★ [DROP-SHIP ONLY]` badge.
2. Enter restaurant delivery details (*e.g., Bistro Bella, Philadelphia, PA*) and click **"Place Order & Trigger Workflow"**.
3. **The Event Chain**:
   - `OrderService` recognizes this item does not ship from internal warehouses and fires `DropShipRequestedEvent`.
   - `DropShipRequestedEventHandler` creates a Drop-Ship Vendor PO.
   - `EdiService` generates an **EDI 850** with the *customer's* delivery address embedded directly into the `N1*ST` (Ship-To) segment:
     ```text
     N1*ST*Bistro Bella Philadelphia~
     N3*1234 Market St~
     N4*Philadelphia*PA*19107*US~
     ```
4. **Purchasing & EDI Tab**: Click **"Simulate Vendor 856 ASN"**.
5. **Order Completion**: MediatR fires `AsnReceivedEvent`, and `AsnReceivedForOrderHandler` marks the customer order as **Shipped** with carrier tracking number!

---

## 📐 Architectural Highlights & Domain Patterns

Key architectural decisions and supply-chain domain patterns implemented in this system:

| Topic | Technical Implementation & Rationale |
|---|---|
| **Domain-Driven Design (DDD)** | Modeled Customer Sales Orders (`OrderService`) and Vendor Purchase Orders (`PurchasingService`) as completely separate aggregates. Avoids the classic pitfall of overloading a single generic order table. |
| **Available-to-Promise (ATP)** | Implemented strict formula: $\text{ATP} = \text{QtyOnHand} - \text{QtyReserved}$. Ensures regional DCs never oversell inventory during peak order rushes. |
| **Event-Driven Architecture** | Used **MediatR** in-memory handlers to cleanly decouple domain operations (simulating asynchronous RabbitMQ queues) without requiring external infrastructure. |
| **Physical vs. Logical Inventory** | Demonstrated that an **EDI 856 ASN** only signifies goods *in-transit*. Inventory cannot transition to *on-hand* until the physical dock barcode scan is completed. |
| **EDI B2B Standards** | Handled raw ANSI X12 envelope structures (`ISA`/`IEA`, `GS`/`GE`, `ST`/`SE`) and modeled the trade-off between direct **AS2** endpoints and **VANs (e.g. SPS Commerce)**. |

---

## 📜 License

Licensed under the [MIT License](LICENSE).
