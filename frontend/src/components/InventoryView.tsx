import React, { useState } from 'react';
import type { StockBalance, Warehouse } from '../types';
import { Package, AlertTriangle, ArrowDownRight, Warehouse as WarehouseIcon, RefreshCw } from 'lucide-react';

interface Props {
  stocks: StockBalance[];
  warehouses: Warehouse[];
  onRefresh: () => void;
  onEventNotification: (msg: string) => void;
}

export const InventoryView: React.FC<Props> = ({ stocks, warehouses, onRefresh, onEventNotification }) => {
  const [selectedWarehouse, setSelectedWarehouse] = useState<number | 'ALL'>('ALL');
  const [loadingId, setLoadingId] = useState<number | null>(null);

  const filteredStocks = selectedWarehouse === 'ALL'
    ? stocks
    : stocks.filter(s => s.warehouseId === selectedWarehouse);

  const handleDeplete = async (stock: StockBalance, amount: number) => {
    setLoadingId(stock.id);
    try {
      const res = await fetch('/api/inventory/deplete', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          warehouseId: stock.warehouseId,
          productId: stock.productId,
          quantity: amount
        })
      });

      if (!res.ok) throw new Error('Failed to deplete stock');

      const newQty = Math.max(0, stock.qtyOnHand - amount);
      if (newQty <= stock.product.reorderPoint) {
        onEventNotification(
          `⚡ MediatR LowStockEvent fired for SKU [${stock.product.sku}] at ${stock.warehouse.code}! Auto-generated Vendor EDI 850 Purchase Order.`
        );
      } else {
        onEventNotification(`Stock depleted by ${amount} units for SKU [${stock.product.sku}].`);
      }

      onRefresh();
    } catch (err: any) {
      alert(err.message);
    } finally {
      setLoadingId(null);
    }
  };

  return (
    <div>
      <div className="card">
        <div className="card-header">
          <div>
            <h2 className="card-title">
              <Package size={20} color="var(--primary)" /> Multi-Warehouse Inventory Matrix
            </h2>
            <p className="card-desc">
              Real-time available-to-promise (ATP) inventory levels across WebstaurantStore distribution centers.
            </p>
          </div>
          <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'center' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.4rem', fontSize: '0.85rem' }}>
              <WarehouseIcon size={16} /> Filter DC:
              <select
                className="form-control"
                style={{ padding: '0.3rem 0.5rem', width: 'auto' }}
                value={selectedWarehouse}
                onChange={e => setSelectedWarehouse(e.target.value === 'ALL' ? 'ALL' : Number(e.target.value))}
              >
                <option value="ALL">All Distribution Centers</option>
                {warehouses.map(w => (
                  <option key={w.id} value={w.id}>{w.code} ({w.city}, {w.state})</option>
                ))}
              </select>
            </div>
            <button className="btn btn-outline" onClick={onRefresh}>
              <RefreshCw size={14} /> Refresh
            </button>
          </div>
        </div>

        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>SKU</th>
                <th>Product Description</th>
                <th>DC Location</th>
                <th>On Hand</th>
                <th>Reserved</th>
                <th>In Transit</th>
                <th>ATP</th>
                <th>Reorder Point</th>
                <th>Stock Health</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {filteredStocks.map(s => {
                const isLow = s.qtyOnHand <= s.product.reorderPoint;
                const hasInTransit = s.qtyInTransit > 0;

                return (
                  <tr key={s.id}>
                    <td>
                      <strong style={{ color: 'var(--primary)' }}>{s.product.sku}</strong>
                    </td>
                    <td>
                      <div style={{ fontWeight: 600 }}>{s.product.name}</div>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                        Category: {s.product.category} | Vendor: {s.product.vendorName}
                      </div>
                    </td>
                    <td>
                      <span className="badge badge-info">{s.warehouse.code}</span>
                    </td>
                    <td style={{ fontWeight: 700, fontSize: '0.95rem' }}>
                      {s.qtyOnHand}
                    </td>
                    <td style={{ color: 'var(--text-muted)' }}>{s.qtyReserved}</td>
                    <td>
                      {s.qtyInTransit > 0 ? (
                        <span className="badge badge-purple">+{s.qtyInTransit} arriving</span>
                      ) : (
                        <span style={{ color: '#cbd5e1' }}>0</span>
                      )}
                    </td>
                    <td style={{ fontWeight: 600, color: s.availableToPromise > 0 ? 'var(--success)' : 'var(--warning)' }}>
                      {s.availableToPromise}
                    </td>
                    <td style={{ color: 'var(--text-muted)' }}>{s.product.reorderPoint}</td>
                    <td>
                      {isLow ? (
                        <span className="badge badge-danger">
                          <AlertTriangle size={12} /> Low Stock (&le; {s.product.reorderPoint})
                        </span>
                      ) : hasInTransit ? (
                        <span className="badge badge-warning">Replenishing</span>
                      ) : (
                        <span className="badge badge-success">Healthy</span>
                      )}
                    </td>
                    <td>
                      <button
                        className="btn btn-secondary"
                        disabled={loadingId === s.id}
                        onClick={() => handleDeplete(s, 5)}
                        title="Simulate sales order pulling stock"
                      >
                        <ArrowDownRight size={13} /> Deplete (-5)
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};
