import {inject, Injectable} from '@angular/core';
import {BehaviorSubject, of} from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { v4 as uuidv4 } from 'uuid';
import { environment } from '../../../environments/environment';
import { CartService } from './cart.service';
import {Book} from '../../models/book.model';
import {BookService} from './book.service';
import {catchError, switchMap} from 'rxjs/operators';

export interface ChatBook {
  id: string;
  title: {
    vi: string;
    en: string;
  };
  price: number;
  images: string[];
}

export interface ChatAction {
  type: string; // "AddToCart", "RemoveFromCart", ...
  payload: any;
}

export interface ChatMessage {
  id: string;
  text: string;
  isUser: boolean;
  timestamp: Date;
  // metadata (khi là Bot)
  intent?: string;
  confidence?: number;
  books?: ChatBook[];
  // nếu là message gợi ý action
  pendingActions?: ChatAction[];
}

@Injectable({ providedIn: 'root' })
export class ChatbotService {
  private hub: signalR.HubConnection | null = null;
  private sessionId: string;

  private messagesSubject = new BehaviorSubject<ChatMessage[]>([]);
  messages$ = this.messagesSubject.asObservable();

  private cart = inject(CartService);
  private bookService = inject(BookService);

  constructor() {
    // Tạo/lưu sessionId để FE reconnect vẫn giữ phiên chat
    const saved = localStorage.getItem('chat_session_id');
    this.sessionId = saved ?? uuidv4();
    localStorage.setItem('chat_session_id', this.sessionId);

    // Tin nhắn mở đầu
    this.pushBot(
      'Xin chào! Tôi có thể gợi ý sách theo thể loại/giá. Bạn cứ nói "gợi ý sách trinh thám ~150k" hoặc "thêm 2 cuốn Sherlock vào giỏ"...'
    );
  }

  /** Kết nối Hub (gọi 1 lần) */
  async connect(): Promise<void> {
    if (this.hub) return;

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/chat?sessionId=${this.sessionId}`)
      .withAutomaticReconnect()
      .build();

    // Tin nhắn từ server
    this.hub.on('ReceiveMessage', (payload: any) => {
      const isUser = String(payload?.senderType || '').toLowerCase() === 'user';

      const msg: ChatMessage = {
        id: payload?.id ?? uuidv4(),
        text: payload?.content ?? '',
        isUser,
        timestamp: payload?.createdAt ? new Date(payload.createdAt) : new Date(),
        intent: payload?.intent,
        confidence: payload?.confidence,
        books: payload?.books ?? undefined,
      };

      console.log(JSON.stringify(msg));
      this.push(msg);
    });

    // Sự kiện hành động
    this.hub.on('ReceiveAction', (actions: ChatAction[] | any) => {
      const list: ChatAction[] = Array.isArray(actions) ? actions : [actions];
      // tạo 1 message bot chứa các action chờ xác nhận
      const text = 'Mình có hành động đề xuất, bạn xác nhận nhé?';
      const msg: ChatMessage = {
        id: uuidv4(),
        text,
        isUser: false,
        timestamp: new Date(),
        pendingActions: list
      };
      this.push(msg);
    });

    await this.hub.start();
  }

  disconnect(): void {
    this.hub?.stop();
    this.hub = null;
  }

  /** Gửi tin của user tới hub */
  async sendMessage(text: string): Promise<void> {
    if (!this.hub) await this.connect();

    const userMessage: ChatMessage = {
      id: uuidv4(),
      text,
      isUser: true,
      timestamp: new Date(),
    };
    this.push(userMessage);

    // invoke tới backend
    await this.hub!.invoke('SendMessageAsync', this.getSession(), 'User', text);
  }

  /** Xác nhận 1 action từ bot */
  async confirmAction(action: ChatAction): Promise<void> {
    switch (action.type) {
      case 'AddToCart': {
        const { bookId, quantity = 1, unitPrice } = action.payload || {};
        this.fetchBookOrFallback(bookId, unitPrice)
          .pipe(switchMap(book => this.cart.addOrUpdateItem(book, quantity)))
          .subscribe({
            next: () => this.pushBot(`Đã thêm ${quantity} x #${bookId} vào giỏ.`),
            error: () => this.pushBot('Thêm vào giỏ thất bại. Vui lòng thử lại.')
          });
        break;
      }

      case 'RemoveFromCart': {
        const { bookId } = action.payload || {};
        try {
          this.cart.removeItem(bookId);
          this.pushBot(`Đã xoá khỏi giỏ: #${bookId}.`);
        } catch (e) {
          this.pushBot('Xin lỗi, xoá khỏi giỏ thất bại.');
        }
        break;
      }

      default:
        console.warn('Unknown action:', action);
        this.pushBot('Hiện tại mình chưa hỗ trợ hành động này trên FE.');
        break;
    }
  }

  /** Click nhanh trên thẻ product */
  quickAdd(b: ChatBook) {
    this.fetchBookOrFallback(b.id, b.price)
      .pipe(switchMap(book => this.cart.addOrUpdateItem(book, 1)))
      .subscribe({
        next: () => this.pushBot(`Đã thêm 1 x ${b.title} vào giỏ.`),
        error: () => this.pushBot('Thêm vào giỏ thất bại.')
      });
  }

  /** Helpers */
  private getSession() { return this.sessionId; }

  private fetchBookOrFallback(bookId: string, unitPrice?: number) {
    return this.bookService.getBookById(bookId).pipe(
      catchError(() => {
        const fallback: Book = {
          bookId,
          authorName: '',
          publisherName: '',
          title: { vi: 'Sản phẩm', en: 'Product' },
          description: { vi: '', en: '' },
          stock: 0, price: unitPrice ?? 0, sale: 0,
          currentPrice: unitPrice ?? 0,
          images: [], publishedDate: '', isSold: false,
          category: { id: '', name: { vi: '', en: '' } }
        };
        return of(fallback);
      })
    );
  }

  private push(msg: ChatMessage) {
    const arr = this.messagesSubject.value.slice();
    arr.push(msg);
    this.messagesSubject.next(arr);
  }

  private pushBot(text: string) {
    this.push({
      id: uuidv4(),
      text,
      isUser: false,
      timestamp: new Date(),
    });
  }
}
