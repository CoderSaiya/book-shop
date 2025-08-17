import { createReducer, on } from "@ngrx/store"
import type { Category, ApiError } from "../../models/api-response.model"
import * as CategoryActions from "./category.actions"

export interface CategoryState {
  categories: Category[]
  selectedCategory: Category | null
  loading: boolean
  error: ApiError | null
  lastUpdated: Date | null
}

export const initialState: CategoryState = {
  categories: [],
  selectedCategory: null,
  loading: false,
  error: null,
  lastUpdated: null,
}

export const categoryReducer = createReducer(
  initialState,

  // Load Categories
  on(CategoryActions.loadCategories, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(CategoryActions.loadCategoriesSuccess, (state, { response }) => ({
    ...state,
    categories: response.data || [],
    loading: false,
    error: null,
    lastUpdated: new Date(),
  })),

  on(CategoryActions.loadCategoriesFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
    categories: [],
  })),

  // Load Category by ID
  on(CategoryActions.loadCategoryById, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(CategoryActions.loadCategoryByIdSuccess, (state, { response }) => ({
    ...state,
    selectedCategory: response.data || null,
    loading: false,
    error: null,
  })),

  on(CategoryActions.loadCategoryByIdFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
    selectedCategory: null,
  })),

  // Clear State
  on(CategoryActions.clearCategoryState, () => initialState),
)
