import { Injectable, inject } from "@angular/core"
import { Actions, createEffect, ofType } from "@ngrx/effects"
import { of } from "rxjs"
import { map, catchError, switchMap } from "rxjs/operators"
import { BookService } from "../../core/services/book.service"
import * as BookActions from "./book.actions"

@Injectable()
export class BookEffects {
  private actions$ = inject(Actions)
  private bookService = inject(BookService)

  searchBooks$ = createEffect(() =>
    this.actions$.pipe(
      ofType(BookActions.searchBooks),
      switchMap(({ keyword, page, pageSize }) =>
        this.bookService.searchBooks({ keyword, page, pageSize }).pipe(
          map((books) =>
            BookActions.searchBooksSuccess({
              books,
              keyword,
              page,
            }),
          ),
          catchError((error) =>
            of(
              BookActions.searchBooksFailure({
                error: error.message || "Failed to search books",
              }),
            ),
          ),
        ),
      ),
    ),
  )

  loadTrendingBooks$ = createEffect(() =>
    this.actions$.pipe(
      ofType(BookActions.loadTrendingBooks),
      switchMap(({ days = 30, limit = 12 }) =>
        this.bookService.getTrendingBooks({ days, limit }).pipe(
          map((books) => BookActions.loadTrendingBooksSuccess({ books })),
          catchError((error) =>
            of(
              BookActions.loadTrendingBooksFailure({
                error: error.message || "Failed to load trending books",
              }),
            ),
          ),
        ),
      ),
    ),
  )
}
