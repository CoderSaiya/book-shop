export interface Book {
  bookId: string
  authorName: string
  publisherName: string
  title: {
    vi: string
    en: string
  }
  description: {
    vi: string
    en: string
  }
  stock: number
  price: number
  sale: number
  currentPrice: number
  images: string[]
  publishedDate: string
  isSold: boolean
  category: {
    id: string
    name: {
      vi: string
      en: string
    }
  }
  rating?: number
  reviewCount?: number
  featured?: boolean
  bestseller?: boolean
}

export interface BookCategory {
  id: string
  name: string
  slug: string
  description: string
  icon: string
  bookCount: number
}

export interface CartItem {
  book: Book
  quantity: number
}

export interface User {
  userId: string
  email: string
  firstName: string
  lastName: string
  phone: string
  address: string
  avatar?: string
  dob: Date;
}
