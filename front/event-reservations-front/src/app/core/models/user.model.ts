export interface User {
  id: number;
  username: string;
  email: string;
  role: 'User' | 'Admin' | 'Organizer';
}