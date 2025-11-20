export interface Client {
  clientId: number;
  firstName: string;
  lastName: string;
  corporateName: string;
  cuit: string;
  birthdate: string;
  cellPhone: string;
  email: string;
}

export interface CreateClientDto {
  firstName: string;
  lastName: string;
  corporateName: string;
  cuit: string;
  birthdate: string;
  cellPhone: string;
  email: string;
}