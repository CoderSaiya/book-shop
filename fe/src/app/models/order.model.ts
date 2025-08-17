export interface CreateOrderItemDto {
  bookId: string;
  quantity: number;
  unitPrice: number;
}

export interface CreateOrderReq {
  shippingAddress: string;
  shippingCity: string;
  shippingPostalCode: string;
  shippingPhone: string;
  notes?: string | null;
  paymentMethod: string;
  items: ReadonlyArray<CreateOrderItemDto>;
}

export interface OrderDetailRes {
  id: string;
  orderNumber: string;
  userId: string;
  totalAmount: number;
  status: string;
  paymentStatus: string;
  shippingAddress: string;
  shippingCity: string;
  shippingPostalCode: string;
  shippingPhone: string;
  notes?: string | null;
  createdAt: string;
  shippedAt?: string | null;
  deliveredAt?: string | null;
  paidAt?: string | null;
  items: OrderItemDto[];
}

export interface OrderItemDto {
  bookId: string;
  bookTitle?: string | null;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}
