import { createAction, props } from "@ngrx/store"
import type { Category, GlobalResponse, ApiError } from "../../models/api-response.model"

// Load Categories Actions
export const loadCategories = createAction("[Category] Load Categories")

export const loadCategoriesSuccess = createAction(
  "[Category] Load Categories Success",
  props<{ response: GlobalResponse<Category[]> }>(),
)

export const loadCategoriesFailure = createAction("[Category] Load Categories Failure", props<{ error: ApiError }>())

// Load Category by ID Actions
export const loadCategoryById = createAction("[Category] Load Category By ID", props<{ id: string }>())

export const loadCategoryByIdSuccess = createAction(
  "[Category] Load Category By ID Success",
  props<{ response: GlobalResponse<Category> }>(),
)

export const loadCategoryByIdFailure = createAction(
  "[Category] Load Category By ID Failure",
  props<{ error: ApiError }>(),
)

// Clear Category State
export const clearCategoryState = createAction("[Category] Clear State")
