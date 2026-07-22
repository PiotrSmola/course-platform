import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import {
  getAdminUsers,
  assignUserRole,
  getAdminCourses,
  updateCourseStatus,
  deleteReview,
  getAdminReviews,
  getAdminAuditLogs,
  getAdminCoupons,
  createAdminCoupon,
  updateAdminCoupon,
  deleteAdminCoupon
} from '@/features/admin/api/admin.api'
import { queryKeys } from '@/shared/queryKeys'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'
import type { CourseStatus } from '@/features/courses/types/course.types'
import type {
  CreateCouponRequest,
  UpdateCouponRequest
} from '@/features/admin/types/coupon.types'

export function useAdminUsers() {
  return useQuery({
    queryKey: queryKeys.adminUsers(),
    queryFn: getAdminUsers
  })
}

export function useAdminCourses() {
  return useQuery({
    queryKey: queryKeys.adminCourses(),
    queryFn: () => getAdminCourses()
  })
}

export function useAdminReviews() {
  return useQuery({
    queryKey: queryKeys.adminReviews(),
    queryFn: getAdminReviews
  })
}

export function useAdminAuditLogs(pageNumber: MaybeRefOrGetter<number> = 1, pageSize: MaybeRefOrGetter<number> = 50) {
  return useQuery({
    queryKey: computed(() => queryKeys.adminAuditLogs(toValue(pageNumber), toValue(pageSize))),
    queryFn: () => getAdminAuditLogs(toValue(pageNumber), toValue(pageSize))
  })
}

export function useAdminCoupons() {
  return useQuery({
    queryKey: queryKeys.adminCoupons(),
    queryFn: getAdminCoupons
  })
}

export function useAdminMutations() {
  const queryClient = useQueryClient()

  const assignRole = useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: string }) => assignUserRole(userId, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.adminUsers() })
      toast.success('Rola została przypisana')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się przypisać roli')
  })

  const setCourseStatus = useMutation({
    mutationFn: ({ courseId, status }: { courseId: string; status: CourseStatus }) =>
      updateCourseStatus(courseId, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.adminCourses() })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses() })
      queryClient.invalidateQueries({ queryKey: queryKeys.coursesBrowseAll() })
      toast.success('Status kursu został zmieniony')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się zmienić statusu')
  })

  const removeReview = useMutation({
    mutationFn: (reviewId: string) => deleteReview(reviewId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.adminReviews() })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses() })
      queryClient.invalidateQueries({ queryKey: queryKeys.coursesBrowseAll() })
      toast.success('Recenzja została usunięta')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się usunąć recenzji')
  })

  const createCoupon = useMutation({
    mutationFn: (data: CreateCouponRequest) => createAdminCoupon(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.adminCoupons() })
      toast.success('Kupon utworzony')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się utworzyć kuponu')
  })

  const updateCoupon = useMutation({
    mutationFn: ({ couponId, data }: { couponId: string; data: UpdateCouponRequest }) =>
      updateAdminCoupon(couponId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.adminCoupons() })
      toast.success('Kupon zaktualizowany')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się zaktualizować kuponu')
  })

  const removeCoupon = useMutation({
    mutationFn: (couponId: string) => deleteAdminCoupon(couponId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.adminCoupons() })
      toast.success('Kupon usunięty')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się usunąć kuponu')
  })

  return {
    assignRole,
    setCourseStatus,
    removeReview,
    createCoupon,
    updateCoupon,
    removeCoupon
  }
}
