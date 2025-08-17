import { createReducer, on } from "@ngrx/store"
import type { Book } from "../../models/book.model"
import * as BookActions from "./book.actions"

export interface BookState {
  // Search state
  searchResults: Book[]
  searchKeyword: string
  searchPage: number
  searchLoading: boolean
  searchError: string | null

  // Trending books state
  trendingBooks: Book[]
  trendingLoading: boolean
  trendingError: string | null
}

export const initialState: BookState = {
  searchResults: [],
  searchKeyword: "",
  searchPage: 1,
  searchLoading: false,
  searchError: null,
  trendingBooks: [],
  trendingLoading: false,
  trendingError: null,
}

export const bookReducer = createReducer(
  initialState,

  // Search Books
  on(BookActions.searchBooks, (state, { keyword, page }) => ({
    ...state,
    searchKeyword: keyword,
    searchPage: page,
    searchLoading: true,
    searchError: null,
  })),

  on(BookActions.searchBooksSuccess, (state, { books, keyword, page }) => ({
    ...state,
    searchResults: page === 1 ? books : [...state.searchResults, ...books],
    searchKeyword: keyword,
    searchPage: page,
    searchLoading: false,
    searchError: null,
  })),

  on(BookActions.searchBooksFailure, (state, { error }) => ({
    ...state,
    searchLoading: false,
    searchError: error,
  })),

  // Trending Books
  on(BookActions.loadTrendingBooks, (state) => ({
    ...state,
    trendingLoading: true,
    trendingError: null,
  })),

  on(BookActions.loadTrendingBooksSuccess, (state, { books }) => ({
    ...state,
    trendingBooks: books,
    trendingLoading: false,
    trendingError: null,
  })),

  on(BookActions.loadTrendingBooksFailure, (state, { error }) => ({
    ...state,
    trendingLoading: false,
    trendingError: error,
  })),

  // Clear Actions
  on(BookActions.clearBooks, () => initialState),

  on(BookActions.clearSearchResults, (state) => ({
    ...state,
    searchResults: [],
    searchKeyword: "",
    searchPage: 1,
    searchError: null,
  })),
)
