import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { CategoryCheckboxes } from './category-checkboxes';

@Component({
  imports: [ReactiveFormsModule, CategoryCheckboxes],
  template: `<qt-category-checkboxes [formControl]="categories" />`,
})
class Host {
  readonly categories = new FormControl<readonly string[] | null>(['Apprenticeship']);
}

function box(element: HTMLElement, value: string): HTMLInputElement {
  return element.querySelector<HTMLInputElement>(`#category-${value}`)!;
}

describe('CategoryCheckboxes', () => {
  it('shows the four categories with the selected ones checked', async () => {
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    expect(
      [...element.querySelectorAll('label')].map((label) => label.textContent?.trim()),
    ).toEqual([
      'Training courses',
      'Skills assessments',
      'Accreditation of prior experience (VAE)',
      'Apprenticeship',
    ]);
    expect(box(element, 'Apprenticeship').checked).toBe(true);
    expect(box(element, 'TrainingCourses').checked).toBe(false);
  });

  it('updates the control in API order and marks it touched', async () => {
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;
    const control = fixture.componentInstance.categories;

    box(element, 'TrainingCourses').click();
    await fixture.whenStable();
    expect(control.value).toEqual(['TrainingCourses', 'Apprenticeship']);
    expect(control.touched).toBe(true);

    box(element, 'Apprenticeship').click();
    await fixture.whenStable();
    expect(control.value).toEqual(['TrainingCourses']);
  });

  it('follows the control value and disabled state', async () => {
    const fixture = TestBed.createComponent(Host);
    const control = fixture.componentInstance.categories;
    control.setValue(null);
    control.disable();
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    expect([...element.querySelectorAll('input')].some((input) => input.checked)).toBe(false);
    expect(box(element, 'SkillsAssessments').disabled).toBe(true);
  });

  it('works without a form control', async () => {
    const fixture = TestBed.createComponent(CategoryCheckboxes);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    box(element, 'SkillsAssessments').click();
    await fixture.whenStable();

    expect(box(element, 'SkillsAssessments').checked).toBe(true);
  });
});
