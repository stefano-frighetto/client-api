import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { ClientService } from '../../services/client';
import { Client } from '../../models/client.model';
import { MatDialog } from '@angular/material/dialog';
import { ClientForm } from '../client-form/client-form';

@Component({
  selector: 'app-client-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatSortModule
  ],
  templateUrl: './client-list.html',
  styleUrl: './client-list.css',
  animations: [
    trigger('detailExpand', [
      state('collapsed,void', style({height: '0px', minHeight: '0'})),
      state('expanded', style({height: '*'})),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ],
})
export class ClientList implements OnInit {
  private clientService = inject(ClientService);
  private dialog = inject(MatDialog);

  displayedColumns: string[] = ['id', 'firstName', 'lastName', 'actions'];
  
  dataSource = new MatTableDataSource<Client>([]);
  
  expandedElement: Client | null = null;

  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void {
    this.dataSource.filterPredicate = (data: Client, filter: string) => {
      const searchStr = (data.firstName).toLowerCase();
      const transformedFilter = filter.trim().toLowerCase();      
      return searchStr.includes(transformedFilter);
    };
    this.dataSource.sortingDataAccessor = (item, property) => {
      switch(property) {
        case 'id': return item.clientId;
        case 'firstName': return item.firstName;
        case 'lastName': return item.lastName;
        default: return (item as any)[property];
      }
    };

    this.loadClients();
  }

  loadClients(): void {
    this.clientService.getAll().subscribe({
      next: (data) => {
        this.dataSource.data = data;
        this.dataSource.sort = this.sort;
      },
      error: (err) => console.error('Error loading clients', err)
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  createClient() {
    const dialogRef = this.dialog.open(ClientForm, {
      width: '500px',
      data: null
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.clientService.create(result).subscribe({
          next: () => {
            console.log('Cliente creado con éxito');
            this.loadClients();
          },
          error: (err) => console.error('Error al crear', err)
        });
      }
    });
  }

  editClient(client: Client, event: Event) {
    event.stopPropagation();
    
    const dialogRef = this.dialog.open(ClientForm, {
      width: '400px',
      data: client
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.clientService.update(result.clientId, result).subscribe({
          next: () => {
            console.log('Cliente actualizado');
            this.loadClients();
          },
          error: (err) => console.error('Error al actualizar', err)
        });
      }
    });
  }

  deleteClient(id: number, event: Event) {
    event.stopPropagation();
    if(confirm('¿Estás seguro de borrar este cliente?')) {
      this.clientService.delete(id).subscribe(() => this.loadClients());
    }
  }
}