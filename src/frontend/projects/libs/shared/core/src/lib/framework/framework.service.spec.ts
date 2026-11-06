import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideAppEnvironment } from '@qualiopi-tracker/shared/environment';
import { firstValueFrom } from 'rxjs';
import { Criterion, FrameworkService, Indicator } from './framework.service';

const indicator: Indicator = {
  number: 27,
  criterionNumber: 6,
  title: 'Subcontracting',
  appliesTo: ['TrainingCourses', 'Apprenticeship'],
  status: 'NotApplicable',
  notApplicableReason: 'We never subcontract.',
  notes: null,
};

describe('FrameworkService', () => {
  let service: FrameworkService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideAppEnvironment({ apiBaseUrl: '/api' }),
      ],
    });
    service = TestBed.inject(FrameworkService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('reads the criteria with their indicators', async () => {
    const criteria: Criterion[] = [
      { number: 6, title: 'Professional environment', indicators: [indicator] },
    ];
    const result = firstValueFrom(service.getCriteria());

    http.expectOne('/api/framework').flush(criteria);

    expect(await result).toEqual(criteria);
  });

  it('reads one indicator', async () => {
    const result = firstValueFrom(service.getIndicator(27));

    http.expectOne('/api/framework/indicators/27').flush(indicator);

    expect(await result).toEqual(indicator);
  });

  it('saves the settings of an indicator', async () => {
    const body = { notApplicableReason: 'We never subcontract.', notes: 'Owner: Claire.' };
    const result = firstValueFrom(service.updateIndicator(27, body));

    const request = http.expectOne('/api/framework/indicators/27');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(body);
    request.flush(indicator);

    expect(await result).toEqual(indicator);
  });
});
