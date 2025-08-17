export interface GlobalResponse<T> {
    data: T
    message: string
    statusCode: number
    isSuccess: boolean
}

export interface Category {
  id: string
  name: {
    vi: string
    en: string
  }
  description: {
    vi: string
    en: string
  }
  icon: string | null
  bookCount: number
  createdAt: string
}

export interface ApiError {
    message: string
    statusCode: number
}

