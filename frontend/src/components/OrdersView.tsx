import React, { useState } from 'react';
import type { CustomerOrder, Product, Warehouse } from '../types';
import { ShoppingCart, Send, Truck } from 'lucide-react';

interface Props {
  orders: CustomerOrder[];
  products: Product[];
  warehouses: Warehouse[];
  onRefresh: () => void;
  onEventNotification: (msg: string) => void;
}

export const OrdersView: React.FC<Props> = ({ orders, products, warehouses, onRefresh, onEventNotification }) => {
  const [customerName, setCustomerName] = useState('Bistro Bella Philadelphia');
  const [address, setAddress] = useState('1234 Market St');
  const [city, setCity] = useState('Philadelphia');
  const [state, setState] = useState('PA');
  const [zip, setZip] = useState('19107');
  const [productId, setProductId] = useState<number>(products[0]?.id || 1);
  const [quantity, setQuantity] = useState<number>(1);
  const [warehouseId, setWarehouseId] = useState<number>(warehouses[0]?.id || 1);
  const [submitting, setSubmitting] = useState(false);

  const selectedProduct = products.find(p => p.id === productId);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      const res = await fetch('/api/orders', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          customerName,
          address,
          city,
          state,
          zip,
          productId,
          quantity,
          warehouseId: selectedProduct?.isDropShipOnly ? null : warehouseId
        })
      });

      if (!res.ok) throw new Error('Failed to place order');
      const order: CustomerOrder = await res.json();

      if (selectedProduct?.isDropShipOnly) {
        onEventNotification(
          `🚚 Drop-Ship Order [${order.orderNumber}] placed! MediatR published DropShipRequestedEvent -> Auto-generated EDI 850 PO to ${selectedProduct.vendorName} with customer address.`
        );
      } else {
        onEventNotification(
          `📦 Standard Order [${order.orderNumber}] fulfilled from DC inventory! Stock deducted from warehouse.`
        );
      }

      onRefresh();
    } catch (err: any) {
      alert(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div>
      <div className="grid-2">
        {/* Order Placement Form */}
        <div className="card">
          <div className="card-header">
            <div>
              <h2 className="card-title">
                <ShoppingCart size={20} color="var(--primary)" /> Place Customer Sales Order
              </h2>
              <p className="card-desc">
                Simulate a restaurant ordering products online from WebstaurantStore.
              </p>
            </div>
          </div>

          <form onSubmit={handleSubmit}>
            <div className="form-group" style={{ marginBottom: '1rem' }}>
              <label className="form-label">Select Product:</label>
              <select
                className="form-control"
                value={productId}
                onChange={e => setProductId(Number(e.target.value))}
              >
                {products.map(p => (
                  <option key={p.id} value={p.id}>
                    [{p.sku}] {p.name} {p.isDropShipOnly ? '★ [DROP-SHIP ONLY]' : '• [WAREHOUSE STOCKED]'} (${p.unitPrice})
                  </option>
                ))}
              </select>

              {selectedProduct?.isDropShipOnly ? (
                <div style={{ marginTop: '0.4rem', fontSize: '0.8rem', color: '#b45309', background: '#fef3c7', padding: '0.4rem 0.6rem', borderRadius: '4px' }}>
                  <strong>⚡ Drop-Ship Item:</strong> This equipment does not ship from our DC. Placing this order will emit a MediatR <code>DropShipRequestedEvent</code> to generate an EDI 850 directly to {selectedProduct.vendorName}!
                </div>
              ) : (
                <div style={{ marginTop: '0.4rem', fontSize: '0.8rem', color: '#15803d', background: '#dcfce7', padding: '0.4rem 0.6rem', borderRadius: '4px' }}>
                  <strong>✓ Warehouse Stocked:</strong> This order will be fulfilled directly from physical warehouse stock.
                </div>
              )}
            </div>

            <div className="form-grid" style={{ marginBottom: '1rem' }}>
              <div className="form-group">
                <label className="form-label">Quantity:</label>
                <input
                  type="number"
                  min="1"
                  className="form-control"
                  value={quantity}
                  onChange={e => setQuantity(Number(e.target.value))}
                  required
                />
              </div>

              {!selectedProduct?.isDropShipOnly && (
                <div className="form-group">
                  <label className="form-label">Fulfill From DC:</label>
                  <select
                    className="form-control"
                    value={warehouseId}
                    onChange={e => setWarehouseId(Number(e.target.value))}
                  >
                    {warehouses.map(w => (
                      <option key={w.id} value={w.id}>{w.code} ({w.city}, {w.state})</option>
                    ))}
                  </select>
                </div>
              )}
            </div>

            <div className="form-group" style={{ marginBottom: '0.75rem' }}>
              <label className="form-label">Customer / Restaurant Name:</label>
              <input
                type="text"
                className="form-control"
                value={customerName}
                onChange={e => setCustomerName(e.target.value)}
                required
              />
            </div>

            <div className="form-grid" style={{ marginBottom: '1.25rem' }}>
              <div className="form-group" style={{ gridColumn: 'span 2' }}>
                <label className="form-label">Street Address:</label>
                <input
                  type="text"
                  className="form-control"
                  value={address}
                  onChange={e => setAddress(e.target.value)}
                  required
                />
              </div>
              <div className="form-group">
                <label className="form-label">City:</label>
                <input
                  type="text"
                  className="form-control"
                  value={city}
                  onChange={e => setCity(e.target.value)}
                  required
                />
              </div>
              <div className="form-group">
                <label className="form-label">State:</label>
                <input
                  type="text"
                  className="form-control"
                  value={state}
                  onChange={e => setState(e.target.value)}
                  required
                />
              </div>
              <div className="form-group">
                <label className="form-label">Zip:</label>
                <input
                  type="text"
                  className="form-control"
                  value={zip}
                  onChange={e => setZip(e.target.value)}
                  required
                />
              </div>
            </div>

            <button type="submit" className="btn btn-primary" style={{ width: '100%' }} disabled={submitting}>
              <Send size={15} /> Place Order & Trigger Workflow
            </button>
          </form>
        </div>

        {/* Workflow Info Box */}
        <div className="card" style={{ background: '#f8fafc', borderLeft: '4px solid var(--primary)' }}>
          <div className="card-header">
            <h3 className="card-title">IDS Order Routing Logic</h3>
          </div>
          <div style={{ fontSize: '0.875rem', lineHeight: 1.6, color: '#334155' }}>
            <p style={{ marginBottom: '0.75rem' }}>
              When a customer places an order on WebstaurantStore, the IDS backend evaluates whether the SKU is a <strong>Physical Inventory Stock</strong> or a <strong>Vendor Drop-Ship</strong> item.
            </p>
            <ul style={{ paddingLeft: '1.25rem', marginBottom: '1rem' }}>
              <li style={{ marginBottom: '0.5rem' }}>
                <strong>Standard Item:</strong> Allocated from the closest regional distribution center (Lititz, Dayton, or Cumming). If stock drops below threshold, MediatR automatically fires <code>LowStockEvent</code>.
              </li>
              <li>
                <strong>Drop-Ship Item:</strong> Bypasses internal warehouses. Emits <code>DropShipRequestedEvent</code> to create a Vendor PO and sends an ANSI X12 850 Purchase Order with the restaurant's delivery address in the <code>N1*ST</code> segment!
              </li>
            </ul>
            <div style={{ background: '#fff', border: '1px solid var(--border)', borderRadius: '6px', padding: '0.75rem' }}>
              <div style={{ fontWeight: 600, color: 'var(--primary)', marginBottom: '0.25rem' }}>💡 Architecture Note:</div>
              "In this architecture, customer sales orders and vendor purchase orders are distinct domains communicating exclusively via MediatR events, matching WebstaurantStore's distributed design."
            </div>
          </div>
        </div>
      </div>

      {/* Customer Orders Table */}
      <div className="card">
        <div className="card-header">
          <h2 className="card-title">Customer Order History</h2>
          <span className="badge badge-info">{orders.length} Total Orders</span>
        </div>

        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Order #</th>
                <th>Customer</th>
                <th>Item Ordered</th>
                <th>Type</th>
                <th>Destination</th>
                <th>Status</th>
                <th>Carrier Tracking</th>
              </tr>
            </thead>
            <tbody>
              {orders.length === 0 ? (
                <tr>
                  <td colSpan={7} style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>
                    No customer orders yet. Use the form above to place your first test order!
                  </td>
                </tr>
              ) : (
                orders.map(o => (
                  <tr key={o.id}>
                    <td><strong>{o.orderNumber}</strong></td>
                    <td>{o.customerName}</td>
                    <td>
                      {o.items.map(i => (
                        <div key={i.id}>
                          <strong>{i.sku}</strong> &times; {i.quantity} ({i.productName})
                        </div>
                      ))}
                    </td>
                    <td>
                      {o.orderType === 'DropShip' ? (
                        <span className="badge badge-purple">Drop-Ship</span>
                      ) : (
                        <span className="badge badge-info">Standard DC</span>
                      )}
                    </td>
                    <td style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                      {o.shipToAddress}, {o.shipToCity}, {o.shipToState} {o.shipToZip}
                    </td>
                    <td>
                      {o.status === 'SentToVendor' ? (
                        <span className="badge badge-warning">Sent to Vendor (EDI 850)</span>
                      ) : o.status === 'Shipped' ? (
                        <span className="badge badge-success"><Truck size={12} /> Shipped</span>
                      ) : (
                        <span className="badge badge-info">{o.status}</span>
                      )}
                    </td>
                    <td>
                      {o.trackingNumber ? (
                        <span style={{ fontFamily: 'monospace', fontSize: '0.8rem', color: 'var(--info)' }}>
                          {o.trackingNumber}
                        </span>
                      ) : (
                        <span style={{ color: '#cbd5e1' }}>Awaiting Vendor ASN</span>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};
