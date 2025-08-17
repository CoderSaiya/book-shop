import { Injectable, inject } from "@angular/core"
import { HttpClient } from "@angular/common/http"
import { Observable } from "rxjs"
import { map } from "rxjs/operators"
import { environment } from "../../../environments/environment"
import type { Book } from "../../models/book.model"
import type { GlobalResponse } from "../../models/api-response.model"

export interface BookSearchParams {
  keyword?: string
  page?: number
  pageSize?: number
}

export interface TrendingBooksParams {
  days?: number
  limit?: number
}

@Injectable({
  providedIn: "root",
})
export class BookService {
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/api/book`

  searchBooks(params: BookSearchParams = {}): Observable<Book[]> {
    const { keyword = "", page = 1, pageSize = 50 } = params

    return this.http
      .get<GlobalResponse<Book[]>>(`${this.apiUrl}`, {
        params: {
          keyword,
          page: page.toString(),
          pageSize: pageSize.toString(),
        },
      })
      .pipe(map((response) => response.data || []))
  }

  getTrendingBooks(params: TrendingBooksParams = {}): Observable<Book[]> {
    const { days = 30, limit = 12 } = params

    return this.http
      .get<GlobalResponse<Book[]>>(`${this.apiUrl}/trending`, {
        params: {
          days: days.toString(),
          limit: limit.toString(),
        },
      })
      .pipe(map((response) => response.data || []))
  }

  getFeaturedBooks(): Observable<Book[]> {
    return this.getTrendingBooks({ limit: 8 })
  }

  getBookById(id: string): Observable<Book> {
    return this.http
      .get<GlobalResponse<Book>>(`${this.apiUrl}/${id}`)
      .pipe(map((response) => response.data || null))
  }

  getBookText(book: Book, field: "title" | "description", language: "vi" | "en" = "vi"): string {
    return book[field][language] || book[field]["vi"] || ""
  }

  getCategoryText(category: Book["category"], language: "vi" | "en" = "vi"): string {
    return category.name[language] || category.name["vi"] || ""
  }

  searchBooksByKeyword(keyword: string, page = 1): Observable<Book[]> {
    return this.searchBooks({ keyword, page, pageSize: 20 })
  }

  getBooksByCategory(categoryId: string, page = 1): Observable<Book[]> {
    // This would need a separate API endpoint
    return this.searchBooks({ page, pageSize: 20 })
  }
}
