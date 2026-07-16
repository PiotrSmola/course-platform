export interface CertificateDto {
  id: string
  number: string
  courseId: string
  courseTitle: string
  issuedAt: string
}

export interface CertificateVerificationDto {
  number: string
  holderName: string
  courseTitle: string
  instructorName: string
  issuedAt: string
}

export interface CertificateDownloadDto {
  url: string
}
