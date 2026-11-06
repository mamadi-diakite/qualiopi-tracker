import { Component, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { ACTIVITY_CATEGORIES } from '@qualiopi-tracker/shared/core';

/** Form control picking activity categories; its value is the list of API names, in API order. */
@Component({
  selector: 'qt-category-checkboxes',
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: CategoryCheckboxes, multi: true }],
  template: `
    <div class="mt-2 grid gap-2 sm:grid-cols-2">
      @for (category of categories; track category.value) {
        <label class="flex items-center gap-2 text-sm text-slate-700">
          <input
            #box
            type="checkbox"
            class="size-4 accent-teal-600"
            [id]="'category-' + category.value"
            [checked]="selected().includes(category.value)"
            [disabled]="disabled()"
            (change)="toggle(category.value, box.checked)"
          />
          {{ category.label }}
        </label>
      }
    </div>
  `,
})
export class CategoryCheckboxes implements ControlValueAccessor {
  protected readonly categories = ACTIVITY_CATEGORIES;
  protected readonly selected = signal<readonly string[]>([]);
  protected readonly disabled = signal(false);

  private onChange: (value: readonly string[]) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  writeValue(value: readonly string[] | null): void {
    this.selected.set(value ?? []);
  }

  registerOnChange(onChange: (value: readonly string[]) => void): void {
    this.onChange = onChange;
  }

  registerOnTouched(onTouched: () => void): void {
    this.onTouched = onTouched;
  }

  setDisabledState(disabled: boolean): void {
    this.disabled.set(disabled);
  }

  protected toggle(value: string, checked: boolean): void {
    const next = this.categories
      .map((category) => category.value)
      .filter((name) => (name === value ? checked : this.selected().includes(name)));
    this.selected.set(next);
    this.onChange(next);
    this.onTouched();
  }
}
