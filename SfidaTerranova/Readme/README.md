## Come comunicano ASP.net core (server) e Angular (Fornt end)

server = ASP.Net
frontend = Angular

1) (Server) nella cartella dei controllers trovermo un file (WeatherForecastController.cs) che fornisce i dati json che possono essere consumati da angular
``` cs
using Microsoft.AspNetCore.Mvc;

namespace WebAppAngularNet.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
```
2) (Server) invece nel file (appsettings.json) contiene configurazioni, come la porta su cui il backend è in ascolto.

3) (Server) (Program.cs) Configura i servizi di ASP.NET Core, inclusa la politica CORS per permettere ad Angular di comunicare con il backend.

4) (Frontend) (proxy.conf.js) questo file è usato per evitare problemi di CORS durante lo sviluppo, indirizzando automaticamente le richieste API al backend.
``` js
const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
    env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:7035';

const PROXY_CONFIG = [
  {
    context: [
      "/weatherforecast",
    ],
    target,
    secure: false
  }
]

module.exports = PROXY_CONFIG;
```

5) (FrontEnd) (app/services/api.service.ts (creato manualmente)) questo è un servizio Angular per consumare le API del backend
``` ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = '/weatherforecast'; // Grazie al proxy, viene inoltrata a http://localhost:5000/weatherforecast

  constructor(private http: HttpClient) {}

  getWeather(): Observable<any> {
    return this.http.get(this.apiUrl);
  }
}
```

6) (FrontEnd) (src/app/app.component.ts) usa il servizio per recuperare i dati dal backend.
```ts
import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  public forecasts: WeatherForecast[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.getForecasts();
  }

  getForecasts() {
    this.http.get<WeatherForecast[]>('/weatherforecast').subscribe(
      (result) => {
        this.forecasts = result;
      },
      (error) => {
        console.error(error);
      }
    );
  }

  title = 'webappangularnet.client';
}

``` 

--- 
Curiosità 
- CORS = 
``` 
CORS sta per Cross-Origin Resource Sharing (Condivisione delle Risorse tra Origini Diverse) ed è un meccanismo di sicurezza implementato nei browser per limitare le richieste HTTP tra siti con origini diverse. 

Cosa significa "origini diverse"?
Un'origine (origin) è definita da:

Protocollo (es: http:// o https://)
Dominio (es: example.com)
Porta (es: :4200 per Angular in sviluppo, :5000 per ASP.NET Core)
Due URL hanno origini diverse se almeno uno di questi elementi è differente.

es: 
http://localhost:5000/api/data  (Backend)
http://localhost:5000/app       (Frontend)

``` 
