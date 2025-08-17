import {inject, Injectable} from "@angular/core"
import { Actions, createEffect, ofType } from "@ngrx/effects"
import { HttpClient, HttpErrorResponse } from "@angular/common/http"
import { of } from "rxjs"
import { map, catchError, switchMap } from "rxjs/operators"
import { environment } from "../../../environments/environment"
import type { GlobalResponse, Category, ApiError } from "../../models/api-response.model"
import * as CategoryActions from "./category.actions"

@Injectable()
export class CategoryEffects {
  private readonly apiUrl = `${environment.apiUrl}/api/category`

  private actions$ = inject(Actions);
  private http = inject(HttpClient);

  loadCategories$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CategoryActions.loadCategories),
      switchMap(() =>
        this.http.get<GlobalResponse<Category[]>>(this.apiUrl).pipe(
          map((response) => CategoryActions.loadCategoriesSuccess({ response })),
          catchError((error: HttpErrorResponse) => {
            const apiError: ApiError = {
              message: error.error?.message || error.message || "Failed to load categories",
              statusCode: error.status || 500,
            }
            return of(CategoryActions.loadCategoriesFailure({ error: apiError }))
          }),
        ),
      ),
    ),
  )

  loadCategoryById$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CategoryActions.loadCategoryById),
      switchMap(({ id }) =>
        this.http.get<GlobalResponse<Category>>(`${this.apiUrl}/${id}`).pipe(
          map((response) => CategoryActions.loadCategoryByIdSuccess({ response })),
          catchError((error: HttpErrorResponse) => {
            const apiError: ApiError = {
              message: error.error?.message || error.message || "Failed to load category",
              statusCode: error.status || 500,
            }
            return of(CategoryActions.loadCategoryByIdFailure({ error: apiError }))
          }),
        ),
      ),
    ),
  )
}
