import { TestBed } from '@angular/core/testing';

import { AppInfo } from './app-info';

describe('AppInfo', () => {
  let service: AppInfo;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AppInfo);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
