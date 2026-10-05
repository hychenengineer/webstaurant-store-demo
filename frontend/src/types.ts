export interface Warehouse {
  id: number;
  code: string;
  name: string;
  city: string;
  state: string;
}

export interface Product {
  id: number;
  sku: string;
  name: string;
  category: string;
  unitPrice: number;
  isDropShipOnly: boolean;
  reorderPoint: number;
  targetReplenishQty: number;
  vendorName: string;
}

export interface StockBalance {
  id: number;
  productId: number;
  product: Product;
  warehouseId: number;
  warehouse: Warehouse;
  qtyOnHand: number;
  qtyReserved: number;
  qtyInTransit: number;
  availableToPromise: number;
}

export interface PurchaseOrderItem {
  id: number;
  productId: number;
  sku: string;
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface VendorPurchaseOrder {
  id: number;
  poNumber: string;
  vendorName: string;
  poType: 'Replenishment' | 'DropShip';
  warehouseId?: number;
  customerOrderId?: number;
  status: 'Pending' | 'Sent850' | 'InTransit856' | 'Received';
  createdAt: string;
  shippedAt?: string;
  trackingNumber?: string;
  raw850Payload?: string;
  raw856Payload?: string;
  items: PurchaseOrderItem[];
}

export interface OrderItem {
  id: number;
  productId: number;
  sku: string;
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface CustomerOrder {
  id: number;
  orderNumber: string;
  customerName: string;
  shipToAddress: string;
  shipToCity: string;
  shipToState: string;
  shipToZip: string;
  orderType: 'Standard' | 'DropShip';
  status: 'Pending' | 'SentToVendor' | 'Shipped' | 'Delivered';
  createdAt: string;
  trackingNumber?: string;
  items: OrderItem[];
}

export interface EdiTransaction {
  id: number;
  transactionSet: string;
  controlNumber: string;
  direction: 'Outbound' | 'Inbound';
  partnerName: string;
  poNumber: string;
  rawPayload: string;
  createdAt: string;
}
