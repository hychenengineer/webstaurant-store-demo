import React, { useState } from 'react';
import type { VendorPurchaseOrder, EdiTransaction } from '../types';
import { FileText, Send, CheckCircle2, Eye, Terminal, ArrowRightLeft, Truck } from 'lucide-react';

interface Props {
  purchaseOrders: VendorPurchaseOrder[];
  ediTransactions: EdiTransaction[];
  onRefresh: () => void;
  onEventNotification: (msg: string) => void;
}

export const EdiConsoleView: React.FC<Props> = ({
  purchaseOrders,
  ediTransactions,
  onRefresh,
  onEventNotification
}) => {
  const [selectedTransaction, setSelectedTransaction] = useState<EdiTransaction | null>(null);
  const [simulatingPo, setSimulatingPo] = useState<string | null>(null);

  const handleSimulateAsn = async (poNumber: string, poType: string) => {
    setSimulatingPo(poNumber);
    try {
      const res = await fetch('/api/purchasing/simulate-asn', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ poNumber })
      });

      if (!res.ok) throw new Error('Failed to simulate vendor ASN');
      const updatedPo = await res.json();

      if (poType === 'DropShip') {
        onEventNotification(
          `🚚 Inbound EDI 856 received for Drop-Ship PO [${poNumber}]! Tracking #${updatedPo.trackingNumber} generated. Customer Order marked Shipped!`
        );
      } else {
        onEventNotification(
          `📥 Inbound EDI 856 received for Replenishment PO [${poNumber}]! Items are now "In-Transit" to DC. Head to Receiving Dock to receive the shipment!`
        );
      }

      onRefresh();
    } catch (err: any) {
      alert(err.message);
    } finally {
      setSimulatingPo(null);
    }
  };

  return (
    <div>
      {/* Vendor Purchase Orders Card */}
      <div className="card">
        <div className="card-header">
          <div>
            <h2 className="card-title">
              <FileText size={20} color="var(--primary)" /> Vendor Purchase Orders (Procurement)
            </h2>
            <p className="card-desc">
              Purchase Orders automatically dispatched to manufacturers (Avantco, Cambro, Vulcan) via EDI 850.
            </p>
          </div>
          <span className="badge badge-info">{purchaseOrders.length} Total POs</span>
        </div>

        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>PO Number</th>
                <th>Vendor Partner</th>
                <th>Type</th>
                <th>Items Ordered</th>
                <th>Status</th>
                <th>Tracking #</th>
                <th>Simulate Vendor Action</th>
              </tr>
            </thead>
            <tbody>
              {purchaseOrders.length === 0 ? (
                <tr>
                  <td colSpan={7} style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>
                    No Purchase Orders yet. Deplete stock in the Inventory tab or place a Drop-Ship order to trigger an automatic PO!
                  </td>
                </tr>
              ) : (
                purchaseOrders.map(po => (
                  <tr key={po.id}>
                    <td><strong>{po.poNumber}</strong></td>
                    <td>{po.vendorName}</td>
                    <td>
                      {po.poType === 'DropShip' ? (
                        <span className="badge badge-purple">Drop-Ship</span>
                      ) : (
                        <span className="badge badge-info">DC Restock</span>
                      )}
                    </td>
                    <td>
                      {po.items.map(i => (
                        <div key={i.id} style={{ fontSize: '0.85rem' }}>
                          <strong>{i.sku}</strong> &times; {i.quantity} ({i.productName})
                        </div>
                      ))}
                    </td>
                    <td>
                      {po.status === 'Sent850' ? (
                        <span className="badge badge-warning">
                          <Send size={11} /> Sent EDI 850
                        </span>
                      ) : po.status === 'InTransit856' ? (
                        <span className="badge badge-purple">
                          <Truck size={11} /> In Transit (856 ASN)
                        </span>
                      ) : po.status === 'Received' ? (
                        <span className="badge badge-success">
                          <CheckCircle2 size={11} /> Received at Dock
                        </span>
                      ) : (
                        <span className="badge badge-info">{po.status}</span>
                      )}
                    </td>
                    <td>
                      {po.trackingNumber ? (
                        <span style={{ fontFamily: 'monospace', fontSize: '0.8rem', color: 'var(--info)' }}>
                          {po.trackingNumber}
                        </span>
                      ) : (
                        <span style={{ color: '#cbd5e1' }}>—</span>
                      )}
                    </td>
                    <td>
                      {po.status === 'Sent850' ? (
                        <button
                          className="btn btn-primary"
                          style={{ fontSize: '0.78rem', padding: '0.35rem 0.65rem' }}
                          disabled={simulatingPo === po.poNumber}
                          onClick={() => handleSimulateAsn(po.poNumber, po.poType)}
                        >
                          <Send size={12} /> Simulate Vendor 856 ASN
                        </button>
                      ) : po.status === 'InTransit856' ? (
                        <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                          {po.poType === 'Replenishment' ? 'Awaiting Dock Receive' : 'Fulfilled to Customer'}
                        </span>
                      ) : (
                        <span style={{ fontSize: '0.8rem', color: 'var(--success)' }}>Cycle Complete ✓</span>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* EDI Transaction Logs & Inspector */}
      <div className="grid-2">
        <div className="card">
          <div className="card-header">
            <h3 className="card-title">
              <ArrowRightLeft size={18} color="var(--primary)" /> EDI X12 Message Stream
            </h3>
            <span className="badge badge-purple">{ediTransactions.length} Messages</span>
          </div>

          <div className="table-container" style={{ maxHeight: '380px', overflowY: 'auto' }}>
            <table>
              <thead>
                <tr>
                  <th>Set</th>
                  <th>Direction</th>
                  <th>Partner</th>
                  <th>PO #</th>
                  <th>Inspect</th>
                </tr>
              </thead>
              <tbody>
                {ediTransactions.length === 0 ? (
                  <tr>
                    <td colSpan={5} style={{ textAlign: 'center', padding: '1.5rem', color: 'var(--text-muted)' }}>
                      No EDI messages logged yet.
                    </td>
                  </tr>
                ) : (
                  ediTransactions.map(t => (
                    <tr
                      key={t.id}
                      style={{
                        cursor: 'pointer',
                        backgroundColor: selectedTransaction?.id === t.id ? '#f1f5f9' : undefined
                      }}
                      onClick={() => setSelectedTransaction(t)}
                    >
                      <td>
                        <span className="badge" style={{ backgroundColor: '#0f172a', color: '#38bdf8' }}>
                          EDI {t.transactionSet}
                        </span>
                      </td>
                      <td>
                        {t.direction === 'Outbound' ? (
                          <span className="badge badge-info">Outbound &rarr;</span>
                        ) : (
                          <span className="badge badge-success">&larr; Inbound</span>
                        )}
                      </td>
                      <td style={{ fontSize: '0.825rem' }}>{t.partnerName}</td>
                      <td style={{ fontSize: '0.825rem', fontFamily: 'monospace' }}>{t.poNumber}</td>
                      <td>
                        <button
                          className="btn btn-outline"
                          style={{ padding: '0.2rem 0.5rem', fontSize: '0.75rem' }}
                          onClick={(e) => {
                            e.stopPropagation();
                            setSelectedTransaction(t);
                          }}
                        >
                          <Eye size={12} /> View Raw
                        </button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>

        {/* Raw EDI Payload Inspector */}
        <div className="card" style={{ display: 'flex', flexDirection: 'column' }}>
          <div className="card-header">
            <h3 className="card-title">
              <Terminal size={18} color="#38bdf8" /> ANSI X12 Inspector
            </h3>
            {selectedTransaction && (
              <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                Ctrl: {selectedTransaction.controlNumber} | Set: {selectedTransaction.transactionSet}
              </span>
            )}
          </div>

          {selectedTransaction ? (
            <div style={{ flex: 1 }}>
              <div style={{ marginBottom: '0.5rem', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                Viewing raw <strong>EDI {selectedTransaction.transactionSet}</strong> ({selectedTransaction.direction}) for PO <strong>{selectedTransaction.poNumber}</strong>:
              </div>
              <div className="edi-terminal" style={{ maxHeight: '300px' }}>
                {selectedTransaction.rawPayload}
              </div>
              <div style={{ marginTop: '0.75rem', fontSize: '0.775rem', color: 'var(--text-muted)' }}>
                <strong>Key Segments:</strong>
                {selectedTransaction.transactionSet === '850' ? (
                  <span> <code>BEG</code>: Order header | <code>N1*ST</code>: Ship-to location | <code>PO1</code>: Item & quantity | <code>CTT</code>: Hash total</span>
                ) : (
                  <span> <code>BSN</code>: Shipment notice | <code>PRF</code>: PO Reference | <code>REF*2I</code>: Tracking number | <code>HL</code>: Hierarchical level</span>
                )}
              </div>
            </div>
          ) : (
            <div style={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--text-muted)', fontSize: '0.85rem', minHeight: '220px' }}>
              Select any EDI transaction on the left to inspect its raw ANSI X12 segment structure.
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
