import type { CategoryState } from "./category/category.reducer"
import type { BookState } from "./book/book.reducer"

export interface AppState {
  category: CategoryState
  book: BookState
}
