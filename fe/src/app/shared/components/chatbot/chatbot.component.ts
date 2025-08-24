import { Component, inject, OnInit } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { ChatbotService, ChatMessage, ChatBook, ChatAction } from '../../../core/services/chat.service';
import {getLangText} from '../../utils/lang';

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [CommonModule, FormsModule, NgOptimizedImage],
  styleUrls: ['./chatbot.component.scss'],
  template: `
  <div class="chatbot-container">
    <button class="chat-toggle-btn" (click)="toggleChat()" [class.active]="isOpen">
      <svg *ngIf="!isOpen" width="24" height="24" viewBox="0 0 24 24">
        <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
      </svg>
      <svg *ngIf="isOpen" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <line x1="18" y1="6" x2="6" y2="18"/>
        <line x1="6" y1="6" x2="18" y2="18"/>
      </svg>
    </button>

    <div class="chat-window" [class.open]="isOpen">
      <div class="chat-header">
        <h3>Trợ lý ảo</h3>
        <span class="status">Đang hoạt động</span>
      </div>

      <div class="chat-messages" #messagesContainer>
        <ng-container *ngFor="let m of messages$ | async">
          <!-- bubble -->
          <div class="message" [class.user]="m.isUser" [class.bot]="!m.isUser">
            <div class="message-content">
              <p [innerHTML]="m.text"></p>
              <div class="meta" *ngIf="m.intent">
<!--                <small>intent: {{m.intent}} ({{ m.confidence | number:'1.0-2' }})</small>-->
              </div>
              <span class="timestamp">{{ m.timestamp | date:'HH:mm' }}</span>
            </div>
          </div>

          <!-- product cards -->
          <div class="products" *ngIf="m.books?.length">
            <div class="product-card" *ngFor="let b of m.books">
              <img [src]="b.images[0]" [alt]="getLangText(b.title)" width="64" height="96" />
              <div class="info">
                <div class="title">{{ getLangText(b.title) }}</div>
                <div class="price">{{ b.price | number:'1.0-0' }} đ</div>
                <button (click)="quickAdd(b)">Thêm vào giỏ</button>
              </div>
            </div>
          </div>

          <!-- action prompts -->
          <div class="actions" *ngIf="m.pendingActions?.length">
            <div *ngFor="let a of m.pendingActions" class="action">
              <div class="action-text">
                <strong>Hành động:</strong> {{a.type}}
                <pre>{{ a.payload | json }}</pre>
              </div>
              <div class="action-ctas">
                <button (click)="confirm(a)">Xác nhận</button>
              </div>
            </div>
          </div>
        </ng-container>
      </div>

      <div class="chat-input">
        <input type="text" [(ngModel)]="currentMessage" (keyup.enter)="sendMessage()" placeholder="Nhập tin nhắn..." class="message-input">
        <button (click)="sendMessage()" [disabled]="!currentMessage.trim()" class="send-btn">
          <svg width="20" height="20" viewBox="0 0 24 24">
            <line x1="22" y1="2" x2="11" y2="13"/>
            <polygon points="22,2 15,22 11,13 2,9 22,2"/>
          </svg>
        </button>
      </div>
    </div>
  </div>
  `
})
export class ChatbotComponent implements OnInit {
  private chatbotService = inject(ChatbotService);

  isOpen = false;
  currentMessage = '';
  messages$: Observable<ChatMessage[]> = this.chatbotService.messages$;

  async ngOnInit() {
    await this.chatbotService.connect();
  }

  toggleChat(): void {
    this.isOpen = !this.isOpen;
  }

  async sendMessage(): Promise<void> {
    if (this.currentMessage.trim()) {
      await this.chatbotService.sendMessage(this.currentMessage.trim());
      this.currentMessage = '';
    }
  }

  quickAdd(book: ChatBook) {
    this.chatbotService.quickAdd(book);
  }

  async confirm(a: ChatAction) {
    await this.chatbotService.confirmAction(a);
  }

  protected readonly getLangText = getLangText;
}
