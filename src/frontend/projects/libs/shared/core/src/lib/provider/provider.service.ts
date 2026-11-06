import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { APP_ENVIRONMENT } from '@qualiopi-tracker/shared/environment';
import { Observable } from 'rxjs';

export interface ProviderProfile {
  readonly id: string;
  readonly name: string;
  readonly registrationNumber: string | null;
  readonly activityCategories: readonly string[];
}

export interface UpdateProviderProfileRequest {
  readonly name: string;
  readonly registrationNumber: string;
  readonly activityCategories: readonly string[];
}

@Injectable({ providedIn: 'root' })
export class ProviderService {
  private readonly http = inject(HttpClient);
  private readonly url = `${inject(APP_ENVIRONMENT).apiBaseUrl}/provider`;

  get(): Observable<ProviderProfile> {
    return this.http.get<ProviderProfile>(this.url);
  }

  update(request: UpdateProviderProfileRequest): Observable<ProviderProfile> {
    return this.http.put<ProviderProfile>(this.url, request);
  }
}
