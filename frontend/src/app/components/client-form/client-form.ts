import { Component, inject, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Client } from '../../models/client.model';

@Component({
  selector: 'app-client-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './client-form.html',
  styleUrl: './client-form.css'
})

export class ClientForm {
  private fb = inject(FormBuilder);
  public dialogRef = inject(MatDialogRef<ClientForm>);
  
  form: FormGroup;
  isEditMode: boolean = false;

  constructor(@Inject(MAT_DIALOG_DATA) public data: Client | null) {
    this.isEditMode = !!data;

    this.form = this.fb.group({
      firstName: [data?.firstName || '', Validators.required],
      lastName: [data?.lastName || '', Validators.required],
      email: [data?.email || '', [Validators.required, Validators.email]],
      cellPhone: [data?.cellPhone || '', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      
      corporateName: [data?.corporateName || '', Validators.required], 
      cuit: [data?.cuit || '', Validators.pattern(/^\d{2}-\d{8}-\d$/)],
      birthdate: [data?.birthdate ? data.birthdate.split('T')[0] : '', Validators.required]
    });
    
    if (this.isEditMode) {
    }
  }

  onSave(): void {
    if (this.form.valid) {
      const result: Client = {
        ...this.data,
        ...this.form.value
      } as Client;
      
      this.dialogRef.close(result);
    }
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }
}