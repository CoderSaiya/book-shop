import { Routes } from '@angular/router';
import {authGuard} from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: "",
    loadComponent: () => import("./pages/home/home.component").then((m) => m.HomeComponent),
  },
  {
    path: "auth/login",
    loadComponent: () => import("./pages/auth/login/login.component").then((m) => m.LoginComponent),
  },
  {
    path: "auth/register",
    loadComponent: () => import("./pages/auth/register/register.component").then((m) => m.RegisterComponent),
  },
  {
    path: "books",
    loadComponent: () => import("./pages/books/books.component").then((m) => m.BooksComponent),
  },
  {
    path: "books/:id",
    loadComponent: () => import("./pages/book-detail/book-detail.component").then((m) => m.BookDetailComponent),
  },
  {
    path: "cart",
    loadComponent: () => import("./pages/cart/cart.component").then((m) => m.CartComponent),
    canActivate: [authGuard]
  },
  {
    path: "checkout",
    loadComponent: () => import("./pages/checkout/checkout.component").then((m) => m.CheckoutComponent),
    canActivate: [authGuard]
  },
  {
    path: "checkout/success/:id",
    loadComponent: () => import("./pages/checkout/success/checkout-success.component").then((m) => m.CheckoutSuccessComponent),
    canActivate: [authGuard]
  },
  {
    path: "profile",
    loadComponent: () => import("./pages/profile/profile.component").then((m) => m.ProfileComponent),
    canActivate: [authGuard]
  },
  {
    path: "order",
    loadComponent: () => import("./pages/order/orders.component").then((m) => m.OrdersComponent),
    canActivate: [authGuard]
  },
  {
    path: "**",
    redirectTo: "",
  },
];
