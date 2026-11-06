export type UserRole = 'QualityLead' | 'Member';

export interface CurrentUser {
  readonly id: string;
  readonly email: string;
  readonly displayName: string;
  readonly role: UserRole;
  readonly providerId: string;
}

export interface LoginRequest {
  readonly email: string;
  readonly password: string;
}

/** Sign-up creates a provider together with its first quality lead. */
export interface SignUpRequest {
  readonly providerName: string;
  readonly registrationNumber: string;
  readonly activityCategories: readonly string[];
  readonly displayName: string;
  readonly email: string;
  readonly password: string;
  readonly confirmPassword: string;
}

export interface ChangePasswordRequest {
  readonly currentPassword: string;
  readonly newPassword: string;
  readonly confirmPassword: string;
}

export interface LoginResponse {
  readonly accessToken: string;
  readonly expiresAtUtc: string;
  readonly user: CurrentUser;
}
