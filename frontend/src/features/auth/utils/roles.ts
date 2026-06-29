const ROLE_ALIASES: Record<string, string> = {
  admin: 'Admin',
  instructor: 'Instructor',
  student: 'Student'
}

export function normalizeRoles(roles: unknown): string[] {
  if (!Array.isArray(roles)) return []
  return roles
    .filter((role): role is string => typeof role === 'string')
    .map((role) => ROLE_ALIASES[role.toLowerCase()] ?? role)
}

export function hasRole(roles: string[] | undefined, role: string): boolean {
  return normalizeRoles(roles).includes(role)
}
