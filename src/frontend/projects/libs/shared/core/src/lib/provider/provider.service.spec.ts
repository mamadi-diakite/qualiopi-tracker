import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideAppEnvironment } from '@qualiopi-tracker/shared/environment';
import { firstValueFrom } from 'rxjs';
import { ProviderProfile, ProviderService } from './provider.service';

const profile: ProviderProfile = {
  id: 'provider-1',
  name: 'Horizon Skills Academy',
  registrationNumber: '12345678901',
  activityCategories: ['TrainingCourses', 'Apprenticeship'],
};

describe('ProviderService', () => {
  let service: ProviderService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideAppEnvironment({ apiBaseUrl: '/api' }),
      ],
    });
    service = TestBed.inject(ProviderService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('reads the provider profile', async () => {
    const result = firstValueFrom(service.get());

    const request = http.expectOne('/api/provider');
    expect(request.request.method).toBe('GET');
    request.flush(profile);

    expect(await result).toEqual(profile);
  });

  it('saves the provider profile', async () => {
    const body = {
      name: 'Horizon',
      registrationNumber: '',
      activityCategories: ['Apprenticeship'],
    };
    const result = firstValueFrom(service.update(body));

    const request = http.expectOne('/api/provider');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(body);
    request.flush(profile);

    expect(await result).toEqual(profile);
  });
});
