import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})

export class AppComponent {
  title = 'Cokatiels';
}

// export class AppComponent implements OnInit {
//   public forecasts: CocktailDebacle[] = [];

//   constructor(private http: HttpClient) {}

//   ngOnInit() {
//     this.getForecasts();
//   }

//   getForecasts() {
//     this.http.get<CocktailDebacle[]>('/CocktailDebacle').subscribe(
//       (result) => {
//         this.forecasts = result;
//       },
//       (error) => {
//         console.error(error);
//       }
//     );
//   }

//   title = 'cocktaildebacle.client';
// }
