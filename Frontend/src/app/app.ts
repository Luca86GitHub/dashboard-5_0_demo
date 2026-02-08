import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { interval, of, Subscription } from 'rxjs';
import { catchError, concatMap, distinctUntilChanged, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './app.html',
  styleUrls: ['./app.scss'],
  
})
export class AppComponent implements OnInit, OnDestroy {
  laserData = null;
  sisxtyData = null;

  // Due segnali distinti per i log
  laserLog = signal<any[]>([]);
  sixtyLog = signal<any[]>([]);

//  dashboardData: any = null;
  dashboardData = signal<any>(null);
  historyLog = signal<any[]>([]);
  counter = signal(0);

  // Lista per lo storico eventi (lo riempiamo noi lato frontend per la demo)
  private lastLaserJob = '';
  private lastSixtyJob = '';
  private subscription!: Subscription;
  private apiUrl = 'https://refactored-space-capybara-5g9rxp74v75hv56j-5064.app.github.dev/api/dashboard';


  constructor(private http: HttpClient) {}

  ngOnInit() {


    // Portiamo il polling a 2000ms per sicurezza con concatMap
    this.subscription = interval(1000).pipe(
      // 1. Facciamo la chiamata
      switchMap(() => this.http.get(this.apiUrl).pipe(
        catchError(() => of(null)) // Se fallisce, emette null
      )),
      // 2. Filtriamo i dati: se sono identici a prima, non far passare nulla nel 'next'
      distinctUntilChanged((prev, curr) => {
        // Confrontiamo le stringhe JSON: se sono uguali, blocca l'aggiornamento
        return JSON.stringify(prev) === JSON.stringify(curr);
      })
    ).subscribe({
      next: (data: any) => {
        // Protezione: se data è null, esci subito
        if (!data || !data.laser || !data.sixty) return;

//          this.counter = this.counter + 1;

        console.log('arrivati dati', data, new Date());

          this.counter.update(v => v + 1);

        this.dashboardData.set(data);
        // Creiamo nuovi riferimenti per TUTTI i livelli che ci interessano
/*    this.dashboardData = {
      ...data,
      laser: { ...data.laser },
      sixty: { ...data.sixty }
    };*/
    
    // Passiamo i dati alla funzione dello storico
//        this.addHistoryLog(data);
        // Aggiorniamo i log separatamente
        this.updateMachineLogs(data);
  //   this.cdr.markForCheck();
/*
        console.log('arrivati dati', data);
        this.laserData = data.laser;
       // const sixty = data.sixty;
        this.dashboardData = {...data};
        console.log(this.dashboardData, 'this datshboard data', new Date());
        */
        /*
        if (data) {
          this.dashboardData = data;
          this.checkHistory(data);
        }
        */
      }
    });
  }

  private updateMachineLogs(data: any) {
    const time = new Date().toLocaleTimeString();

    // Log Laser
    const laserEntry = {
      ora: time,
      stato: data.laser?.stato,
      info: `Job: ${data.laser?.commessaAttuale} | ${Math.round(data.laser?.potenzaWatt)}W`
    };
    this.laserLog.update(logs => [laserEntry, ...logs].slice(0, 50));

    // Log Sixty
    const sixtyEntry = {
      ora: time,
      stato: data.sixty?.stato,
      info: `Ricetta: ${data.sixty?.ricettaAttiva} | Ang: ${data.sixty?.angoloCurvatura}°`
    };
    this.sixtyLog.update(logs => [sixtyEntry, ...logs].slice(0, 50));
  }
  
  private addHistoryLog(data: any) {
    if (!data) return;

    const time = new Date().toLocaleTimeString();
    
    // Creiamo una entry per il Laser
    const laserEntry = {
      ora: time,
      macchina: 'LASER LT5',
      evento: data.laser?.stato || 'OFFLINE',
      dettaglio: `Job: ${data.laser?.commessaAttuale} | Pwr: ${Math.round(data.laser?.potenzaWatt)}W`
    };

    // Creiamo una entry per la Sixty
    const sixtyEntry = {
      ora: time,
      macchina: 'SIXTY',
      evento: data.sixty?.stato || 'OFFLINE',
      dettaglio: `Ricetta: ${data.sixty?.ricettaAttiva} | Angolo: ${data.sixty?.angoloCurvatura}°`
    };

    // Aggiorniamo lo storico mettendo entrambi in cima
    // Aumentiamo il limite a 50 per vedere più storia
    this.historyLog.update(logs => [laserEntry, sixtyEntry, ...logs].slice(0, 50));
  }

  ngOnDestroy() {
    if (this.subscription) this.subscription.unsubscribe();
  }
}