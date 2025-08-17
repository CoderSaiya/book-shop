import { createFeatureSelector, createSelector } from "@ngrx/store"
import type { BookState } from "./book.reducer"

export const selectBookState = createFeatureSelector<BookState>("book")

// Search selectors
export const selectSearchResults = createSelector(selectBookState, (state: BookState) => state.searchResults)

export const selectSearchKeyword = createSelector(selectBookState, (state: BookState) => state.searchKeyword)

export const selectSearchLoading = createSelector(selectBookState, (state: BookState) => state.searchLoading)

export const selectSearchError = createSelector(selectBookState, (state: BookState) => state.searchError)

export const selectSearchPage = createSelector(selectBookState, (state: BookState) => state.searchPage)

// Trending books selectors
export const selectTrendingBooks = createSelector(selectBookState, (state: BookState) => state.trendingBooks)

export const selectTrendingLoading = createSelector(selectBookState, (state: BookState) => state.trendingLoading)

export const selectTrendingError = createSelector(selectBookState, (state: BookState) => state.trendingError)

// Combined selectors
export const selectBookLoadingState = createSelector(
  selectSearchLoading,
  selectTrendingLoading,
  (searchLoading, trendingLoading) => ({
    searchLoading,
    trendingLoading,
    isAnyLoading: searchLoading || trendingLoading,
  }),
)
