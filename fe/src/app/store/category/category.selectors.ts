import { createFeatureSelector, createSelector } from "@ngrx/store"
import type { CategoryState } from "./category.reducer"

export const selectCategoryState = createFeatureSelector<CategoryState>("category")

export const selectAllCategories = createSelector(selectCategoryState, (state) => state.categories)

export const selectSelectedCategory = createSelector(selectCategoryState, (state) => state.selectedCategory)

export const selectCategoryLoading = createSelector(selectCategoryState, (state) => state.loading)

export const selectCategoryError = createSelector(selectCategoryState, (state) => state.error)

export const selectCategoryLastUpdated = createSelector(selectCategoryState, (state) => state.lastUpdated)

export const selectCategoriesWithBooks = createSelector(selectAllCategories, (categories) =>
  categories.filter((category) => category.bookCount > 0),
)

export const selectCategoryById = (id: string) =>
  createSelector(selectAllCategories, (categories) => categories.find((category) => category.id === id))
