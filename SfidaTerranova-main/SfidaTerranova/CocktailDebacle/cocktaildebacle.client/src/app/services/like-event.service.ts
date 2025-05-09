import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LikeEventService {
  private likeEventSubject = new Subject<void>();
  
  public likeEvent$ = this.likeEventSubject.asObservable();

  emitLikeEvent() {
    console.log('LikeEventService: emitting like event');
    this.likeEventSubject.next();
  }
}