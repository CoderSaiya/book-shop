export type CouponType = 'Percentage' | 'FixedAmount';

export interface Coupon {
  id: string;
  code: string;
  type: CouponType;
  value: number;
  maxDiscountAmount?: number | null;
  minSubtotal?: number | null;
  startsAtUtc?: string | null;
  expiresAtUtc?: string | null;
  isActive: boolean;
  isUsed: boolean;
}
