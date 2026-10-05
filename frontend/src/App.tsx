import React, { useState, useEffect } from 'react';
import type { StockBalance, Warehouse, Product, CustomerOrder, VendorPurchaseOrder, EdiTransaction } from './types';
import { InventoryView } from './components/InventoryView';
import { OrdersView } from './components/OrdersView';
import { EdiConsoleView } from './components/EdiConsoleView';
import { ReceivingDockView } from './components/ReceivingDockView';
import {
  Package,
  ShoppingCart,
  ArrowRightLeft,
  Truck,
  Database,
  Cpu,
  Layers,
  X
} from 'lucide-react';

export const App: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'inventory' | 'orders' | 'edi' | 'receiving'>('inventory');
  const [stocks, setStocks] = useState<StockBalance[]>([]);
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [orders, setOrders] = useState<CustomerOrder[]>([]);
  const [purchaseOrders, setPurchaseOrders] = useState<VendorPurchaseOrder[]>([]);
  const [ediTransactions, setEdiTransactions] = useState<EdiTransaction[]>([]);
  const [notification, setNotification] = useState<string | null>(
    '🚀 WebstaurantStore Mini-IDS running. Event-driven architecture with MediatR, C# Web API, and ANSI X12 EDI simulation.'
  );

  const fetchAllData = async () => {
    try {
      const [stocksRes, productsRes, whRes, ordersRes, posRes, ediRes] = await Promise.all([
        fetch('/api/inventory'),
        fetch('/api/inventory/products'),
        fetch('/api/inventory/warehouses'),
        fetch('/api/orders'),
        fetch('/api/purchasing/pos'),
        fetch('/api/edi/transactions'),
      ]);

      if (stocksRes.ok) setStocks(await stocksRes.json());
      if (productsRes.ok) setProducts(await productsRes.json());
      if (whRes.ok) setWarehouses(await whRes.json());
      if (ordersRes.ok) setOrders(await ordersRes.json());
      if (posRes.ok) setPurchaseOrders(await posRes.json());
      if (ediRes.ok) setEdiTransactions(await ediRes.json());
    } catch (err) {
      console.error('Failed to fetch data from backend API:', err);
    }
  };

  useEffect(() => {
    fetchAllData();
    const interval = setInterval(fetchAllData, 8000);
    return () => clearInterval(interval);
  }, []);

  const inboundShipmentCount = purchaseOrders.filter(
    po => po.poType === 'Replenishment' && po.status === 'InTransit856'
  ).length;

  return (
    <div>
      {/* Top Header */}
      <header className="header">
        <div className="brand-section">
          <span className="brand-badge">IDS</span>
          <div>
            <span className="brand-title">WebstaurantStore</span>
            <span className="brand-sub">Inventory Distribution System (IDS)</span>
          </div>
        </div>

        <div className="header-status">
          <div className="status-pill">
            <Cpu size={12} color="#38bdf8" />
            <span>MediatR Event Bus: <strong>Active</strong></span>
          </div>
          <div className="status-pill">
            <Database size={12} color="#4ade80" />
            <span>EF Core SQLite: <strong>Synced</strong></span>
          </div>
          <div className="status-pill">
            <Layers size={12} color="#f472b6" />
            <span>ANSI X12 Engine: <strong>Online</strong></span>
          </div>
        </div>
      </header>

      {/* Navigation Bar */}
      <nav className="nav-tabs">
        <button
          className={`nav-tab ${activeTab === 'inventory' ? 'active' : ''}`}
          onClick={() => setActiveTab('inventory')}
        >
          <Package size={16} /> Inventory Matrix
        </button>

        <button
          className={`nav-tab ${activeTab === 'orders' ? 'active' : ''}`}
          onClick={() => setActiveTab('orders')}
        >
          <ShoppingCart size={16} /> Customer Sales Orders
          {orders.length > 0 && (
            <span className="badge badge-info" style={{ marginLeft: '0.25rem' }}>{orders.length}</span>
          )}
        </button>

        <button
          className={`nav-tab ${activeTab === 'edi' ? 'active' : ''}`}
          onClick={() => setActiveTab('edi')}
        >
          <ArrowRightLeft size={16} /> Purchasing &amp; EDI Console
          {purchaseOrders.length > 0 && (
            <span className="badge badge-purple" style={{ marginLeft: '0.25rem' }}>{purchaseOrders.length}</span>
          )}
        </button>

        <button
          className={`nav-tab ${activeTab === 'receiving' ? 'active' : ''}`}
          onClick={() => setActiveTab('receiving')}
        >
          <Truck size={16} /> Warehouse Receiving Dock
          {inboundShipmentCount > 0 && (
            <span className="badge badge-warning" style={{ marginLeft: '0.25rem' }}>
              {inboundShipmentCount} Arriving
            </span>
          )}
        </button>
      </nav>

      {/* Main Content Area */}
      <main className="main-container">
        {notification && (
          <div className="toast-bar">
            <span>{notification}</span>
            <button
              onClick={() => setNotification(null)}
              style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#1e40af' }}
            >
              <X size={16} />
            </button>
          </div>
        )}

        {activeTab === 'inventory' && (
          <InventoryView
            stocks={stocks}
            warehouses={warehouses}
            onRefresh={fetchAllData}
            onEventNotification={msg => setNotification(msg)}
          />
        )}

        {activeTab === 'orders' && (
          <OrdersView
            orders={orders}
            products={products}
            warehouses={warehouses}
            onRefresh={fetchAllData}
            onEventNotification={msg => setNotification(msg)}
          />
        )}

        {activeTab === 'edi' && (
          <EdiConsoleView
            purchaseOrders={purchaseOrders}
            ediTransactions={ediTransactions}
            onRefresh={fetchAllData}
            onEventNotification={msg => setNotification(msg)}
          />
        )}

        {activeTab === 'receiving' && (
          <ReceivingDockView
            purchaseOrders={purchaseOrders}
            warehouses={warehouses}
            onRefresh={fetchAllData}
            onEventNotification={msg => setNotification(msg)}
          />
        )}
      </main>
    </div>
  );
};

export default App;
