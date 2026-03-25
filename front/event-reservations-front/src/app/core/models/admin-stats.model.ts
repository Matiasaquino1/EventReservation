export interface DashboardStats {
  totalUsers: number;
  totalReservations: number;
  totalRevenue: number;
  topEvents: { eventName: string; ticketCount: number }[];
}