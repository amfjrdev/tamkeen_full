export type ClientStatus = 'active' | 'blocked' | 'pending';
export type UserStatus = ClientStatus;

export interface Client {
  id: number;
  userId?: string;
  name: string;
  email: string;
  phone?: string;
  initials: string;
  status: ClientStatus;
  joined: string;
  requests: number;
}

export type User = Client;

export interface ClientsResponse {
  clients?: Client[];
  users: Client[];
  totalCount?: number;
  page?: number;
  pageSize?: number;
  totalPages?: number;
  navItems: Array<{
    label: string;
    icon: string;
    active: boolean;
  }>;
}

export type UsersResponse = ClientsResponse;
