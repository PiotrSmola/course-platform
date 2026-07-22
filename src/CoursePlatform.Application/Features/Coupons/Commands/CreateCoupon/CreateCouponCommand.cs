using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Domain.Enums;

namespace CoursePlatform.Application.Features.Coupons.Commands.CreateCoupon;

public record CreateCouponCommand(
    string Code,
    DiscountType DiscountType,
    decimal Value,
    DateTime StartsAt,
    DateTime? ExpiresAt,
    int? MaxRedemptions,
    bool IsActive,
    IReadOnlyList<Guid>? CourseIds) : IRequest<Guid>;

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateCouponCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();

        var exists = await _context.Coupons.AnyAsync(c => c.Code == code, cancellationToken);
        if (exists)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("Code", "Kod rabatowy już istnieje.")
            });
        }

        var courseIds = request.CourseIds?.Distinct().ToList() ?? new List<Guid>();
        if (courseIds.Count > 0)
        {
            var validCount = await _context.Courses
                .CountAsync(c => courseIds.Contains(c.Id), cancellationToken);

            if (validCount != courseIds.Count)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("CourseIds", "Jeden lub więcej kursów nie istnieje.")
                });
            }
        }

        var coupon = new Coupon
        {
            Code = code,
            DiscountType = request.DiscountType,
            Value = request.Value,
            StartsAt = request.StartsAt,
            ExpiresAt = request.ExpiresAt,
            MaxRedemptions = request.MaxRedemptions,
            IsActive = request.IsActive,
            RedeemedCount = 0,
            CouponCourses = courseIds.Select(id => new CouponCourse { CourseId = id }).ToList()
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(cancellationToken);
        return coupon.Id;
    }
}
