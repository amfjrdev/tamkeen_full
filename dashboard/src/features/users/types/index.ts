export type UserStatus = 'active' | 'blocked' | 'pending';

export interface User {
  id: number;
  name: string;
  email: string;
  initials: string;
  status: UserStatus;
  joined: string;
  requests: number;
}

export interface UsersResponse {
  users: User[];
  navItems: Array<{
    label: string;
    icon: string;
    active: boolean;
  }>;
}
