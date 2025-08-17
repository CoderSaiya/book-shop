import { createAction, props } from "@ngrx/store"
import type { Book } from "../../models/book.model"

// Search Books Actions
export const searchBooks = createAction(
  "[Book] Search Books",
  props<{ keyword: string; page: number; pageSize: number }>(),
)

export const searchBooksSuccess = createAction(
  "[Book] Search Books Success",
  props<{ books: Book[]; keyword: string; page: number }>(),
)

export const searchBooksFailure = createAction("[Book] Search Books Failure", props<{ error: string }>())

// Trending Books Actions
export const loadTrendingBooks = createAction("[Book] Load Trending Books", props<{ days?: number; limit?: number }>())

export const loadTrendingBooksSuccess = createAction("[Book] Load Trending Books Success", props<{ books: Book[] }>())

export const loadTrendingBooksFailure = createAction("[Book] Load Trending Books Failure", props<{ error: string }>())

// Clear Actions
export const clearBooks = createAction("[Book] Clear Books")
export const clearSearchResults = createAction("[Book] Clear Search Results")
