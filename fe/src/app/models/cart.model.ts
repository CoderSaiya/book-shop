import {Book} from './book.model';

export interface CartRes {
  id: string;
  userId: string;
  isActive: boolean;
  totalAmount: number;
  createdAt: string;
  updatedAt?: string | null;
  items: CartItemDto[];
}

export interface CartItemDto {
  bookId: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface AddCartItemReq {
  bookId: string;
  quantity: number;
  unitPrice: number;
}

// View model dùng cho template hiện tại (giữ nguyên shape cũ)
export interface CombinedCartItem {
  book: Book;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}
