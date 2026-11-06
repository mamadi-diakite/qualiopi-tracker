import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { APP_ENVIRONMENT } from '@qualiopi-tracker/shared/environment';
import { Observable } from 'rxjs';

/**
 * `Applicable`, `NotApplicable` (marked by the quality lead) or `OutOfScope` (outside the
 * provider's activity categories).
 */
export type IndicatorStatus = 'Applicable' | 'NotApplicable' | 'OutOfScope';

export interface Indicator {
  readonly number: number;
  readonly criterionNumber: number;
  readonly title: string;
  readonly appliesTo: readonly string[];
  readonly status: IndicatorStatus;
  readonly notApplicableReason: string | null;
  readonly notes: string | null;
}

export interface Criterion {
  readonly number: number;
  readonly title: string;
  readonly indicators: readonly Indicator[];
}

export interface UpdateIndicatorRequest {
  /** A reason marks the indicator as not applicable; `null` makes it applicable again. */
  readonly notApplicableReason: string | null;
  readonly notes: string;
}

@Injectable({ providedIn: 'root' })
export class FrameworkService {
  private readonly http = inject(HttpClient);
  private readonly url = `${inject(APP_ENVIRONMENT).apiBaseUrl}/framework`;

  getCriteria(): Observable<Criterion[]> {
    return this.http.get<Criterion[]>(this.url);
  }

  getIndicator(number: number): Observable<Indicator> {
    return this.http.get<Indicator>(`${this.url}/indicators/${number}`);
  }

  updateIndicator(number: number, request: UpdateIndicatorRequest): Observable<Indicator> {
    return this.http.put<Indicator>(`${this.url}/indicators/${number}`, request);
  }
}
