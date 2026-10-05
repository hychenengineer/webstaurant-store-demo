import React, { useState } from 'react';
import type { VendorPurchaseOrder, Warehouse } from '../types';
import { Box, CheckCircle, Truck, Warehouse as WhIcon } from 'lucide-react';

interface Props {
  purchaseOrders: VendorPurchaseOrder[];
  warehouses: Warehouse[];
  onRefresh: () => void;
  onEventNotification: (msg: string) => void;
}

export const ReceivingDockView: React.FC<Props> = ({
  purchaseOrders,
  warehouses,
  onRefresh,
  onEventNotification
}) => {
  const [receivingId, setReceivingId] = useState<number | null>(null);

  // Filter for replenishment POs currently In-Transit
  const inboundShipments = purchaseOrders.filter(
    po => po.poType === 'Replenishment' && po.status === 'InTransit856'
  );

  const completedReceipts = purchaseOrders.filter(
    po => po.poType === 'Replenishment' && po.status === 'Received'
  );

  const handleReceive = async (po: VendorPurchaseOrder) => {
    setReceivingId(po.id);
    try {
      const res = await fetch('/api/inventory/receive', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ purchaseOrderId: po.id })
      });

      if (!res.ok) throw new Error('Failed to receive shipment');

      const wh = warehouses.find(w => w.id === po.warehouseId);
      onEventNotification(
        `📦 Pallet scan complete! Purchase Order [${po.poNumber}] successfully received at ${wh?.code || 'DC'}. Stock transferred from In-Transit to On-Hand inventory!`
      );

      onRefresh();
    } catch (err: any) {
      alert(err.message);
    } finally {
      setReceivingId(null);
    }
  };

  return (
    <div>
      <div className="card">
        <div className="card-header">
          <div>
            <h2 className="card-title">
              <Truck size={20} color="var(--primary)" /> Distribution Center Receiving Dock (Operator View)
            </h2>
            <p className="card-desc">
              Physical inventory check-in. Inbound freight notified via EDI 856 ASN arriving at the warehouse dock.
            </p>
          </div>
          <span className="badge badge-purple">{inboundShipments.length} Shipments Awaiting Dock Scan</span>
        </div>

        <div style={{ marginBottom: '1.25rem', background: '#eff6ff', border: '1px solid #bfdbfe', borderRadius: '6px', padding: '0.75rem 1rem', fontSize: '0.85rem', color: '#1e40af' }}>
          <strong>ℹ Supply Chain Domain Note:</strong> When an EDI 856 arrives from the vendor, goods are merely <em>"In-Transit"</em> on a freight trailer. The stock is <strong>not</strong> sellable on WebstaurantStore until a dock worker scans the pallet barcode, moving stock into <code>QtyOnHand</code>.
        </div>

        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>PO Number</th>
                <th>Manufacturer</th>
                <th>Destination DC</th>
                <th>Incoming Merchandise</th>
                <th>Tracking #</th>
                <th>Status</th>
                <th>Dock Action</th>
              </tr>
            </thead>
            <tbody>
              {inboundShipments.length === 0 ? (
                <tr>
                  <td colSpan={7} style={{ textAlign: 'center', padding: '2.5rem', color: 'var(--text-muted)' }}>
                    No inbound shipments pending receipt. Deplete warehouse stock and click "Simulate Vendor 856 ASN" in the Purchasing tab to dispatch freight here!
                  </td>
                </tr>
              ) : (
                inboundShipments.map(po => {
                  const wh = warehouses.find(w => w.id === po.warehouseId);
                  return (
                    <tr key={po.id}>
                      <td><strong>{po.poNumber}</strong></td>
                      <td>{po.vendorName}</td>
                      <td>
                        <span className="badge badge-info">
                          <WhIcon size={12} /> {wh?.code || 'DC'} ({wh?.city}, {wh?.state})
                        </span>
                      </td>
                      <td>
                        {po.items.map(i => (
                          <div key={i.id}>
                            <strong>{i.sku}</strong> &times; {i.quantity} units ({i.productName})
                          </div>
                        ))}
                      </td>
                      <td>
                        <span style={{ fontFamily: 'monospace', fontSize: '0.8rem', color: 'var(--info)' }}>
                          {po.trackingNumber}
                        </span>
                      </td>
                      <td>
                        <span className="badge badge-purple">
                          <Truck size={12} /> In Transit on Trailer
                        </span>
                      </td>
                      <td>
                        <button
                          className="btn btn-success"
                          disabled={receivingId === po.id}
                          onClick={() => handleReceive(po)}
                        >
                          <Box size={13} /> Receive Pallet &amp; Stock In
                        </button>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Completed Receipts Archive */}
      {completedReceipts.length > 0 && (
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">
              <CheckCircle size={18} color="var(--success)" /> Recently Received Deliveries (Stock Added)
            </h3>
            <span className="badge badge-success">{completedReceipts.length} Received</span>
          </div>

          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>PO Number</th>
                  <th>Vendor</th>
                  <th>Destination</th>
                  <th>Received Items</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {completedReceipts.map(po => {
                  const wh = warehouses.find(w => w.id === po.warehouseId);
                  return (
                    <tr key={po.id}>
                      <td>{po.poNumber}</td>
                      <td>{po.vendorName}</td>
                      <td>{wh?.code}</td>
                      <td>
                        {po.items.map(i => (
                          <span key={i.id} style={{ marginRight: '0.5rem' }}>
                            {i.sku} (+{i.quantity})
                          </span>
                        ))}
                      </td>
                      <td>
                        <span className="badge badge-success">
                          <CheckCircle size={11} /> Stocked In (QtyOnHand +)
                        </span>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};
