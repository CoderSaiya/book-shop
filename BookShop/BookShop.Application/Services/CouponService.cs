using BookShop.Application.DTOs.Req;
using BookShop.Application.DTOs.Res;
using BookShop.Application.Interface;
using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;
using BookShop.Domain.ValueObjects;

namespace BookShop.Application.Services;

public class CouponService(IUnitOfWork uow) : ICouponService
{
    public async Task<CouponRes> GrantAsync(CreateCouponReq req)
    {
        var code = (req.Code ?? "").Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required.", nameof(req.Code));

        // Validate value theo type
        if (req.Type == CouponType.Percentage)
        {
            if (req.Value <= 0 || req.Value > 100)
                throw new ArgumentException("Percentage value must be in (0, 100].", nameof(req.Value));
        }
        else
        {
            if (req.Value <= 0)
                throw new ArgumentException("Value must be > 0.", nameof(req.Value));
        }

        if (req.StartsAt is not null && req.ExpiresAt is not null && req.StartsAt >= req.ExpiresAt)
            throw new ArgumentException("StartsAt must be before ExpiresAt.");
        
        var dup = await uow.Coupons.GetByCodeAsync(code);
        if (dup is not null)
            throw new InvalidOperationException("Code already exists.");

        var c = new Coupon
        {
            UserId = req.UserId,
            Code = code,
            Type = req.Type,
            Value = req.Value,
            MaxDiscountAmount = req.MaxDiscountAmount,
            MinSubtotal = req.MinSubtotal,
            StartsAt = req.StartsAt,
            ExpiresAt = req.ExpiresAt,
            IsActive = req.IsActive
        };

        await uow.Coupons.AddAsync(c);
        await uow.SaveAsync();
        return Map(c);
    }

    public async Task<IReadOnlyList<CouponRes>> ListMineAsync(Guid userId, bool includeUsed = true) =>
        (await uow.Coupons
            .GetByUserAsync(userId, includeUsed, includeInactive: false))
        .Select(Map)
        .ToList();

    public async Task<ValidateCouponRes> ValidateAsync(Guid userId, ValidateCouponReq req)
    {
        var code = (req.Code ?? "").Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(code)) 
            return new(false, "Mã trống.", 0);
        if (req.Subtotal < 0) 
            return new(false, "Subtotal không hợp lệ.", 0);

        var c = await uow.Coupons.GetByUserAndCodeAsync(userId, code);
        if (c is null) 
            return new(false, "Mã không tồn tại.", 0);
        if (!c.IsActive) 
            return new(false, "Mã đang không hoạt động.", 0);
        if (c.IsUsed) 
            return new(false, "Mã đã được sử dụng.", 0);

        var now = DateTime.UtcNow;
        if (c.StartsAt is not null && now < c.StartsAt.Value) 
            return new(false, "Mã chưa bắt đầu.", 0);
        if (c.ExpiresAt is not null && now > c.ExpiresAt.Value) 
            return new(false, "Mã đã hết hạn.", 0);
        if (c.MinSubtotal is not null && req.Subtotal < c.MinSubtotal.Value)
            return new(false, $"Đơn tối thiểu {c.MinSubtotal.Value:N0}đ.", 0);

        var discount = c.Type == CouponType.Percentage
            ? Math.Round(req.Subtotal * (c.Value / 100m), 2, MidpointRounding.AwayFromZero)
            : c.Value;

        if (c.MaxDiscountAmount is not null)
            discount = Math.Min(discount, c.MaxDiscountAmount.Value);
        discount = Math.Clamp(discount, 0m, req.Subtotal);

        if (discount <= 0) 
            return new(false, "Mã không tạo ra giảm giá.", 0);

        return new(true, "Áp mã hợp lệ.", discount);
    }

    public async Task UseAsync(Guid userId, string code, string? context = null)
    {
        var c = await uow.Coupons.GetByUserAndCodeAsync(userId, code.Trim().ToUpperInvariant(), tracking: true)
                ?? throw new KeyNotFoundException("Mã không tồn tại.");
        
        if (!c.IsActive) 
            throw new InvalidOperationException("Mã không hoạt động.");
        if (c.IsUsed) 
            throw new InvalidOperationException("Mã đã được sử dụng.");
        var now = DateTime.UtcNow;
        if (c.StartsAt is not null && now < c.StartsAt.Value) 
            throw new InvalidOperationException("Mã chưa bắt đầu.");
        if (c.ExpiresAt is not null && now > c.ExpiresAt.Value) 
            throw new InvalidOperationException("Mã đã hết hạn.");

        c.IsUsed = true;
        c.UsedAt = DateTime.UtcNow;
        c.UsedContext = context;
        await uow.SaveAsync();
    }

    public async Task<IReadOnlyList<EligibleCouponRes>> ListEligibleAsync(Guid userId, decimal subtotal)
    {
        var all = await uow.Coupons.GetByUserAsync(userId, includeUsed: false, includeInactive: false);
        var now = DateTime.UtcNow;

        var list = new List<EligibleCouponRes>();
        foreach (var c in all)
        {
            if (c.StartsAt is not null && now < c.StartsAt) continue;
            if (c.ExpiresAt is not null && now > c.ExpiresAt) continue;
            if (c.MinSubtotal is not null && subtotal < c.MinSubtotal.Value) continue;

            var discount = c.Type == CouponType.Percentage
                ? Math.Round(subtotal * (c.Value / 100m), 2, MidpointRounding.AwayFromZero)
                : c.Value;

            if (c.MaxDiscountAmount is not null) discount = Math.Min(discount, c.MaxDiscountAmount.Value);
            discount = Math.Clamp(discount, 0m, subtotal);
            if (discount <= 0) continue;

            list.Add(new EligibleCouponRes(
                c.Id, c.Code, c.Type.ToString(), c.Value,
                c.MaxDiscountAmount, c.MinSubtotal, c.StartsAt, c.ExpiresAt,
                discount, "Eligible"
            ));
        }
        
        return list
            .OrderByDescending(x => x.Discount)
            .ToList();
    }

    private static CouponRes Map(Coupon c) => new(
        c.Id,
        c.Code,
        c.Type.ToString(),
        c.Value,
        c.MaxDiscountAmount,
        c.MinSubtotal,
        c.StartsAt,
        c.ExpiresAt,
        c.IsUsed,
        c.UsedAt,
        c.IsActive,
        c.CreatedAt
    );
}