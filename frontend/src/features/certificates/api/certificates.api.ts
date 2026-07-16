import client from '@/shared/api/client'
import type {
  CertificateDownloadDto,
  CertificateDto,
  CertificateVerificationDto
} from '@/features/certificates/types/certificate.types'

export async function getMyCertificates(): Promise<CertificateDto[]> {
  const response = await client.get('/certificates/my')
  return response.data
}

export async function verifyCertificate(number: string): Promise<CertificateVerificationDto> {
  const response = await client.get(`/certificates/verify/${encodeURIComponent(number)}`)
  return response.data
}

export async function getCertificateDownloadUrl(id: string): Promise<CertificateDownloadDto> {
  const response = await client.get(`/certificates/${id}/download-url`)
  return response.data
}
